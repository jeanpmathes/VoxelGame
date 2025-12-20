// <copyright file="SharedIndexBuffer.hpp" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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

class Space;

/**
 * A buffer of indices for quad meshes.
 * Because all quad meshes have the same index order, a common buffer can be used.
 */
class SharedIndexBuffer
{
public:
    explicit SharedIndexBuffer(Space& space);

    std::pair<Allocation<ID3D12Resource>, UINT> GetIndexBuffer(
        UINT                                 vertexCount,
        std::vector<D3D12_RESOURCE_BARRIER>* barriers);
    void CleanupRender();

private:
    Space& m_space;

    std::vector<UINT>                                                              m_indices            = {};
    Allocation<ID3D12Resource>                                                     m_sharedIndexBuffer  = {};
    UINT                                                                           m_sharedIndexCount   = 0;
    std::vector<std::pair<Allocation<ID3D12Resource>, Allocation<ID3D12Resource>>> m_indexBufferUploads = {};
};
