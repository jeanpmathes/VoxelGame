#include "stdafx.h"

Effect::Effect(NativeClient& client)
    : Drawable(client)
{
    m_instanceConstantDataBufferAlignedSize = sizeof EffectDataBuffer;
    m_instanceConstantDataBuffer            = util::AllocateConstantBuffer(GetClient(), &m_instanceConstantDataBufferAlignedSize);
    NAME_D3D12_OBJECT_WITH_ID(m_instanceConstantDataBuffer);

    m_instanceConstantDataBufferView.BufferLocation = m_instanceConstantDataBuffer.GetGPUVirtualAddress();
    m_instanceConstantDataBufferView.SizeInBytes    = static_cast<UINT>(m_instanceConstantDataBufferAlignedSize);

    TryDo(m_instanceConstantDataBuffer.Map(&m_instanceConstantBufferMapping, 1));

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
    Camera const& camera = *GetClient().GetSpace()->GetCamera();
    
    DirectX::XMMATRIX const m  = XMLoadFloat4x4(&GetTransform());
    DirectX::XMMATRIX const vp = XMLoadFloat4x4(&camera.GetViewProjectionMatrix());

    DirectX::XMFLOAT4X4 pvm;
    XMStoreFloat4x4(&pvm, XMMatrixTranspose(m * vp));

    m_instanceConstantBufferMapping.Write({
        .pvm = pvm,
        .zNear = camera.GetNearPlane(),
        .zFar  = camera.GetFarPlane()
    });
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
        &m_instanceConstantDataBufferView);

    D3D12_RESOURCE_BARRIER const transitionShaderResourceToVertexBuffer = {
        CD3DX12_RESOURCE_BARRIER::Transition(
            m_geometryBuffer.Get(),
            D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE,
            D3D12_RESOURCE_STATE_VERTEX_AND_CONSTANT_BUFFER)
    };
    commandList->ResourceBarrier(1, &transitionShaderResourceToVertexBuffer);

    commandList->IASetVertexBuffers(0, 1, &m_geometryVBV);
    commandList->DrawInstanced(GetDataElementCount(), 1, 0, 0);
}

void Effect::Accept(Visitor& visitor) { visitor.Visit(*this); }

void Effect::DoDataUpload(
    ComPtr<ID3D12GraphicsCommandList> const& commandList,
    std::vector<D3D12_RESOURCE_BARRIER>*     barriers)
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
    barriers->push_back(transitionCopyDestToShaderResource);

    m_geometryVBV.SizeInBytes    = static_cast<UINT>(geometryBufferSize);
    m_geometryVBV.BufferLocation = m_geometryBuffer.GetGPUVirtualAddress();
}

void Effect::DoReset()
{
    m_pipeline = nullptr;

    // Instance buffer is intentionally not reset, because it is reused.

    m_geometryBuffer = {};
}
