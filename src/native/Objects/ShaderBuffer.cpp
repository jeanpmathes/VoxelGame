#include "stdafx.h"

ShaderBuffer::ShaderBuffer(NativeClient& client, const UINT size)
    : Object(client), m_size(size)
{
    UINT64 alignedSize = size;
    m_constantBuffer = util::AllocateConstantBuffer(GetClient(), &alignedSize);

    REQUIRE(alignedSize <= UINT_MAX);
    m_size = static_cast<UINT>(alignedSize);

    m_cbvDesc.BufferLocation = m_constantBuffer.resource->GetGPUVirtualAddress();
    m_cbvDesc.SizeInBytes = static_cast<UINT>(m_size);
}

void ShaderBuffer::CreateResourceView(const ComPtr<ID3D12DescriptorHeap> heap) const
{
    GetClient().GetDevice()->CreateConstantBufferView(&m_cbvDesc, heap->GetCPUDescriptorHandleForHeapStart());
}

void ShaderBuffer::SetData(const void* data) const
{
    auto* pData = static_cast<const std::byte*>(data);
    TRY_DO(util::MapAndWrite(m_constantBuffer, pData, m_size));
}

D3D12_GPU_VIRTUAL_ADDRESS ShaderBuffer::GetGPUVirtualAddress() const
{
    return m_constantBuffer.resource->GetGPUVirtualAddress();
}
