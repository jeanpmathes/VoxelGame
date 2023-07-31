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
     * Create a resource view for the buffer.
     */
    void CreateResourceView(const DescriptorHeap& heap) const;

    /**
     * Set the data of the buffer.
     */
    void SetData(const void* data) const;

    /**
     * Get the GPU virtual address of the buffer.
     */
    [[nodiscard]] D3D12_GPU_VIRTUAL_ADDRESS GetGPUVirtualAddress() const;

private:
    UINT m_size;
    Allocation<ID3D12Resource> m_constantBuffer;
    D3D12_CONSTANT_BUFFER_VIEW_DESC m_cbvDesc = {};
};
