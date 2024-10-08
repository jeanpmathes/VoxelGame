// <copyright file="ContextTests.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Logic.Sections;

namespace VoxelGame.Core.Tests.Generation.Worlds;

public class ContextTestBase
{
    protected static Chunk CreateInitializedChunk(ChunkContext context, ChunkPosition position)
    {
        Chunk chunk = new(context, blocks => new Section(blocks));

        chunk.Initialize(null!, position);

        return chunk;
    }

    protected static Chunk CreateChunk(ChunkContext ctx)
    {
        return new Chunk(ctx, blocks => new Section(blocks));
    }
}
