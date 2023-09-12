// <copyright file="SharedIndexBuffer.hpp" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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

    std::pair<Allocation<ID3D12Resource>, UINT> GetIndexBuffer(UINT vertexCount);
    void CleanupRenderSetup();

private:
    Space& m_space;

    std::vector<UINT> m_indices = {};
    Allocation<ID3D12Resource> m_sharedIndexBuffer = {};
    UINT m_sharedIndexCount = 0;
    std::vector<std::pair<Allocation<ID3D12Resource>, Allocation<ID3D12Resource>>> m_indexBufferUploads = {};
};
