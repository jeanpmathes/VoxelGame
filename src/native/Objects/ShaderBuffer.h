﻿// <copyright file="ShaderBuffer.h" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

#include "Object.h"

/**
 * Abstraction for a cbuffer used in shaders.
 */
class ShaderBuffer final : public Object
{
    DECLARE_OBJECT_SUBCLASS(ShaderBuffer)

public:
    ShaderBuffer(NativeClient& client, uint64_t size);

    /**
     * Create a resource view for the buffer.
     */
    void CreateResourceView(ComPtr<ID3D12DescriptorHeap> heap) const;

    /**
     * Set the data of the buffer.
     */
    void SetData(const void* data) const;

    /**
     * Get the GPU virtual address of the buffer.
     */
    [[nodiscard]] D3D12_GPU_VIRTUAL_ADDRESS GetGPUVirtualAddress() const;

private:
    uint64_t m_size;
    Allocation<ID3D12Resource> m_constantBuffer;
    D3D12_CONSTANT_BUFFER_VIEW_DESC m_cbvDesc = {};
};
