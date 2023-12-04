#include "stdafx.h"
#include "MeshObject.hpp"

MeshObject::MeshObject(NativeClient& client)
    : SpatialObject(client)
{
    REQUIRE(GetClient().GetDevice() != nullptr);

    m_instanceDataBufferAlignedSize = sizeof InstanceConstantBuffer;
    m_instanceDataBuffer = util::AllocateConstantBuffer(GetClient(), &m_instanceDataBufferAlignedSize);
    NAME_D3D12_OBJECT_WITH_ID(m_instanceDataBuffer);

    m_instanceDataBufferView.BufferLocation = m_instanceDataBuffer.GetGPUVirtualAddress();
    m_instanceDataBufferView.SizeInBytes = static_cast<UINT>(m_instanceDataBufferAlignedSize);

    TRY_DO(m_instanceDataBuffer.Map(&m_instanceConstantBufferMapping, 1));

    {
        m_geometrySRV.Format = DXGI_FORMAT_UNKNOWN;
        m_geometrySRV.ViewDimension = D3D12_SRV_DIMENSION_BUFFER;
        m_geometrySRV.Shader4ComponentMapping = D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING;
        m_geometrySRV.Buffer.FirstElement = 0;
        m_geometrySRV.Buffer.Flags = D3D12_BUFFER_SRV_FLAG_NONE;
    }

    {
        m_geometryUAV.Format = DXGI_FORMAT_UNKNOWN;
        m_geometryUAV.ViewDimension = D3D12_UAV_DIMENSION_BUFFER;
        m_geometryUAV.Buffer.FirstElement = 0;
        m_geometryUAV.Buffer.CounterOffsetInBytes = 0;
        m_geometryUAV.Buffer.Flags = D3D12_BUFFER_UAV_FLAG_NONE;
    }
}

void MeshObject::Initialize(UINT materialIndex)
{
    m_material = &GetClient().GetSpace()->GetMaterial(materialIndex);
    
    Update();
}

void MeshObject::Update()
{
    if (const bool transformDirty = ClearTransformDirty(); !transformDirty) return;

    const DirectX::XMFLOAT4X4 objectToWorld = GetTransform();

    const DirectX::XMMATRIX transform = XMLoadFloat4x4(&objectToWorld);
    const DirectX::XMMATRIX transformNormal = XMMatrixToNormal(transform);

    DirectX::XMFLOAT4X4 objectToWorldNormal = {};
    XMStoreFloat4x4(&objectToWorldNormal, transformNormal);

    m_instanceConstantBufferMapping.Write({
        .objectToWorld = objectToWorld,
        .objectToWorldNormal = objectToWorldNormal
    });
}

void MeshObject::SetEnabledState(const bool enabled)
{
    m_enabled = enabled;
    UpdateActiveState();
}

void MeshObject::SetNewVertices(const SpatialVertex* vertices, const UINT vertexCount)
{
    REQUIRE(!m_uploadEnqueued);
    REQUIRE(GetMaterial().geometryType == D3D12_RAYTRACING_GEOMETRY_TYPE_TRIANGLES);

    const auto vertexBufferSize = sizeof(SpatialVertex) * vertexCount;

    m_geometryElementCount = vertexCount;
    
    UpdateActiveState();
    UpdateGeometryViews(sizeof(SpatialVertex));

    if (m_geometryElementCount == 0)
    {
        m_geometryBufferUpload = {};
        return;
    }

    GetClient().GetSpace()->MarkMeshObjectModified(m_handle.value());
    m_requiresFreshBLAS = true;
    m_uploadRequired = true;

    util::ReAllocateBuffer(&m_geometryBufferUpload,
                           GetClient(), vertexBufferSize,
                           D3D12_RESOURCE_FLAG_NONE,
                           D3D12_RESOURCE_STATE_GENERIC_READ,
                           D3D12_HEAP_TYPE_UPLOAD);
    NAME_D3D12_OBJECT_WITH_ID(m_geometryBufferUpload);

    TRY_DO(util::MapAndWrite(m_geometryBufferUpload, vertices, vertexCount));
}

