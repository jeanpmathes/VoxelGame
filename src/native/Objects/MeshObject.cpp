#include "stdafx.h"
#include "MeshObject.h"

MeshObject::MeshObject(NativeClient& client, const UINT materialIndex)
    : SpatialObject(client), m_materialIndex(materialIndex)
{
    REQUIRE(GetClient().GetDevice() != nullptr);

    m_instanceConstantBufferAlignedSize = sizeof m_instanceConstantBufferData;

    m_instanceConstantBuffer = util::AllocateConstantBuffer(GetClient(), &m_instanceConstantBufferAlignedSize);

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

    {
        uint8_t* pData;
        TRY_DO(m_instanceConstantBuffer.resource->Map(0, nullptr, reinterpret_cast<void**>(&pData)));

        memcpy(pData, &m_instanceConstantBufferData, sizeof m_instanceConstantBufferData);

        m_instanceConstantBuffer.resource->Unmap(0, nullptr);
    }
}

void MeshObject::SetEnabledState(const bool enabled)
{
    m_enabled = enabled;
}

void MeshObject::SetNewMesh(const SpatialVertex* vertices, UINT vertexCount, const UINT* indices, UINT indexCount)
{
    const auto vertexBufferSize = sizeof(SpatialVertex) * vertexCount;
    const auto indexBufferSize = sizeof(UINT) * indexCount;

    m_vertexCount = vertexCount;
    m_indexCount = indexCount;
    m_modified = true;

    if (m_vertexCount == 0 || m_indexCount == 0)
    {
        m_vertexBufferUpload = {};
        m_indexBufferUpload = {};

        return;
    }
    
    const auto vertexBufferUploadDesc = CD3DX12_RESOURCE_DESC::Buffer(vertexBufferSize);
    m_vertexBufferUpload = util::AllocateResource<ID3D12Resource>(GetClient(),
                                                                  vertexBufferUploadDesc, D3D12_HEAP_TYPE_UPLOAD,
                                                                  D3D12_RESOURCE_STATE_GENERIC_READ);
    NAME_D3D12_OBJECT_WITH_ID(m_vertexBufferUpload);

    const auto indexBufferUploadDesc = CD3DX12_RESOURCE_DESC::Buffer(indexBufferSize);
    m_indexBufferUpload = util::AllocateResource<ID3D12Resource>(GetClient(),
                                                                 indexBufferUploadDesc, D3D12_HEAP_TYPE_UPLOAD,
                                                                 D3D12_RESOURCE_STATE_GENERIC_READ);
    NAME_D3D12_OBJECT_WITH_ID(m_indexBufferUpload);

    {
        UINT8* pVertexDataBegin;
        const CD3DX12_RANGE readRange(0, 0);
        TRY_DO(m_vertexBufferUpload.resource->Map(0, &readRange, reinterpret_cast<void**>(&pVertexDataBegin)));
        memcpy(pVertexDataBegin, vertices, vertexBufferSize);
        m_vertexBufferUpload.resource->Unmap(0, nullptr);
    }

    {
        UINT8* pIndexDataBegin;
        const CD3DX12_RANGE readRange(0, 0);
        TRY_DO(m_indexBufferUpload.resource->Map(0, &readRange, reinterpret_cast<void**>(&pIndexDataBegin)));
        memcpy(pIndexDataBegin, indices, indexBufferSize);
        m_indexBufferUpload.resource->Unmap(0, nullptr);
    }
}

bool MeshObject::IsMeshModified() const
{
    return m_modified;
}

bool MeshObject::IsEnabled() const
{
    return m_enabled && m_vertexCount > 0 && m_indexCount > 0;
}

