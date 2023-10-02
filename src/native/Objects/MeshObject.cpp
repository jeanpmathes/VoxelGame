#include "stdafx.h"
#include "MeshObject.hpp"

MeshObject::MeshObject(NativeClient& client, const UINT materialIndex)
    : SpatialObject(client), m_material(client.GetSpace()->GetMaterial(materialIndex))
{
    REQUIRE(GetClient().GetDevice() != nullptr);

    m_instanceDataBufferAlignedSize = sizeof InstanceConstantBuffer;
    m_instanceDataBuffer = util::AllocateConstantBuffer(GetClient(), &m_instanceDataBufferAlignedSize);
    NAME_D3D12_OBJECT_WITH_ID(m_instanceDataBuffer);

    m_instanceDataBufferView.BufferLocation = m_instanceDataBuffer.GetGPUVirtualAddress();
    m_instanceDataBufferView.SizeInBytes = static_cast<UINT>(m_instanceDataBufferAlignedSize);

    TRY_DO(m_instanceDataBuffer.Map(&m_instanceConstantBufferMapping));

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
    REQUIRE(m_material.geometryType == D3D12_RAYTRACING_GEOMETRY_TYPE_TRIANGLES);

    const auto vertexBufferSize = sizeof(SpatialVertex) * vertexCount;

    m_geometryElementCount = vertexCount;
    
    UpdateActiveState();
    UpdateGeometryBufferView(sizeof(SpatialVertex));

    if (m_geometryElementCount == 0)
    {
        m_geometryBufferUpload = {};
        return;
    }

    GetClient().GetSpace()->MarkMeshObjectModified(m_handle.value());
    m_uploadRequired = true;
    
    m_geometryBufferUpload = util::AllocateBuffer(GetClient(), vertexBufferSize,
                                                  D3D12_RESOURCE_FLAG_NONE,
                                                  D3D12_RESOURCE_STATE_GENERIC_READ,
                                                  D3D12_HEAP_TYPE_UPLOAD);
    NAME_D3D12_OBJECT_WITH_ID(m_geometryBufferUpload);

    TRY_DO(util::MapAndWrite(m_geometryBufferUpload, vertices, vertexCount));
}

