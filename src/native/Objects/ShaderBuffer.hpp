// <copyright file="ShaderBuffer.hpp" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

#include "Object.hpp"

/**
 * Abstraction for a cbuffer used in shaders.
 */
class ShaderBuffer final : public Object
{
    DECLARE_OBJECT_SUBCLASS(ShaderBuffer)

public:
    ShaderBuffer(NativeClient& client, UINT size);

    /**
     * Set the data of the buffer.
     */
    void SetData(void const* data) const;

    /**
     * Get the GPU virtual address of the buffer.
     */
    [[nodiscard]] D3D12_GPU_VIRTUAL_ADDRESS GetGPUVirtualAddress() const;

    /**
     * \brief Get a descriptor for the buffer.
     * \return The descriptor.
     */
    [[nodiscard]] ShaderResources::ConstantBufferViewDescriptor GetDescriptor() const;

private:
    UINT                       m_size;
    Allocation<ID3D12Resource> m_constantBuffer;
    D3D12_CONSTANT_BUFFER_VIEW_DESC m_cbvDesc = {};
};
