#include "stdafx.h"
#include "MeshObject.h"

MeshObject::MeshObject(NativeClient& client, const UINT materialIndex)
    : SpatialObject(client), m_materialIndex(materialIndex)
{
    REQUIRE(GetClient().GetDevice() != nullptr);

    UINT64 alignedSize = sizeof m_instanceConstantBufferData;

    m_instanceConstantBuffer = nv_helpers_dx12::CreateConstantBuffer(GetClient().GetDevice().Get(),
                                                                     &alignedSize,
                                                                     D3D12_RESOURCE_FLAG_NONE,
                                                                     D3D12_RESOURCE_STATE_GENERIC_READ,
                                                                     nv_helpers_dx12::kUploadHeapProps);

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
        TRY_DO(m_instanceConstantBuffer->Map(0, nullptr, reinterpret_cast<void**>(&pData)));

        memcpy(pData, &m_instanceConstantBufferData, sizeof m_instanceConstantBufferData);

        m_instanceConstantBuffer->Unmap(0, nullptr);
    }
}

void MeshObject::SetNewMesh(const SpatialVertex* vertices, UINT vertexCount, const UINT* indices, UINT indexCount)
{
    const auto vertexBufferSize = sizeof(SpatialVertex) * vertexCount;
    const auto indexBufferSize = sizeof(UINT) * indexCount;

    m_vertexCount = vertexCount;
    m_indexCount = indexCount;

    const auto vertexBufferUploadHeapProps = CD3DX12_HEAP_PROPERTIES(D3D12_HEAP_TYPE_UPLOAD);
    const auto vertexBufferUploadDesc = CD3DX12_RESOURCE_DESC::Buffer(vertexBufferSize);
    TRY_DO(GetClient().GetDevice()->CreateCommittedResource(
        &vertexBufferUploadHeapProps,
        D3D12_HEAP_FLAG_NONE,
        &vertexBufferUploadDesc,
        D3D12_RESOURCE_STATE_GENERIC_READ,
        nullptr,
        IID_PPV_ARGS(&m_vertexBufferUpload)));

    NAME_D3D12_OBJECT_WITH_ID(m_vertexBufferUpload);

    auto indexBufferUploadHeapProps = CD3DX12_HEAP_PROPERTIES(D3D12_HEAP_TYPE_UPLOAD);
    auto indexBufferUploadDesc = CD3DX12_RESOURCE_DESC::Buffer(indexBufferSize);
    TRY_DO(GetClient().GetDevice()->CreateCommittedResource(
        &indexBufferUploadHeapProps,
        D3D12_HEAP_FLAG_NONE,
        &indexBufferUploadDesc,
        D3D12_RESOURCE_STATE_GENERIC_READ,
        nullptr,
        IID_PPV_ARGS(&m_indexBufferUpload)));

    NAME_D3D12_OBJECT_WITH_ID(m_indexBufferUpload);

    UINT8* pVertexDataBegin;
    CD3DX12_RANGE readRange(0, 0);
    TRY_DO(m_vertexBufferUpload->Map(0, &readRange, reinterpret_cast<void**>(&pVertexDataBegin)));
    memcpy(pVertexDataBegin, vertices, vertexBufferSize);
    m_vertexBufferUpload->Unmap(0, nullptr);

    UINT8* pIndexDataBegin;
    CD3DX12_RANGE readRange2(0, 0);
    TRY_DO(m_indexBufferUpload->Map(0, &readRange2, reinterpret_cast<void**>(&pIndexDataBegin)));
    memcpy(pIndexDataBegin, indices, indexBufferSize);
    m_indexBufferUpload->Unmap(0, nullptr);
}

bool MeshObject::IsMeshModified() const
{
    return m_vertexBufferUpload != nullptr && m_indexBufferUpload != nullptr;
}

void MeshObject::EnqueueMeshUpload(ComPtr<ID3D12GraphicsCommandList> commandList)
{
    REQUIRE(IsMeshModified());

    const auto vertexBufferSize = m_vertexBufferUpload->GetDesc().Width;
    const auto indexBufferSize = m_indexBufferUpload->GetDesc().Width;

    const auto vertexBufferHeapProps = CD3DX12_HEAP_PROPERTIES(D3D12_HEAP_TYPE_DEFAULT);
    const auto vertexBufferDesc = CD3DX12_RESOURCE_DESC::Buffer(vertexBufferSize);
    TRY_DO(GetClient().GetDevice()->CreateCommittedResource(
        &vertexBufferHeapProps,
        D3D12_HEAP_FLAG_NONE,
        &vertexBufferDesc,
        D3D12_RESOURCE_STATE_COMMON,
        nullptr,
        IID_PPV_ARGS(&m_vertexBuffer)));

    NAME_D3D12_OBJECT_WITH_ID(m_vertexBuffer);

    const auto indexBufferHeapProps = CD3DX12_HEAP_PROPERTIES(D3D12_HEAP_TYPE_DEFAULT);
    const auto indexBufferDesc = CD3DX12_RESOURCE_DESC::Buffer(indexBufferSize);
    TRY_DO(GetClient().GetDevice()->CreateCommittedResource(
        &indexBufferHeapProps,
        D3D12_HEAP_FLAG_NONE,
        &indexBufferDesc,
        D3D12_RESOURCE_STATE_COMMON,
        nullptr,
        IID_PPV_ARGS(&m_indexBuffer)));

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
    m_vertexBufferUpload.Reset();
    m_indexBufferUpload.Reset();
}

