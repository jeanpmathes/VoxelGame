#include "stdafx.h"
#include "Mesh.hpp"

Mesh::Mesh(NativeClient& client) : Drawable(client)
{
    REQUIRE(GetClient().GetDevice() != nullptr);

    m_instanceDataBufferAlignedSize = sizeof MeshDataBuffer;
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

void Mesh::Initialize(UINT materialIndex)
{
    m_material = &GetClient().GetSpace()->GetMaterial(materialIndex);
    
    Update();
}

void Mesh::Update()
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

void Mesh::SetNewVertices(const SpatialVertex* vertices, const UINT vertexCount)
{
    REQUIRE(GetMaterial().geometryType == D3D12_RAYTRACING_GEOMETRY_TYPE_TRIANGLES);

    UpdateGeometryViews(vertexCount, sizeof(SpatialVertex));

    if (const bool uploadRequired = HandleModification(vertexCount); !uploadRequired) return;
    m_requiresFreshBLAS = true;

    const auto vertexBufferSize = sizeof(SpatialVertex) * vertexCount;
    util::ReAllocateBuffer(&GetUploadDataBuffer(),
                           GetClient(), vertexBufferSize,
                           D3D12_RESOURCE_FLAG_NONE,
                           D3D12_RESOURCE_STATE_GENERIC_READ,
                           D3D12_HEAP_TYPE_UPLOAD);
    NAME_D3D12_OBJECT_WITH_ID(GetUploadDataBuffer());

    TRY_DO(util::MapAndWrite(GetUploadDataBuffer(), vertices, vertexCount));
}

void Mesh::SetNewBounds(const SpatialBounds* bounds, const UINT boundsCount)
{
    REQUIRE(GetMaterial().geometryType == D3D12_RAYTRACING_GEOMETRY_TYPE_PROCEDURAL_PRIMITIVE_AABBS);


    UpdateGeometryViews(boundsCount, sizeof(SpatialBounds));

    if (const bool uploadRequired = HandleModification(boundsCount); !uploadRequired) return;
    m_requiresFreshBLAS = true;

    const auto vertexBufferSize = sizeof(SpatialBounds) * boundsCount;
    util::ReAllocateBuffer(&GetUploadDataBuffer(),
                           GetClient(), vertexBufferSize,
                           D3D12_RESOURCE_FLAG_NONE,
                           D3D12_RESOURCE_STATE_GENERIC_READ,
                           D3D12_HEAP_TYPE_UPLOAD);
    NAME_D3D12_OBJECT_WITH_ID(GetUploadDataBuffer());

    TRY_DO(util::MapAndWrite(GetUploadDataBuffer(), bounds, boundsCount));
}

const Material& Mesh::GetMaterial() const
{
    REQUIRE(m_material != nullptr);
    return *m_material;
}

UINT Mesh::GetGeometryUnitCount() const
{
    switch (GetMaterial().geometryType)
    {
    case D3D12_RAYTRACING_GEOMETRY_TYPE_TRIANGLES: return GetDataElementCount() / 4;
    case D3D12_RAYTRACING_GEOMETRY_TYPE_PROCEDURAL_PRIMITIVE_AABBS: return GetDataElementCount();
    default: throw NativeException("Unknown geometry type.");
    }
}

Allocation<ID3D12Resource> Mesh::GetGeometryBuffer() const
{
    return const_cast<Mesh*>(this)->GeometryBuffer();
}

ShaderResources::ConstantBufferViewDescriptor Mesh::GetInstanceDataViewDescriptor() const
{
    return ShaderResources::ConstantBufferViewDescriptor(
        m_instanceDataBuffer.GetGPUVirtualAddress(),
        static_cast<UINT>(m_instanceDataBufferAlignedSize)
    );
}

ShaderResources::ShaderResourceViewDescriptor Mesh::GetGeometryBufferViewDescriptor() const
{
    return {
        .resource = GetGeometryBuffer(),
        .description = &m_geometrySRV
    };
}

ShaderResources::ShaderResourceViewDescriptor Mesh::GetAnimationSourceBufferViewDescriptor() const
{
    return {
        .resource = m_sourceGeometryBuffer,
        .description = &m_geometrySRV
    };
}

ShaderResources::UnorderedAccessViewDescriptor Mesh::GetAnimationDestinationBufferViewDescriptor() const
{
    return {
        .resource = m_destinationGeometryBuffer,
        .description = &m_geometryUAV
    };
}

void Mesh::CreateBLAS(
    ComPtr<ID3D12GraphicsCommandList4> commandList,
    std::vector<ID3D12Resource*>* uavs,
    bool isForAnimation)
{
    REQUIRE(uavs != nullptr);
    
    if (isForAnimation && m_requiresFreshBLAS) return;

    if (GetDataElementCount() == 0)
    {
        m_blas = {};
        return;
    }

    if (GetMaterial().geometryType == D3D12_RAYTRACING_GEOMETRY_TYPE_TRIANGLES)
    {
        CreateBottomLevelASFromVertices(commandList,
                                        {{GeometryBuffer(), GetDataElementCount()}},
                                        {{m_usedIndexBuffer, m_usedIndexCount}});
    }

    if (GetMaterial().geometryType == D3D12_RAYTRACING_GEOMETRY_TYPE_PROCEDURAL_PRIMITIVE_AABBS)
    {
        CreateBottomLevelASFromBounds(commandList,
                                      {{GeometryBuffer(), GetDataElementCount()}});
    }

    if (ID3D12Resource* resource = m_blas.result.GetResource(); resource != nullptr) uavs->push_back(resource);
}

const BLAS& Mesh::GetBLAS()
{
    return m_blas;
}

void Mesh::SetAnimationHandle(const AnimationController::Handle handle)
{
    m_animationHandle = handle;
}

AnimationController::Handle Mesh::GetAnimationHandle() const
{
    return m_animationHandle;
}

void Mesh::Accept(Visitor& visitor)
{
    visitor.Visit(*this);
}

void Mesh::DoDataUpload(ComPtr<ID3D12GraphicsCommandList> commandList)
{
    if (GetDataElementCount() == 0)
    {
        m_sourceGeometryBuffer = {};
        m_destinationGeometryBuffer = {};
        return;
    }

    const auto geometryBufferSize = GetUploadDataBuffer().resource->GetDesc().Width;

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

    commandList->CopyBufferRegion(m_sourceGeometryBuffer.Get(), 0, GetUploadDataBuffer().Get(), 0, geometryBufferSize);

    const D3D12_RESOURCE_BARRIER transitionCopyDestToShaderResource = {
        CD3DX12_RESOURCE_BARRIER::Transition(m_sourceGeometryBuffer.Get(), D3D12_RESOURCE_STATE_COPY_DEST,
                                             D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE)
    };
    commandList->ResourceBarrier(1, &transitionCopyDestToShaderResource);

    if (GetMaterial().geometryType == D3D12_RAYTRACING_GEOMETRY_TYPE_TRIANGLES)
    {
        std::tie(m_usedIndexBuffer, m_usedIndexCount) = GetClient().GetSpace()->GetIndexBuffer(GetDataElementCount());
    }
}

void Mesh::DoReset()
{
    m_material = nullptr;

    // Instance buffer is intentionally not reset, because it is reused.

    m_sourceGeometryBuffer = {};
    m_destinationGeometryBuffer = {};

    m_usedIndexBuffer = {};
    m_usedIndexCount = 0;

    m_blas = {};
    m_requiresFreshBLAS = false;

    m_animationHandle = AnimationController::Handle::INVALID;
}

void Mesh::CreateBottomLevelASFromVertices(
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

void Mesh::CreateBottomLevelASFromBounds(
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

void Mesh::CreateBottomLevelAS(
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

Allocation<ID3D12Resource>& Mesh::GeometryBuffer()
{
    return GetMaterial().IsAnimated() ? m_destinationGeometryBuffer : m_sourceGeometryBuffer;
}

void Mesh::UpdateGeometryViews(const UINT count, const UINT stride)
{
    m_geometrySRV.Buffer.NumElements = count;
    m_geometrySRV.Buffer.StructureByteStride = stride;

    m_geometryUAV.Buffer.NumElements = count;
    m_geometryUAV.Buffer.StructureByteStride = stride;
}
