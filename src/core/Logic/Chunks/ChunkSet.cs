// <copyright file="ChunkSet.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Chunks;

/// <summary>
///     Stores all chunks currently handled by the game.
/// </summary>
public sealed class ChunkSet : IDisposable
{
    private readonly Dictionary<ChunkPosition, Chunk> chunks = new();

    private readonly World world;
    private readonly ChunkContext context;

    private readonly Bag<Chunk> active = new(null!);
    private readonly Bag<Chunk> complete = new(null!);

    /// <summary>
    ///     Create a new chunk set.
    /// </summary>
    /// <param name="world">The world in which the chunks exist.</param>
    /// <param name="context">The context in which to create chunks.</param>
    public ChunkSet(World world, ChunkContext context)
    {
        this.world = world;
        this.context = context;
    }

    /// <summary>
    ///     Get all currently existing chunks.
    /// </summary>
    public IEnumerable<Chunk> All => chunks.Values;

    /// <summary>
    ///     Get the number of active chunks.
    /// </summary>
    public Int32 ActiveCount => active.Count;

    /// <summary>
    ///     Get whether there are no chunks, neither active nor inactive.
    /// </summary>
    public Boolean IsEmpty => chunks.Count == 0;

    /// <summary>
    ///     Request that a position has an active chunk.
    /// </summary>
    /// <param name="position">The position to request.</param>
    public void Request(ChunkPosition position)
    {
        Throw.IfDisposed(disposed);

        for (Int32 x = -1; x <= 1; x++)
        for (Int32 y = -1; y <= 1; y++)
        for (Int32 z = -1; z <= 1; z++)
        {
            ChunkPosition current = position.Offset(x, y, z);

            var level = RequestLevel.Loaded;

            if (current == position)
                level = RequestLevel.Active;

            RequestDirect(current, level);
        }
    }

    private void RequestDirect(ChunkPosition position, RequestLevel level)
    {
        if (!chunks.TryGetValue(position, out Chunk? chunk))
        {
            chunk = context.GetObject(world, position);
            chunks.Add(position, chunk);
        }

        chunk.RaiseRequestLevel(level);
    }

    /// <summary>
    ///     Release a chunk. If the chunk does not exist, nothing happens.
    /// </summary>
    /// <param name="position">The position of the chunk to release.</param>
    public void Release(ChunkPosition position)
    {
        Throw.IfDisposed(disposed);

        // First, we go down to loaded, as we might not need to completely release the chunk.

        if (chunks.TryGetValue(position, out Chunk? chunk))
            chunk.LowerRequestLevel(RequestLevel.Loaded);

        // Then, we check the level for all neighbors.

        for (Int32 x = -1; x <= 1; x++)
        for (Int32 y = -1; y <= 1; y++)
        for (Int32 z = -1; z <= 1; z++)
        {
            ChunkPosition current = position.Offset(x, y, z);

            if (!chunks.TryGetValue(current, out chunk)) return;

            UpdateAfterNeighborRelease(chunk);
        }
    }

    private void UpdateAfterNeighborRelease(Chunk chunk)
    {
        // If the chunk is active, that was explicitly requested, so we don't change it.
        if (chunk.RequestLevel == RequestLevel.Active) return;

        // A release can only lower the request level, so nothing to do if already at minimum.
        if (chunk.RequestLevel == RequestLevel.None) return;

        var max = RequestLevel.None;

        for (Int32 x = -1; x <= 1; x++)
        for (Int32 y = -1; y <= 1; y++)
        for (Int32 z = -1; z <= 1; z++)
            SetMax(x, y, z);

        chunk.SetRequestLevel(max);

        void SetMax(Int32 x, Int32 y, Int32 z)
        {
            if (x == 0 && y == 0 && z == 0) return;

            if (chunks.TryGetValue(chunk.Position.Offset(x, y, z), out Chunk? neighbor) && neighbor.RequestLevel > max)
                max = neighbor.RequestLevel - 1;
        }
    }

    /// <summary>
    ///     Get a chunk. If the chunk does not exist, null is returned.
    /// </summary>
    /// <param name="position">The position of the chunk to get.</param>
    /// <returns>The chunk, or null if it does not exist.</returns>
    private Chunk? Get(ChunkPosition position)
    {
        Throw.IfDisposed(disposed);

        Throw.IfNotOnMainThread(this);

        return chunks.GetValueOrDefault(position);
    }