void MeshObject::SetNewBounds(const SpatialBounds* bounds, const UINT boundsCount)
{
    REQUIRE(!m_uploadEnqueued);
    REQUIRE(m_material.geometryType == D3D12_RAYTRACING_GEOMETRY_TYPE_PROCEDURAL_PRIMITIVE_AABBS);

    const auto vertexBufferSize = sizeof(SpatialBounds) * boundsCount;

    m_geometryElementCount = boundsCount;

    UpdateActiveState();
    UpdateGeometryBufferView(sizeof(SpatialBounds));

    if (m_geometryElementCount == 0)
    {
        m_geometryBufferUpload = {};
        return;
    }

    GetClient().GetSpace()->MarkMeshObjectModified(m_handle.value());
    m_uploadRequired = true;

    m_geometryBufferUpload = util::AllocateBuffer(GetClient(), vertexBufferSize,
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
    return m_material;
}

void MeshObject::EnqueueMeshUpload(const ComPtr<ID3D12GraphicsCommandList> commandList)
{
    REQUIRE(m_uploadRequired);
    REQUIRE(!m_uploadEnqueued);

    m_uploadRequired = false;
    m_uploadEnqueued = true;

    if (m_geometryElementCount == 0)
    {
        m_geometryBuffer = {};
        return;
    }

    const auto geometryBufferSize = m_geometryBufferUpload.resource->GetDesc().Width;

    m_geometryBuffer = util::AllocateBuffer(GetClient(), geometryBufferSize,
                                            D3D12_RESOURCE_FLAG_NONE,
                                            D3D12_RESOURCE_STATE_COPY_DEST,
                                            D3D12_HEAP_TYPE_DEFAULT);
    NAME_D3D12_OBJECT_WITH_ID(m_geometryBuffer);

    commandList->CopyBufferRegion(m_geometryBuffer.Get(), 0, m_geometryBufferUpload.Get(), 0, geometryBufferSize);

    const D3D12_RESOURCE_BARRIER transitionCopyDestToShaderResource = {
        CD3DX12_RESOURCE_BARRIER::Transition(m_geometryBuffer.Get(), D3D12_RESOURCE_STATE_COPY_DEST,
                                             D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE)
    };
    commandList->ResourceBarrier(1, &transitionCopyDestToShaderResource);

    if (m_material.geometryType == D3D12_RAYTRACING_GEOMETRY_TYPE_TRIANGLES)
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

void MeshObject::CreateInstanceResourceViews(const DescriptorHeap& heap, const UINT data, const UINT geometry) const
{
    GetClient().GetDevice()->CreateConstantBufferView(
        &m_instanceDataBufferView,
        heap.GetDescriptorHandleCPU(data));

    GetClient().GetDevice()->CreateShaderResourceView(
        m_geometryBuffer.Get(),
        &m_geometryBufferView,
        heap.GetDescriptorHandleCPU(geometry));
}

void MeshObject::CreateBLAS(ComPtr<ID3D12GraphicsCommandList4> commandList, std::vector<ID3D12Resource*>* uavs)
{
    REQUIRE(!m_uploadRequired);
    REQUIRE(uavs != nullptr);

    if (m_geometryElementCount == 0)
    {
        m_blas = {};
        return;
    }

    if (m_material.geometryType == D3D12_RAYTRACING_GEOMETRY_TYPE_TRIANGLES)
    {
        m_blas = CreateBottomLevelASFromVertices(commandList,
                                                 {{m_geometryBuffer, m_geometryElementCount}},
                                                 {{m_usedIndexBuffer, m_usedIndexCount}});
    }

    if (m_material.geometryType == D3D12_RAYTRACING_GEOMETRY_TYPE_PROCEDURAL_PRIMITIVE_AABBS)
    {
        m_blas = CreateBottomLevelASFromBounds(commandList,
                                               {{m_geometryBuffer, m_geometryElementCount}});
    }

    if (ID3D12Resource* resource = m_blas.result.GetResource(); resource != nullptr) uavs->push_back(resource);
}

const BLAS& MeshObject::GetBLAS()
{
    REQUIRE(!m_uploadRequired);
    return m_blas;
}

void MeshObject::AssociateWithHandle(Handle handle)
{
    REQUIRE(!m_handle.has_value());
    m_handle = handle;
}

void MeshObject::Free()
{
    REQUIRE(!m_uploadEnqueued);
    REQUIRE(m_handle.has_value());

    SetEnabledState(false);
    GetClient().GetSpace()->FreeMeshObject(m_handle.value());
}

BLAS MeshObject::CreateBottomLevelASFromVertices(
    ComPtr<ID3D12GraphicsCommandList4> commandList,
    std::vector<std::pair<Allocation<ID3D12Resource>, uint32_t>> vertexBuffers,
    std::vector<std::pair<Allocation<ID3D12Resource>, uint32_t>> indexBuffers) const
{
    nv_helpers_dx12::BottomLevelASGenerator bottomLevelAS;

    REQUIRE(vertexBuffers.size() == indexBuffers.size());
    for (size_t index = 0; index < vertexBuffers.size(); index++)
    {
        auto& [vertexBuffer, vertexCount] = vertexBuffers[index];
        auto& [indexBuffer, indexCount] = indexBuffers[index];

        const bool isOpaque = m_material.isOpaque;
        
        bottomLevelAS.AddVertexBuffer(
            vertexBuffer.Get(), 0, vertexCount,
            sizeof(SpatialVertex),
            indexBuffer.Get(), 0, indexCount,
            nullptr, 0,
            isOpaque);
    }

    UINT64 scratchSizeInBytes = 0;
    UINT64 resultSizeInBytes = 0;
    bottomLevelAS.ComputeASBufferSizes(GetClient().GetDevice().Get(), false, &scratchSizeInBytes, &resultSizeInBytes);

    BLAS blas = GetClient().GetSpace()->AllocateBLAS(resultSizeInBytes, scratchSizeInBytes);

    NAME_D3D12_OBJECT_WITH_ID(blas.scratch);
    NAME_D3D12_OBJECT_WITH_ID(blas.result);

    bottomLevelAS.Generate(commandList.Get(),
                           blas.scratch.GetAddress(), blas.result.GetAddress(), false);

    return blas;
}

BLAS MeshObject::CreateBottomLevelASFromBounds(
    ComPtr<ID3D12GraphicsCommandList4> commandList,
    std::vector<std::pair<Allocation<ID3D12Resource>, uint32_t>> boundsBuffers) const
{
    nv_helpers_dx12::BottomLevelASGenerator bottomLevelAS;

    for (size_t index = 0; index < boundsBuffers.size(); index++)
    {
        auto& [boundsBuffer, boundsCount] = boundsBuffers[index];

        bottomLevelAS.AddBoundsBuffer(
            boundsBuffer.Get(), 0, boundsCount,
            sizeof(SpatialBounds));
    }

    UINT64 scratchSizeInBytes = 0;
    UINT64 resultSizeInBytes = 0;
    bottomLevelAS.ComputeASBufferSizes(GetClient().GetDevice().Get(), false, &scratchSizeInBytes, &resultSizeInBytes);

    BLAS blas = GetClient().GetSpace()->AllocateBLAS(resultSizeInBytes, scratchSizeInBytes);

    NAME_D3D12_OBJECT_WITH_ID(blas.scratch);
    NAME_D3D12_OBJECT_WITH_ID(blas.result);

    bottomLevelAS.Generate(commandList.Get(),
                           blas.scratch.GetAddress(), blas.result.GetAddress(), false);

    return blas;
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

void MeshObject::UpdateGeometryBufferView(const UINT stride)
{
    m_geometryBufferView.Format = DXGI_FORMAT_UNKNOWN;
    m_geometryBufferView.ViewDimension = D3D12_SRV_DIMENSION_BUFFER;
    m_geometryBufferView.Shader4ComponentMapping = D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING;
    m_geometryBufferView.Buffer.FirstElement = 0;
    m_geometryBufferView.Buffer.NumElements = m_geometryElementCount;
    m_geometryBufferView.Buffer.StructureByteStride = stride;
    m_geometryBufferView.Buffer.Flags = D3D12_BUFFER_SRV_FLAG_NONE;
}