void MeshObject::EnqueueMeshUpload(ComPtr<ID3D12GraphicsCommandList> commandList)
{
    REQUIRE(IsMeshModified());

    if (m_vertexCount == 0 || m_indexCount == 0)
    {
        m_vertexBuffer = {};
        m_indexBuffer = {};

        return;
    }

    const auto vertexBufferSize = m_vertexBufferUpload.resource->GetDesc().Width;
    const auto indexBufferSize = m_indexBufferUpload.resource->GetDesc().Width;
    
    const auto vertexBufferDesc = CD3DX12_RESOURCE_DESC::Buffer(vertexBufferSize);
    m_vertexBuffer = util::AllocateResource<ID3D12Resource>(GetClient(),
                                                            vertexBufferDesc, D3D12_HEAP_TYPE_DEFAULT,
                                                            D3D12_RESOURCE_STATE_COMMON);
    NAME_D3D12_OBJECT_WITH_ID(m_vertexBuffer);
    
    const auto indexBufferDesc = CD3DX12_RESOURCE_DESC::Buffer(indexBufferSize);
    m_indexBuffer = util::AllocateResource<ID3D12Resource>(GetClient(),
                                                           indexBufferDesc, D3D12_HEAP_TYPE_DEFAULT,
                                                           D3D12_RESOURCE_STATE_COMMON);
    NAME_D3D12_OBJECT_WITH_ID(m_indexBuffer);

    D3D12_RESOURCE_BARRIER transitionCommonToCopyDest[] = {
        CD3DX12_RESOURCE_BARRIER::Transition(m_vertexBuffer.Get(), D3D12_RESOURCE_STATE_COMMON,
                                             D3D12_RESOURCE_STATE_COPY_DEST),
        CD3DX12_RESOURCE_BARRIER::Transition(m_indexBuffer.Get(), D3D12_RESOURCE_STATE_COMMON,
                                             D3D12_RESOURCE_STATE_COPY_DEST)
    };
    commandList->ResourceBarrier(_countof(transitionCommonToCopyDest), transitionCommonToCopyDest);

    commandList->CopyBufferRegion(m_vertexBuffer.Get(), 0, m_vertexBufferUpload.Get(), 0, vertexBufferSize);
    commandList->CopyBufferRegion(m_indexBuffer.Get(), 0, m_indexBufferUpload.Get(), 0, indexBufferSize);

    D3D12_RESOURCE_BARRIER transitionCopyDestToBuffer[] = {
        CD3DX12_RESOURCE_BARRIER::Transition(m_vertexBuffer.Get(), D3D12_RESOURCE_STATE_COPY_DEST,
                                             D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE),
        CD3DX12_RESOURCE_BARRIER::Transition(m_indexBuffer.Get(), D3D12_RESOURCE_STATE_COPY_DEST,
                                             D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE)
    };
    commandList->ResourceBarrier(_countof(transitionCopyDestToBuffer), transitionCopyDestToBuffer);
}

void MeshObject::CleanupMeshUpload()
{
    m_vertexBufferUpload = {};
    m_indexBufferUpload = {};

    m_modified = false;
}

void MeshObject::FillArguments(StandardShaderArguments& shaderArguments) const
{
    shaderArguments.instanceBuffer = reinterpret_cast<void*>(m_instanceConstantBuffer.resource->GetGPUVirtualAddress());
}

void MeshObject::SetupHitGroup(nv_helpers_dx12::ShaderBindingTableGenerator& sbt,
                               StandardShaderArguments& shaderArguments) const
{
    const Material& material = GetClient().GetSpace()->GetMaterial(m_materialIndex);

    sbt.AddHitGroup(material.normalHitGroup,
                    {
                        reinterpret_cast<void*>(m_vertexBuffer.resource->GetGPUVirtualAddress()),
                        reinterpret_cast<void*>(m_indexBuffer.resource->GetGPUVirtualAddress()),
                        shaderArguments.heap,
                        shaderArguments.globalBuffer,
                        shaderArguments.instanceBuffer
                    });
    sbt.AddHitGroup(material.shadowHitGroup,
                    {
                        reinterpret_cast<void*>(m_vertexBuffer.resource->GetGPUVirtualAddress()),
                        reinterpret_cast<void*>(m_indexBuffer.resource->GetGPUVirtualAddress()),
                        shaderArguments.heap,
                        shaderArguments.globalBuffer,
                        shaderArguments.instanceBuffer
                    });
}

void MeshObject::CreateBLAS(ComPtr<ID3D12GraphicsCommandList4> commandList)
{
    REQUIRE(IsMeshModified());

    if (m_vertexCount == 0 || m_indexCount == 0)
    {
        m_blas = {};
        return;
    }
    
    m_blas = CreateBottomLevelAS(commandList,
                                 {{m_vertexBuffer, m_vertexCount}},
                                 {{m_indexBuffer, m_indexCount}});
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

    for (size_t i = 0; i < vertexBuffers.size(); i++)
    {
        auto& [buffer, count] = vertexBuffers[i];

        const bool isOpaque = false;

        if (auto [indexBuffer, indexCount] = i < indexBuffers.size()
                                                 ? indexBuffers[i]
                                                 : std::make_pair(Allocation<ID3D12Resource>(), uint32_t());
            indexCount > 0)
        {
            bottomLevelAS.AddVertexBuffer(buffer.Get(), 0, count, sizeof(SpatialVertex),
                                          indexBuffer.Get(), 0, indexCount, nullptr, 0, isOpaque);
        }
        else
        {
            bottomLevelAS.AddVertexBuffer(buffer.Get(), 0, count, sizeof(SpatialVertex), nullptr, 0, isOpaque);
        }
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
