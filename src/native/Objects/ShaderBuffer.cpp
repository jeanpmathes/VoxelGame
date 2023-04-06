#include "stdafx.h"

ShaderBuffer::ShaderBuffer(NativeClient& client, const ComPtr<ID3D12DescriptorHeap> heap, const uint64_t size)
    : Object(client), m_size(size)
{
    uint64_t alignedSize = size;

    m_constantBuffer = nv_helpers_dx12::CreateConstantBuffer(
        client.GetDevice().Get(), &alignedSize,
        D3D12_RESOURCE_FLAG_NONE, D3D12_RESOURCE_STATE_GENERIC_READ,
        nv_helpers_dx12::kUploadHeapProps);

    D3D12_CONSTANT_BUFFER_VIEW_DESC cbvDesc;
    cbvDesc.BufferLocation = m_constantBuffer->GetGPUVirtualAddress();
    cbvDesc.SizeInBytes = static_cast<UINT>(alignedSize);
    client.GetDevice()->CreateConstantBufferView(&cbvDesc, heap->GetCPUDescriptorHandleForHeapStart());
}

void ShaderBuffer::SetData(const void* data) const
{
    uint8_t* pData;
    TRY_DO(m_constantBuffer->Map(0, nullptr, reinterpret_cast<void**>(&pData)));

    memcpy(pData, data, m_size);

    m_constantBuffer->Unmap(0, nullptr);
}
