// <copyright file="ShaderBuffer.h" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

#include "Object.h"

/**
 * Abstraction for a cbuffer used in shaders.
 */
class ShaderBuffer : public Object
{
    DECLARE_OBJECT_SUBCLASS(ShaderBuffer)

public:
    ShaderBuffer(NativeClient& client, uint64_t size);

    /**
     * Set the data of the buffer.
     */
    void SetData(const void* data) const;

    /**
     * Use the data for rendering in the given command list.
     */
    void Use(ComPtr<ID3D12GraphicsCommandList4> commandList) const;

private:
    uint64_t m_size;

    ComPtr<ID3D12DescriptorHeap> m_descriptorHeap;
    ComPtr<ID3D12Resource> m_constantBuffer;
};
