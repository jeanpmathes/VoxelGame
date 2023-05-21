#include "stdafx.h"

ShaderBuffer::ShaderBuffer(NativeClient& client, const uint64_t size)
    : Object(client), m_size(size)
{
    m_constantBuffer = util::AllocateConstantBuffer(GetClient(), &m_size);

    m_cbvDesc.BufferLocation = m_constantBuffer.resource->GetGPUVirtualAddress();
    m_cbvDesc.SizeInBytes = static_cast<UINT>(m_size);
}

void ShaderBuffer::CreateResourceView(const ComPtr<ID3D12DescriptorHeap> heap) const
{
    GetClient().GetDevice()->CreateConstantBufferView(&m_cbvDesc, heap->GetCPUDescriptorHandleForHeapStart());
}

void ShaderBuffer::SetData(const void* data) const
{
    uint8_t* pData;
    TRY_DO(m_constantBuffer.resource->Map(0, nullptr, reinterpret_cast<void**>(&pData)));

    memcpy(pData, data, m_size);

    m_constantBuffer.resource->Unmap(0, nullptr);
}

D3D12_GPU_VIRTUAL_ADDRESS ShaderBuffer::GetGPUVirtualAddress() const
{
    return m_constantBuffer.resource->GetGPUVirtualAddress();
}