void MeshObject::SetNewBounds(const SpatialBounds* bounds, const UINT boundsCount)
{
    REQUIRE(!m_uploadEnqueued);
    REQUIRE(GetMaterial().geometryType == D3D12_RAYTRACING_GEOMETRY_TYPE_PROCEDURAL_PRIMITIVE_AABBS);

    const auto vertexBufferSize = sizeof(SpatialBounds) * boundsCount;

    m_geometryElementCount = boundsCount;

    UpdateActiveState();
    UpdateGeometryViews(sizeof(SpatialBounds));

    if (m_geometryElementCount == 0)
    {
        m_geometryBufferUpload = {};
        return;
    }

    GetClient().GetSpace()->MarkMeshObjectModified(m_handle.value());
    m_requiresFreshBLAS = true;
    m_uploadRequired = true;

    util::ReAllocateBuffer(&m_geometryBufferUpload,
                           GetClient(), vertexBufferSize,
                           D3D12_RESOURCE_FLAG_NONE,
                           D3D12_RESOURCE_STATE_GENERIC_READ,
                           D3D12_HEAP_TYPE_UPLOAD);
    NAME_D3D12_OBJECT_WITH_ID(m_geometryBufferUpload);

    TRY_DO(util::MapAndWrite(m_geometryBufferUpload, bounds, boundsCount));
}

std::optional<size_t> MeshObject::GetActiveIndex() const
{
    return m_active;
}

const Material& MeshObject::GetMaterial() const
{
    REQUIRE(m_material != nullptr);
    return *m_material;
}

UINT MeshObject::GetGeometryUnitCount() const
{
    switch (GetMaterial().geometryType)
    {
    case D3D12_RAYTRACING_GEOMETRY_TYPE_TRIANGLES: return m_geometryElementCount / 4;
    case D3D12_RAYTRACING_GEOMETRY_TYPE_PROCEDURAL_PRIMITIVE_AABBS: return m_geometryElementCount;
    default: REQUIRE(false);
        return 0;
    }
}

Allocation<ID3D12Resource> MeshObject::GetGeometryBuffer() const
{
    return const_cast<MeshObject*>(this)->GeometryBuffer();
}

void MeshObject::EnqueueMeshUpload(const ComPtr<ID3D12GraphicsCommandList> commandList)
{
    REQUIRE(m_uploadRequired);
    REQUIRE(!m_uploadEnqueued);

    m_uploadRequired = false;
    m_uploadEnqueued = true;

    if (m_geometryElementCount == 0)
    {
        m_sourceGeometryBuffer = {};
        m_destinationGeometryBuffer = {};
        return;
    }

    const auto geometryBufferSize = m_geometryBufferUpload.resource->GetDesc().Width;

    util::ReAllocateBuffer(
        &m_sourceGeometryBuffer,
        GetClient(), geometryBufferSize,
        D3D12_RESOURCE_FLAG_NONE,
        D3D12_RESOURCE_STATE_COPY_DEST,
        D3D12_HEAP_TYPE_DEFAULT);
    NAME_D3D12_OBJECT_WITH_ID(m_sourceGeometryBuffer);

    if (GetMaterial().IsAnimated())
    {
        util::ReAllocateBuffer(
            &m_destinationGeometryBuffer,
            GetClient(), geometryBufferSize,
            D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS,
            D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE,
            D3D12_HEAP_TYPE_DEFAULT);
        NAME_D3D12_OBJECT_WITH_ID(m_destinationGeometryBuffer);
    }
    else
    {
        m_destinationGeometryBuffer = {};
    }

    commandList->CopyBufferRegion(m_sourceGeometryBuffer.Get(), 0, m_geometryBufferUpload.Get(), 0, geometryBufferSize);

    const D3D12_RESOURCE_BARRIER transitionCopyDestToShaderResource = {
        CD3DX12_RESOURCE_BARRIER::Transition(m_sourceGeometryBuffer.Get(), D3D12_RESOURCE_STATE_COPY_DEST,
                                             D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE)
    };
    commandList->ResourceBarrier(1, &transitionCopyDestToShaderResource);

    if (GetMaterial().geometryType == D3D12_RAYTRACING_GEOMETRY_TYPE_TRIANGLES)
    {
        std::tie(m_usedIndexBuffer, m_usedIndexCount) = GetClient().GetSpace()->GetIndexBuffer(m_geometryElementCount);
    }
}

