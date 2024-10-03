// <copyright file="ChunkPool.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Utilities;
using Chunk = VoxelGame.Core.Logic.Chunks.Chunk;

namespace VoxelGame.Core.Collections;

/// <summary>
///     A chunk pool stores chunks for reuse.
/// </summary>
public sealed class ChunkPool : IDisposable
{
    private readonly ObjectPool<Chunk> pool;

    /// <summary>
    ///     Create a new chunk pool.
    /// </summary>
    /// <param name="factory">The factory to use for creating chunks.</param>
    public ChunkPool(Func<Chunk> factory)
    {
        pool = new ObjectPool<Chunk>(factory);
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

        Chunk chunk = pool.Get();

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

        pool.Return(chunk);
    }

    #region IDisposable Support

    private Boolean disposed;

    /// <inheritdoc />
    public void Dispose()
    {
        Chunk[] chunks = pool.Clear();

        foreach (Chunk chunk in chunks) chunk.Dispose();

        disposed = true;
    }

    #endregion IDisposable Support
}
