#include "stdafx.h"

Effect::Effect(NativeClient& client)
    : Drawable(client)
{
    m_instanceDataBufferAlignedSize = sizeof MeshDataBuffer;
    m_instanceDataBuffer            = util::AllocateConstantBuffer(GetClient(), &m_instanceDataBufferAlignedSize);
    NAME_D3D12_OBJECT_WITH_ID(m_instanceDataBuffer);

    m_instanceDataBufferView.BufferLocation = m_instanceDataBuffer.GetGPUVirtualAddress();
    m_instanceDataBufferView.SizeInBytes    = static_cast<UINT>(m_instanceDataBufferAlignedSize);

    TryDo(m_instanceDataBuffer.Map(&m_instanceConstantBufferMapping, 1));

    {
        m_geometryVBV.StrideInBytes = sizeof(SpatialVertex);
    }
}

void Effect::Initialize(RasterPipeline& pipeline)
{
    Require(pipeline.GetPreset() == ShaderPreset::SPATIAL_EFFECT);
    m_pipeline = &pipeline;
}

void Effect::Update()
{
    DirectX::XMMATRIX const m  = XMLoadFloat4x4(&GetTransform());
    DirectX::XMMATRIX const vp = XMLoadFloat4x4(&GetClient().GetSpace()->GetCamera()->GetViewProjectionMatrix());

    DirectX::XMFLOAT4X4 mvp;
    XMStoreFloat4x4(&mvp, XMMatrixTranspose(m * vp));

    m_instanceConstantBufferMapping.Write({mvp});
}

void Effect::SetNewVertices(EffectVertex const* vertices, UINT const vertexCount)
{
    if (bool const uploadRequired = HandleModification(vertexCount);
        !uploadRequired)
        return;

    auto const vertexBufferSize = sizeof(SpatialVertex) * vertexCount;
    util::ReAllocateBuffer(
        &GetUploadDataBuffer(),
        GetClient(),
        vertexBufferSize,
        D3D12_RESOURCE_FLAG_NONE,
        D3D12_RESOURCE_STATE_GENERIC_READ,
        D3D12_HEAP_TYPE_UPLOAD);
    NAME_D3D12_OBJECT_WITH_ID(GetUploadDataBuffer());

    TryDo(util::MapAndWrite(GetUploadDataBuffer(), vertices, vertexCount));
}

void Effect::Draw(ComPtr<ID3D12GraphicsCommandList4> const& commandList) const
{
    PIXScopedEvent(commandList.Get(), PIX_COLOR_DEFAULT, m_pipeline->GetName());

    m_pipeline->SetPipeline(commandList);
    m_pipeline->BindResources(commandList);

    m_pipeline->CreateConstantBufferView(
        m_pipeline->GetBindings().SpatialEffect().instanceData,
        0,
        &m_instanceDataBufferView);

    commandList->IASetVertexBuffers(0, 1, &m_geometryVBV);
    commandList->DrawInstanced(GetDataElementCount(), 1, 0, 0);
}

void Effect::Accept(Visitor& visitor) { visitor.Visit(*this); }

void Effect::DoDataUpload(ComPtr<ID3D12GraphicsCommandList> commandList)
{
    if (GetDataElementCount() == 0)
    {
        m_geometryBuffer = {};
        return;
    }

    auto const geometryBufferSize = GetUploadDataBuffer().resource->GetDesc().Width;

    util::ReAllocateBuffer(
        &m_geometryBuffer,
        GetClient(),
        geometryBufferSize,
        D3D12_RESOURCE_FLAG_NONE,
        D3D12_RESOURCE_STATE_COPY_DEST,
        D3D12_HEAP_TYPE_DEFAULT);
    NAME_D3D12_OBJECT_WITH_ID(m_geometryBuffer);

    commandList->CopyBufferRegion(m_geometryBuffer.Get(), 0, GetUploadDataBuffer().Get(), 0, geometryBufferSize);

    D3D12_RESOURCE_BARRIER const transitionCopyDestToShaderResource = {
        CD3DX12_RESOURCE_BARRIER::Transition(
            m_geometryBuffer.Get(),
            D3D12_RESOURCE_STATE_COPY_DEST,
            D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE)
    };
    commandList->ResourceBarrier(1, &transitionCopyDestToShaderResource);

    m_geometryVBV.SizeInBytes    = static_cast<UINT>(geometryBufferSize);
    m_geometryVBV.BufferLocation = m_geometryBuffer.GetGPUVirtualAddress();
}

void Effect::DoReset()
{
    m_pipeline = nullptr;

    // Instance buffer is intentionally not reset, because it is reused.

    m_geometryBuffer = {};
}
