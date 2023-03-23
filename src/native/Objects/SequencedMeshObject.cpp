#include "stdafx.h"
#include "SequencedMeshObject.h"

SequencedMeshObject::SequencedMeshObject(NativeClient& client) : MeshObject(client)
{
}

void SequencedMeshObject::SetNewMesh(const SpatialVertex* vertices, const UINT vertexCount)
{
    const auto vertexBufferSize = sizeof(SpatialVertex) * vertexCount;

    m_vertexCount = vertexCount;

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

    UINT8* pVertexDataBegin;
    const CD3DX12_RANGE readRange(0, 0);
    TRY_DO(m_vertexBufferUpload->Map(0, &readRange, reinterpret_cast<void**>(&pVertexDataBegin)));
    memcpy(pVertexDataBegin, vertices, vertexBufferSize);
    m_vertexBufferUpload->Unmap(0, nullptr);
}

bool SequencedMeshObject::IsMeshModified() const
{
    return m_vertexBufferUpload != nullptr;
}

void SequencedMeshObject::EnqueueMeshUpload(ComPtr<ID3D12GraphicsCommandList> commandList)
{
    assert(IsMeshModified());

    const auto vertexBufferSize = m_vertexBufferUpload->GetDesc().Width;

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

    const auto transitionCommonToCopyDest = CD3DX12_RESOURCE_BARRIER::Transition(m_vertexBuffer.Get(),
        D3D12_RESOURCE_STATE_COMMON, D3D12_RESOURCE_STATE_COPY_DEST);
    commandList->ResourceBarrier(1, &transitionCommonToCopyDest);

    commandList->CopyBufferRegion(m_vertexBuffer.Get(), 0, m_vertexBufferUpload.Get(), 0, vertexBufferSize);

    const auto transitionCopyDestToBuffer = CD3DX12_RESOURCE_BARRIER::Transition(m_vertexBuffer.Get(),
        D3D12_RESOURCE_STATE_COPY_DEST, D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE);
    commandList->ResourceBarrier(1, &transitionCopyDestToBuffer);
}

void SequencedMeshObject::CleanupMeshUpload()
{
    m_vertexBufferUpload.Reset();
}

void SequencedMeshObject::SetupHitGroup(nv_helpers_dx12::ShaderBindingTableGenerator& sbt,
                                        StandardShaderArguments& shaderArguments)
{
    sbt.AddHitGroup(L"SequencedHitGroup",
                    {
                        reinterpret_cast<void*>(m_vertexBuffer->GetGPUVirtualAddress()),
                        shaderArguments.heap,
                        shaderArguments.globalBuffer,
                        shaderArguments.instanceBuffer
                    });
    sbt.AddHitGroup(L"SequencedShadowHitGroup",
                    {
                        reinterpret_cast<void*>(m_vertexBuffer->GetGPUVirtualAddress()),
                        shaderArguments.heap,
                        shaderArguments.globalBuffer,
                        shaderArguments.instanceBuffer
                    });
}

void SequencedMeshObject::CreateBLAS(ComPtr<ID3D12GraphicsCommandList4> commandList)
{
    m_blas = CreateBottomLevelAS(commandList,
                                 {{m_vertexBuffer, m_vertexCount}});
}

ComPtr<ID3D12Resource> SequencedMeshObject::GetBLAS()
{
    return m_blas.result;
}

ComPtr<ID3D12RootSignature> SequencedMeshObject::CreateRootSignature(ComPtr<ID3D12Device5> device)
{
    nv_helpers_dx12::RootSignatureGenerator rsc;

    rsc.AddRootParameter(D3D12_ROOT_PARAMETER_TYPE_SRV, 0); // Vertex Buffer

    rsc.AddHeapRangesParameter({
        {2, 1, 0, D3D12_DESCRIPTOR_RANGE_TYPE_SRV, 1}
    });

    rsc.AddRootParameter(D3D12_ROOT_PARAMETER_TYPE_CBV, 0); // Global Data
    rsc.AddRootParameter(D3D12_ROOT_PARAMETER_TYPE_CBV, 1); // Instance Data

    return rsc.Generate(device.Get(), true);
}
