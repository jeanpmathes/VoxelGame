#include "stdafx.h"
#include "MeshObject.h"

MeshObject::MeshObject(NativeClient& client, const UINT materialIndex)
    : SpatialObject(client), m_materialIndex(materialIndex)
{
    REQUIRE(GetClient().GetDevice() != nullptr);

    m_instanceConstantBufferAlignedSize = sizeof m_instanceConstantBufferData;
    m_instanceConstantBuffer = util::AllocateConstantBuffer(GetClient(), &m_instanceConstantBufferAlignedSize);
    NAME_D3D12_OBJECT_WITH_ID(m_instanceConstantBuffer);

    Update();
}

void MeshObject::Update()
{
    if (!ClearTransformDirty()) return;

    {
        const DirectX::XMFLOAT4X4 objectToWorld = GetTransform();

        const DirectX::XMMATRIX transform = XMLoadFloat4x4(&objectToWorld);
        const DirectX::XMMATRIX transformNormal = XMMatrixToNormal(transform);

        DirectX::XMFLOAT4X4 objectToWorldNormal = {};
        XMStoreFloat4x4(&objectToWorldNormal, transformNormal);

        m_instanceConstantBufferData = {
            .objectToWorld = objectToWorld,
            .objectToWorldNormal = objectToWorldNormal
        };
    }

    TRY_DO(util::MapAndWrite(m_instanceConstantBuffer, m_instanceConstantBufferData));
}

void MeshObject::SetEnabledState(const bool enabled)
{
    m_enabled = enabled;
}

void MeshObject::SetNewMesh(const SpatialVertex* vertices, const UINT vertexCount)
{
    REQUIRE(!IsMeshModified());
    REQUIRE(!m_uploadRequired);
    REQUIRE(!m_uploadEnqueued);
    
    const auto vertexBufferSize = sizeof(SpatialVertex) * vertexCount;

    m_vertexCount = vertexCount;
    m_modified = true;
    m_uploadRequired = true;

    if (m_vertexCount == 0)
    {
        m_vertexBufferUpload = {};
        return;
    }

    m_vertexBufferUpload = util::AllocateBuffer(GetClient(), vertexBufferSize,
                                                D3D12_RESOURCE_FLAG_NONE,
                                                D3D12_RESOURCE_STATE_GENERIC_READ,
                                                D3D12_HEAP_TYPE_UPLOAD);
    NAME_D3D12_OBJECT_WITH_ID(m_vertexBufferUpload);

    TRY_DO(util::MapAndWrite(m_vertexBufferUpload, vertices, vertexCount));
}

bool MeshObject::IsMeshModified() const
{
    return m_modified;
}

bool MeshObject::IsEnabled() const
{
    return m_enabled && m_vertexCount > 0;
}

void MeshObject::EnqueueMeshUpload(const ComPtr<ID3D12GraphicsCommandList> commandList)
{
    REQUIRE(IsMeshModified());
    REQUIRE(m_uploadRequired);
    REQUIRE(!m_uploadEnqueued);

    m_uploadRequired = false;
    m_uploadEnqueued = true;

    if (m_vertexCount == 0)
    {
        m_vertexBuffer = {};
        return;
    }

    const auto vertexBufferSize = m_vertexBufferUpload.resource->GetDesc().Width;
    
    m_vertexBuffer = util::AllocateBuffer(GetClient(), vertexBufferSize,
                                          D3D12_RESOURCE_FLAG_NONE,
                                          D3D12_RESOURCE_STATE_COPY_DEST,
                                          D3D12_HEAP_TYPE_DEFAULT);
    NAME_D3D12_OBJECT_WITH_ID(m_vertexBuffer);

    commandList->CopyBufferRegion(m_vertexBuffer.Get(), 0, m_vertexBufferUpload.Get(), 0, vertexBufferSize);

    const D3D12_RESOURCE_BARRIER transitionCopyDestToShaderResource = {
        CD3DX12_RESOURCE_BARRIER::Transition(m_vertexBuffer.Get(), D3D12_RESOURCE_STATE_COPY_DEST,
                                             D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE)
    };
    commandList->ResourceBarrier(1, &transitionCopyDestToShaderResource);

    std::tie(m_usedIndexBuffer, m_usedIndexCount) = GetClient().GetSpace()->GetIndexBuffer(m_vertexCount);
}