void MeshObject::CleanupMeshUpload()
{
    REQUIRE(!m_uploadRequired);

    m_geometryBufferUpload = {};
    
    m_uploadEnqueued = false;
}

ShaderResources::ConstantBufferViewDescriptor MeshObject::GetInstanceDataViewDescriptor() const
{
    return {
        .gpuAddress = m_instanceDataBuffer.GetGPUVirtualAddress(),
        .size = static_cast<UINT>(m_instanceDataBufferAlignedSize)
    };
}

ShaderResources::ShaderResourceViewDescriptor MeshObject::GetGeometryBufferViewDescriptor() const
{
    return {
        .resource = GetGeometryBuffer(),
        .description = &m_geometrySRV
    };
}

ShaderResources::ShaderResourceViewDescriptor MeshObject::GetAnimationSourceBufferViewDescriptor() const
{
    return {
        .resource = m_sourceGeometryBuffer,
        .description = &m_geometrySRV
    };
}

ShaderResources::UnorderedAccessViewDescriptor MeshObject::GetAnimationDestinationBufferViewDescriptor() const
{
    return {
        .resource = m_destinationGeometryBuffer,
        .description = &m_geometryUAV
    };
}

void MeshObject::CreateBLAS(ComPtr<ID3D12GraphicsCommandList4> commandList, std::vector<ID3D12Resource*>* uavs,
                            bool isForAnimation)
{
    REQUIRE(!m_uploadRequired);
    REQUIRE(uavs != nullptr);
    
    if (isForAnimation && m_requiresFreshBLAS) return;

    if (m_geometryElementCount == 0)
    {
        m_blas = {};
        return;
    }

    if (GetMaterial().geometryType == D3D12_RAYTRACING_GEOMETRY_TYPE_TRIANGLES)
    {
        CreateBottomLevelASFromVertices(commandList,
                                        {{GeometryBuffer(), m_geometryElementCount}},
                                        {{m_usedIndexBuffer, m_usedIndexCount}});
    }

    if (GetMaterial().geometryType == D3D12_RAYTRACING_GEOMETRY_TYPE_PROCEDURAL_PRIMITIVE_AABBS)
    {
        CreateBottomLevelASFromBounds(commandList,
                                      {{GeometryBuffer(), m_geometryElementCount}});
    }

    if (ID3D12Resource* resource = m_blas.result.GetResource(); resource != nullptr) uavs->push_back(resource);
}

const BLAS& MeshObject::GetBLAS()
{
    REQUIRE(!m_uploadRequired);
    
    return m_blas;
}

void MeshObject::SetAnimationHandle(const AnimationController::Handle handle)
{
    m_animationHandle = handle;
}

AnimationController::Handle MeshObject::GetAnimationHandle() const
{
    return m_animationHandle;
}

void MeshObject::AssociateWithHandle(Handle handle)
{
    REQUIRE(!m_handle.has_value());
    m_handle = handle;
}

void MeshObject::Return()
{
    REQUIRE(m_handle.has_value());
    REQUIRE(!m_uploadEnqueued);

    SetEnabledState(false);

    const auto handle = m_handle.value();

    {
        m_material = nullptr;

        // Instance buffer is intentionally not reset, because it is reused.

        m_sourceGeometryBuffer = {};
        m_destinationGeometryBuffer = {};
        m_geometryBufferUpload = {};
        m_geometryElementCount = 0;

        m_usedIndexBuffer = {};
        m_usedIndexCount = 0;

        m_blas = {};
        m_requiresFreshBLAS = false;

        m_handle = std::nullopt;
        m_active = std::nullopt;
        m_enabled = true;

        m_animationHandle = AnimationController::Handle::INVALID;

        m_uploadRequired = false;
        m_uploadEnqueued = false;
    }

    GetClient().GetSpace()->ReturnMeshObject(handle);

    // No code here, because space is allowed to delete this object.
}

