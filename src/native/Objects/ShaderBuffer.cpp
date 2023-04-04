#include "stdafx.h"

ShaderBuffer::ShaderBuffer(NativeClient& client, const uint64_t size) : Object(client), m_size(size)
{
    m_constantBuffer = nv_helpers_dx12::CreateBuffer(
        client.GetDevice().Get(), size,
        D3D12_RESOURCE_FLAG_NONE, D3D12_RESOURCE_STATE_GENERIC_READ,
        nv_helpers_dx12::kUploadHeapProps);

    m_descriptorHeap = nv_helpers_dx12::CreateDescriptorHeap(
        client.GetDevice().Get(), 1,
        D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV, true);

    D3D12_CONSTANT_BUFFER_VIEW_DESC cbvDesc = {};
    cbvDesc.BufferLocation = m_constantBuffer->GetGPUVirtualAddress();
    cbvDesc.SizeInBytes = static_cast<UINT>(size);
    client.GetDevice()->CreateConstantBufferView(&cbvDesc, m_descriptorHeap->GetCPUDescriptorHandleForHeapStart());
}

void ShaderBuffer::SetData(const void* data) const
{
    uint8_t* pData;
    TRY_DO(m_constantBuffer->Map(0, nullptr, reinterpret_cast<void**>(&pData)));

    memcpy(pData, data, m_size);

    m_constantBuffer->Unmap(0, nullptr);
}

void ShaderBuffer::Use(ComPtr<ID3D12GraphicsCommandList4> commandList) const
{
    ID3D12DescriptorHeap* descriptorHeaps[] = {m_descriptorHeap.Get()};
    commandList->SetDescriptorHeaps(_countof(descriptorHeaps), descriptorHeaps);
    commandList->SetGraphicsRootDescriptorTable(0, m_descriptorHeap->GetGPUDescriptorHandleForHeapStart());
}
