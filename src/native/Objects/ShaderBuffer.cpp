#include "stdafx.h"

ShaderBuffer::ShaderBuffer(NativeClient& client, const uint64_t size)
    : Object(client), m_size(size)
{
    uint64_t alignedSize = size;

    m_constantBuffer = nv_helpers_dx12::CreateConstantBuffer(
        client.GetDevice().Get(), &alignedSize,
        D3D12_RESOURCE_FLAG_NONE, D3D12_RESOURCE_STATE_GENERIC_READ,
        nv_helpers_dx12::kUploadHeapProps);

    m_cbvDesc.BufferLocation = m_constantBuffer->GetGPUVirtualAddress();
    m_cbvDesc.SizeInBytes = static_cast<UINT>(alignedSize);
}

void ShaderBuffer::CreateResourceView(ComPtr<ID3D12DescriptorHeap> heap) const
{
    GetClient().GetDevice()->CreateConstantBufferView(&m_cbvDesc, heap->GetCPUDescriptorHandleForHeapStart());
}

void ShaderBuffer::SetData(const void* data) const
{
    uint8_t* pData;
    TRY_DO(m_constantBuffer->Map(0, nullptr, reinterpret_cast<void**>(&pData)));

    memcpy(pData, data, m_size);

    m_constantBuffer->Unmap(0, nullptr);
}
