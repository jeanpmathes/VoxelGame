#include "stdafx.h"

ShaderBuffer::ShaderBuffer(NativeClient& client, const UINT size)
    : Object(client), m_size(size)
{
    UINT64 alignedSize = size;
    m_constantBuffer = util::AllocateConstantBuffer(GetClient(), &alignedSize);
    NAME_D3D12_OBJECT_WITH_ID(m_constantBuffer);

    REQUIRE(alignedSize <= UINT_MAX);
    m_size = static_cast<UINT>(alignedSize);

    m_cbvDesc.BufferLocation = m_constantBuffer.resource->GetGPUVirtualAddress();
    m_cbvDesc.SizeInBytes = m_size;
}

void ShaderBuffer::CreateResourceView(const DescriptorHeap& heap) const
{
    GetClient().GetDevice()->CreateConstantBufferView(&m_cbvDesc, heap.GetDescriptorHandleCPU(0));
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
