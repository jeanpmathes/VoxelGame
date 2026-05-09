#include "stdafx.h"

ShaderBuffer::ShaderBuffer(NativeClient& client, UINT const bufferSize)
    : Object(client)
  , size(bufferSize)
{
    UINT64 alignedSize = bufferSize;
    constantBuffer     = util::AllocateConstantBuffer(GetClient(), &alignedSize);
    NAME_D3D12_OBJECT_WITH_ID(constantBuffer);

    Require(alignedSize <= UINT_MAX);
    size = static_cast<UINT>(alignedSize);

    cbvDesc.BufferLocation = constantBuffer.GetGPUVirtualAddress();
    cbvDesc.SizeInBytes    = bufferSize;
}

void ShaderBuffer::SetData(std::byte const* data) const { TryDo(util::MapAndWrite(constantBuffer, data, size)); }

D3D12_GPU_VIRTUAL_ADDRESS ShaderBuffer::GetGPUVirtualAddress() const { return constantBuffer.GetGPUVirtualAddress(); }

ShaderResources::ConstantBufferViewDescriptor ShaderBuffer::GetDescriptor() const
{
    return {GetGPUVirtualAddress(), size};
}