    /// <summary>
    ///     Get an active chunk. This request may only be called from the main thread, and the chunk is only safe to use during
    ///     the current update.
    ///     All operations are allowed on the chunk, assume that read and write access is available on all resources.
    /// </summary>
    /// <param name="position">The position of the chunk to get.</param>
    /// <returns>The chunk, or null if it does not exist or is not active.</returns>
    public Chunk? GetActive(ChunkPosition position)
    {
        Throw.IfDisposed(disposed);

        Chunk? chunk = Get(position);

        return chunk?.IsActive == true ? chunk : null;
    }

    /// <summary>
    ///     Get a chunk. This request may only be called from the main thread.
    ///     No operations should be performed on the chunk without acquiring the necessary resources first.
    /// </summary>
    /// <param name="position">The position of the chunk to get.</param>
    /// <returns>The chunk, or null if it does not exist.</returns>
    public Chunk? GetAny(ChunkPosition position)
    {
        Throw.IfDisposed(disposed);

        return Get(position);
    }

    /// <summary>
    ///     Unload a chunk.
    /// </summary>
    /// <param name="chunk">The chunk to unload.</param>
    public void Unload(Chunk chunk)
    {
        Throw.IfDisposed(disposed);

        Debug.Assert(!chunk.IsActive);
        Debug.Assert(!chunk.IsRequestedToLoad);

        chunks.Remove(chunk.Position);
        context.ReturnObject(chunk);
    }

    /// <summary>
    ///     Begin saving all chunks.
    /// </summary>
    public void BeginSaving()
    {
        Throw.IfDisposed(disposed);

        foreach (Chunk chunk in chunks.Values)
        {
            chunk.BeginSaving();
        }
    }

    /// <summary>
    ///     Perform an action on all active chunks.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    public void ForEachActive(Action<Chunk> action)
    {
        foreach (Chunk chunk in active)
            action(chunk);
    }

    /// <summary>
    ///     Perform an action on all complete chunks.
    ///     A complete chunk is a chunk that has been activated at least once.
    ///     It may not be active at the moment.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    public void ForEachComplete(Action<Chunk> action)
    {
        foreach (Chunk chunk in complete)
            action(chunk);
    }

    /// <summary>
    ///     Register a chunk as active.
    ///     This will add it to the active list.
    ///     May only be called once before unregistering.
    /// </summary>
    /// <param name="chunk">The chunk to register as active.</param>
    /// <returns>The index of the chunk in the active list.</returns>
    internal Int32 RegisterActive(Chunk chunk)
    {
        return active.Add(chunk);
    }

    /// <summary>
    ///     Unregister a chunk as active.
    ///     This will remove it from the active list.
    ///     Can only be called once per registration.
    /// </summary>
    /// <param name="index">The index of the chunk in the active list.</param>
    internal void UnregisterActive(Int32 index)
    {
        active.RemoveAt(index);
    }

    /// <summary>
    ///     Register a chunk as complete.
    ///     This will add it to the complete list.
    ///     May only be called once before unregistering.
    /// </summary>
    /// <param name="chunk">The chunk to register as complete.</param>
    /// <returns>The index of the chunk in the complete list.</returns>
    internal Int32 RegisterComplete(Chunk chunk)
    {
        return complete.Add(chunk);
    }

    /// <summary>
    ///     Unregister a chunk as complete.
    ///     This will remove it from the complete list.
    ///     Can only be called once per registration.
    /// </summary>
    /// <param name="index">The index of the chunk in the complete list.</param>
    internal void UnregisterComplete(Int32 index)
    {
        complete.RemoveAt(index);
    }

    #region IDisposable Support

    private Boolean disposed;

    /// <summary>
    ///     Dispose of the chunks.
    /// </summary>
    /// <param name="disposing">True when disposing intentionally.</param>
    private void Dispose(Boolean disposing)
    {
        if (disposed) return;

        Debug.Assert(chunks.Count == 0);

        if (disposing)
            foreach (Chunk chunk in chunks.Values)
                chunk.Dispose();

        disposed = true;
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~ChunkSet()
    {
        Dispose(disposing: false);
    }

    /// <summary>
    ///     Dispose of the chunks.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable Support
}