void MeshObject::FillArguments(StandardShaderArguments& shaderArguments) const
{
    shaderArguments.instanceBuffer = reinterpret_cast<void*>(m_instanceConstantBuffer->GetGPUVirtualAddress());
}

void MeshObject::SetupHitGroup(nv_helpers_dx12::ShaderBindingTableGenerator& sbt,
                               StandardShaderArguments& shaderArguments) const
{
    const Material& material = GetClient().GetSpace()->GetMaterial(m_materialIndex);

    sbt.AddHitGroup(material.normalHitGroup,
                    {
                        reinterpret_cast<void*>(m_vertexBuffer->GetGPUVirtualAddress()),
                        reinterpret_cast<void*>(m_indexBuffer->GetGPUVirtualAddress()),
                        shaderArguments.heap,
                        shaderArguments.globalBuffer,
                        shaderArguments.instanceBuffer
                    });
    sbt.AddHitGroup(material.shadowHitGroup,
                    {
                        reinterpret_cast<void*>(m_vertexBuffer->GetGPUVirtualAddress()),
                        reinterpret_cast<void*>(m_indexBuffer->GetGPUVirtualAddress()),
                        shaderArguments.heap,
                        shaderArguments.globalBuffer,
                        shaderArguments.instanceBuffer
                    });
}

void MeshObject::CreateBLAS(ComPtr<ID3D12GraphicsCommandList4> commandList)
{
    m_blas = CreateBottomLevelAS(commandList,
                                 {{m_vertexBuffer, m_vertexCount}},
                                 {{m_indexBuffer, m_indexCount}});
}

ComPtr<ID3D12Resource> MeshObject::GetBLAS()
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
                                                             std::vector<std::pair<ComPtr<ID3D12Resource>, uint32_t>>
                                                             vertexBuffers,
                                                             std::vector<std::pair<ComPtr<ID3D12Resource>, uint32_t>>
                                                             indexBuffers) const
{
    nv_helpers_dx12::BottomLevelASGenerator bottomLevelAS;

    for (size_t i = 0; i < vertexBuffers.size(); i++)
    {
        auto& [buffer, count] = vertexBuffers[i];

        if (auto [indexBuffer, indexCount] = i < indexBuffers.size() ? indexBuffers[i] : std::make_pair(nullptr, 0);
            indexCount > 0)
        {
            bottomLevelAS.AddVertexBuffer(buffer.Get(), 0, count, sizeof(SpatialVertex),
                                          indexBuffer.Get(), 0, indexCount, nullptr, 0, true);
        }
        else
        {
            bottomLevelAS.AddVertexBuffer(buffer.Get(), 0, count, sizeof(SpatialVertex), nullptr, 0);
        }
    }

    UINT64 scratchSizeInBytes = 0;
    UINT64 resultSizeInBytes = 0;
    bottomLevelAS.ComputeASBufferSizes(GetClient().GetDevice().Get(), false, &scratchSizeInBytes, &resultSizeInBytes);

    AccelerationStructureBuffers buffers;
    buffers.scratch = nv_helpers_dx12::CreateBuffer(GetClient().GetDevice().Get(), scratchSizeInBytes,
                                                    D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS,
                                                    D3D12_RESOURCE_STATE_COMMON,
                                                    nv_helpers_dx12::kDefaultHeapProps);
    buffers.result = nv_helpers_dx12::CreateBuffer(GetClient().GetDevice().Get(), resultSizeInBytes,
                                                   D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS,
                                                   D3D12_RESOURCE_STATE_RAYTRACING_ACCELERATION_STRUCTURE,
                                                   nv_helpers_dx12::kDefaultHeapProps);

    bottomLevelAS.Generate(commandList.Get(), buffers.scratch.Get(), buffers.result.Get(),
                           false, nullptr);
    return buffers;
}