void MeshObject::CreateBottomLevelASFromVertices(
    ComPtr<ID3D12GraphicsCommandList4> commandList,
    std::vector<std::pair<Allocation<ID3D12Resource>, uint32_t>> vertexBuffers,
    std::vector<std::pair<Allocation<ID3D12Resource>, uint32_t>> indexBuffers)
{
    if (m_requiresFreshBLAS)
    {
        m_bottomLevelASGenerator = {};

        REQUIRE(vertexBuffers.size() == indexBuffers.size());
        for (size_t index = 0; index < vertexBuffers.size(); index++)
        {
            auto& [vertexBuffer, vertexCount] = vertexBuffers[index];
            auto& [indexBuffer, indexCount] = indexBuffers[index];

            const bool isOpaque = GetMaterial().isOpaque;

            m_bottomLevelASGenerator.AddVertexBuffer(
                vertexBuffer, 0, vertexCount,
                sizeof(SpatialVertex),
                indexBuffer, 0, indexCount,
                {}, 0,
                isOpaque);
        }
    }

    CreateBottomLevelAS(commandList);
}

void MeshObject::CreateBottomLevelASFromBounds(
    ComPtr<ID3D12GraphicsCommandList4> commandList,
    std::vector<std::pair<Allocation<ID3D12Resource>, uint32_t>> boundsBuffers)
{
    if (m_requiresFreshBLAS)
    {
        m_bottomLevelASGenerator = {};

        for (size_t index = 0; index < boundsBuffers.size(); index++)
        {
            auto& [boundsBuffer, boundsCount] = boundsBuffers[index];

            m_bottomLevelASGenerator.AddBoundsBuffer(
                boundsBuffer, 0, boundsCount,
                sizeof(SpatialBounds));
        }
    }

    return CreateBottomLevelAS(commandList);
}

void MeshObject::CreateBottomLevelAS(
    ComPtr<ID3D12GraphicsCommandList4> commandList)
{
    bool updateOnly;
    D3D12_GPU_VIRTUAL_ADDRESS previousResult;

    if (m_requiresFreshBLAS)
    {
        m_requiresFreshBLAS = false;

        UINT64 scratchSizeInBytes = 0;
        UINT64 resultSizeInBytes = 0;
        const bool allowUpdate = GetMaterial().IsAnimated();

        m_bottomLevelASGenerator.ComputeASBufferSizes(GetClient().GetDevice().Get(), allowUpdate, &scratchSizeInBytes,
                                                      &resultSizeInBytes);

        m_blas = GetClient().GetSpace()->AllocateBLAS(resultSizeInBytes, scratchSizeInBytes);

        NAME_D3D12_OBJECT_WITH_ID(m_blas.scratch);
        NAME_D3D12_OBJECT_WITH_ID(m_blas.result);

        updateOnly = false;
        previousResult = 0;
    }
    else
    {
        REQUIRE(GetMaterial().IsAnimated());

        updateOnly = true;
        previousResult = m_blas.result.GetAddress();
    }

    m_bottomLevelASGenerator.Generate(commandList.Get(),
                                      m_blas.scratch.GetAddress(), m_blas.result.GetAddress(),
                                      updateOnly, previousResult);
}

Allocation<ID3D12Resource>& MeshObject::GeometryBuffer()
{
    return GetMaterial().IsAnimated() ? m_destinationGeometryBuffer : m_sourceGeometryBuffer;
}

void MeshObject::UpdateActiveState()
{
    const bool shouldBeActive = m_enabled && m_geometryElementCount > 0;
    if (m_active.has_value() == shouldBeActive) return;

    if (shouldBeActive)
    {
        REQUIRE(!m_active.has_value());
        
        m_active = GetClient().GetSpace()->ActivateMeshObject(m_handle.value());
    }
    else
    {
        REQUIRE(m_active.has_value());
        
        GetClient().GetSpace()->DeactivateMeshObject(m_active.value());
        m_active = std::nullopt;
    }
}

void MeshObject::UpdateGeometryViews(const UINT stride)
{
    m_geometrySRV.Buffer.NumElements = m_geometryElementCount;
    m_geometrySRV.Buffer.StructureByteStride = stride;

    m_geometryUAV.Buffer.NumElements = m_geometryElementCount;
    m_geometryUAV.Buffer.StructureByteStride = stride;
}
