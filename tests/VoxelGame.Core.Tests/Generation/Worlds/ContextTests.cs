// <copyright file="ContextTests.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Toolkit.Memory;

namespace VoxelGame.Core.Tests.Generation.Worlds;

public class ContextTestBase : IDisposable
{
    private readonly NativeAllocator allocator;

    protected ContextTestBase()
    {
        allocator = new NativeAllocator();
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected Chunk CreateInitializedChunk(ChunkContext context, ChunkPosition position)
    {
        Chunk chunk = new(context, allocator.Allocate<UInt32>(Chunk.BlockCount).Segment, blocks => new Section(blocks));

        chunk.Initialize(null!, position);

        return chunk;
    }

    protected static Chunk CreateChunk(NativeSegment<UInt32> segment, ChunkContext ctx)
    {
        return new Chunk(ctx, segment, blocks => new Section(blocks));
    }

    protected virtual void Dispose(Boolean disposing)
    {
        if (disposing) allocator.Dispose();
    }
}
