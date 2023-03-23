#include "stdafx.h"
#include "IndexedMeshObject.h"

IndexedMeshObject::IndexedMeshObject(NativeClient& client) : MeshObject(client)
{
}

void IndexedMeshObject::SetNewMesh(const SpatialVertex* vertices, UINT vertexCount, const UINT* indices,
                                   UINT indexCount)
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

bool IndexedMeshObject::IsMeshModified() const
{
    return m_vertexBufferUpload != nullptr && m_indexBufferUpload != nullptr;
}

void IndexedMeshObject::EnqueueMeshUpload(ComPtr<ID3D12GraphicsCommandList> commandList)
{
    assert(IsMeshModified());

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

void IndexedMeshObject::CleanupMeshUpload()
{
    m_vertexBufferUpload.Reset();
    m_indexBufferUpload.Reset();
}

void IndexedMeshObject::SetupHitGroup(nv_helpers_dx12::ShaderBindingTableGenerator& sbt,
                                      StandardShaderArguments& shaderArguments)
{
    sbt.AddHitGroup(L"IndexedHitGroup",
                    {
                        reinterpret_cast<void*>(m_vertexBuffer->GetGPUVirtualAddress()),
                        reinterpret_cast<void*>(m_indexBuffer->GetGPUVirtualAddress()),
                        shaderArguments.heap,
                        shaderArguments.globalBuffer,
                        shaderArguments.instanceBuffer
                    });
    sbt.AddHitGroup(L"IndexedShadowHitGroup",
                    {
                        reinterpret_cast<void*>(m_vertexBuffer->GetGPUVirtualAddress()),
                        reinterpret_cast<void*>(m_indexBuffer->GetGPUVirtualAddress()),
                        shaderArguments.heap,
                        shaderArguments.globalBuffer,
                        shaderArguments.instanceBuffer
                    });
}

void IndexedMeshObject::CreateBLAS(ComPtr<ID3D12GraphicsCommandList4> commandList)
{
    m_blas = CreateBottomLevelAS(commandList,
                                 {{m_vertexBuffer, m_vertexCount}},
                                 {{m_indexBuffer, m_indexCount}});
}

ComPtr<ID3D12Resource> IndexedMeshObject::GetBLAS()
{
    return m_blas.result;
}

ComPtr<ID3D12RootSignature> IndexedMeshObject::CreateRootSignature(ComPtr<ID3D12Device5> device)
{
    nv_helpers_dx12::RootSignatureGenerator rsc;

    rsc.AddRootParameter(D3D12_ROOT_PARAMETER_TYPE_SRV, 0); // Vertex Buffer
    rsc.AddRootParameter(D3D12_ROOT_PARAMETER_TYPE_SRV, 1); // Index Buffer

    rsc.AddHeapRangesParameter({
        {2, 1, 0, D3D12_DESCRIPTOR_RANGE_TYPE_SRV, 1}
    });

    rsc.AddRootParameter(D3D12_ROOT_PARAMETER_TYPE_CBV, 0); // Global Data
    rsc.AddRootParameter(D3D12_ROOT_PARAMETER_TYPE_CBV, 1); // Instance Data

    return rsc.Generate(device.Get(), true);
}
