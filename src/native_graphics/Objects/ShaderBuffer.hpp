// <copyright file="ShaderBuffer.hpp" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
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
    void SetData(std::byte const* data) const;

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
    UINT                            m_size;
    Allocation<ID3D12Resource>      m_constantBuffer;
    D3D12_CONSTANT_BUFFER_VIEW_DESC m_cbvDesc = {};
};
