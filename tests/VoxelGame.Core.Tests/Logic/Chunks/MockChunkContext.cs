// <copyright file="MockChunkContext.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Tests.Generation;

namespace VoxelGame.Core.Tests.Logic.Chunks;

public static class MockChunkContext
{
    public static ChunkContext Create(ChunkContext.ChunkFactory factory)
    {
        return new ChunkContext(new MockWorldGenerator(), factory, _ => null, _ => null, _ => {});
    }
}