void MeshObject::CleanupMeshUpload()
{
    REQUIRE(!m_uploadRequired);
    
    m_vertexBufferUpload = {};

    m_modified = false;
    m_uploadEnqueued = false;
}

void MeshObject::FillArguments(StandardShaderArguments& shaderArguments) const
{
    shaderArguments.instanceBuffer = reinterpret_cast<void*>(m_instanceConstantBuffer.resource->GetGPUVirtualAddress());
}

void MeshObject::SetupHitGroup(nv_helpers_dx12::ShaderBindingTableGenerator& sbt,
                               StandardShaderArguments& shaderArguments) const
{
    REQUIRE(!m_uploadRequired);
    
    const Material& material = GetClient().GetSpace()->GetMaterial(m_materialIndex);

    sbt.AddHitGroup(material.normalHitGroup,
                    {
                        reinterpret_cast<void*>(m_vertexBuffer.resource->GetGPUVirtualAddress()),
                        shaderArguments.heap,
                        shaderArguments.globalBuffer,
                        shaderArguments.instanceBuffer
                    });
    sbt.AddHitGroup(material.shadowHitGroup,
                    {
                        reinterpret_cast<void*>(m_vertexBuffer.resource->GetGPUVirtualAddress()),
                        shaderArguments.heap,
                        shaderArguments.globalBuffer,
                        shaderArguments.instanceBuffer
                    });
}

void MeshObject::CreateBLAS(ComPtr<ID3D12GraphicsCommandList4> commandList)
{
    REQUIRE(IsMeshModified());
    REQUIRE(!m_uploadRequired);

    if (m_vertexCount == 0)
    {
        m_blas = {};
        return;
    }
    
    m_blas = CreateBottomLevelAS(commandList,
                                 {{m_vertexBuffer, m_vertexCount}},
                                 {{m_usedIndexBuffer, m_usedIndexCount}});
}

Allocation<ID3D12Resource> MeshObject::GetBLAS()
{
    return m_blas.result;
}

UINT MeshObject::GetMaterialIndex() const
{
    return m_materialIndex;
}

void MeshObject::AssociateWithHandle(Handle handle)
{
    REQUIRE(!m_handle.has_value());
    m_handle = handle;
}

void MeshObject::Free() const
{
    REQUIRE(!m_uploadEnqueued);
    REQUIRE(!m_uploadRequired);
    
    REQUIRE(m_handle.has_value());
    GetClient().GetSpace()->FreeMeshObject(m_handle.value());
}

AccelerationStructureBuffers MeshObject::CreateBottomLevelAS(ComPtr<ID3D12GraphicsCommandList4> commandList,
                                                             std::vector<std::pair<
                                                                 Allocation<ID3D12Resource>, uint32_t>>
                                                             vertexBuffers,
                                                             std::vector<std::pair<
                                                                 Allocation<ID3D12Resource>, uint32_t>>
                                                             indexBuffers) const
{
    nv_helpers_dx12::BottomLevelASGenerator bottomLevelAS;

    REQUIRE(vertexBuffers.size() == indexBuffers.size());
    for (size_t i = 0; i < vertexBuffers.size(); i++)
    {
        auto& [vertexBuffer, vertexCount] = vertexBuffers[i];
        auto& [indexBuffer, indexCount] = indexBuffers[i];

        constexpr bool isOpaque = false;
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
    
    AccelerationStructureBuffers buffers;
    buffers.scratch = util::AllocateBuffer(GetClient(), scratchSizeInBytes,
                                           D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS,
                                           D3D12_RESOURCE_STATE_COMMON,
                                           D3D12_HEAP_TYPE_DEFAULT);
    buffers.result = util::AllocateBuffer(GetClient(), resultSizeInBytes,
                                          D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS,
                                          D3D12_RESOURCE_STATE_RAYTRACING_ACCELERATION_STRUCTURE,
                                          D3D12_HEAP_TYPE_DEFAULT);

    NAME_D3D12_OBJECT_WITH_ID(buffers.scratch);
    NAME_D3D12_OBJECT_WITH_ID(buffers.result);

    bottomLevelAS.Generate(commandList.Get(), buffers.scratch.Get(), buffers.result.Get(),
                           false, nullptr);
    return buffers;
}
