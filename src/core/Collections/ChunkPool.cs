// <copyright file="ChunkPool.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Toolkit.Memory;
using VoxelGame.Toolkit.Utilities;
using Chunk = VoxelGame.Core.Logic.Chunks.Chunk;

namespace VoxelGame.Core.Collections;

/// <summary>
///     A chunk pool stores chunks for reuse.
/// </summary>
public sealed class ChunkPool : IDisposable
{
    private readonly NativeAllocator allocator = new();
    private readonly Stack<Allocation> allocations = new();

    private readonly Func<NativeSegment<UInt32>, Chunk> factory;
    private readonly ObjectPool<Chunk> chunks;

    /// <summary>
    ///     Create a new chunk pool.
    /// </summary>
    /// <param name="factory">The factory to use for creating chunks.</param>
    public ChunkPool(Func<NativeSegment<UInt32>, Chunk> factory)
    {
        this.factory = factory;

        chunks = new ObjectPool<Chunk>(CreateChunk);

        allocations.Push(new Allocation(allocator));
    }

    private Chunk CreateChunk()
    {
        if (allocations.Peek().IsExhausted) allocations.Push(new Allocation(allocator));

        return factory(allocations.Peek().GetNextSegment());
    }

    /// <summary>
    ///     Get a chunk from the pool.
    ///     It will be initialized and ready to use.
    /// </summary>
    /// <param name="world">The world in which the chunk will be placed.</param>
    /// <param name="position">The position at which the chunk will be placed.</param>
    /// <returns>A chunk.</returns>
    public Chunk Get(World world, ChunkPosition position)
    {
        Throw.IfDisposed(disposed);

        Chunk chunk = chunks.Get();

        chunk.Initialize(world, position);

        return chunk;
    }

    /// <summary>
    ///     Return a chunk to the pool.
    /// </summary>
    /// <param name="chunk">A chunk that was previously gotten from the pool.</param>
    public void Return(Chunk chunk)
    {
        Throw.IfDisposed(disposed);

        chunk.Reset();

        chunks.Return(chunk);
    }

    private sealed class Allocation(NativeAllocator allocator)
    {
        private const Int32 ChunksPerAllocation = 64;
        private const Int32 BlocksPerChunk = Chunk.BlockSize * Chunk.BlockSize * Chunk.BlockSize;
        private const Int32 AllocationCount = ChunksPerAllocation * BlocksPerChunk;

        private readonly NativeAllocation<UInt32> allocation = allocator.Allocate<UInt32>(AllocationCount);

        private Int32 nextChunkIndex;

        public Boolean IsExhausted => nextChunkIndex >= ChunksPerAllocation;

        public NativeSegment<UInt32> GetNextSegment()
        {
            Int32 offset = nextChunkIndex * BlocksPerChunk;

            nextChunkIndex += 1;

            return allocation.Segment.Slice(offset, BlocksPerChunk);
        }
    }

    #region DISPOSABLE

    private Boolean disposed;

    /// <inheritdoc />
    public void Dispose()
    {
        Chunk[] cleared = chunks.Clear();

        foreach (Chunk chunk in cleared)
            chunk.Dispose();

        allocator.Dispose();

        disposed = true;
    }

    #endregion DISPOSABLE
}
