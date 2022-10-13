// <copyright file="ChunkSet.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic;

/// <summary>
///     Stores all chunks currently handled by the game.
/// </summary>
public sealed class ChunkSet : IDisposable
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<ChunkSet>();

    private readonly Dictionary<ChunkPosition, Chunk> chunks = new();
    private readonly ChunkContext context;

    /// <summary>
    ///     Create a new chunk set.
    /// </summary>
    /// <param name="context">The context in which to create chunks.</param>
    public ChunkSet(ChunkContext context)
    {
        this.context = context;
    }

    /// <summary>
    ///     Get the number of active chunks.
    /// </summary>
    public int ActiveCount => AllActive.Count();

    /// <summary>
    ///     All active chunks.
    /// </summary>
    public IEnumerable<Chunk> AllActive => chunks.Values.Where(c => c.IsActive);

    /// <summary>
    ///     Get whether there are no chunks, neither active nor inactive.
    /// </summary>
    public bool IsEmpty => chunks.Count == 0;

    /// <summary>
    ///     Request that a position has an active chunk.
    /// </summary>
    /// <param name="position">The position to request.</param>
    public void Request(ChunkPosition position)
    {
        if (!chunks.TryGetValue(position, out Chunk? chunk))
        {
            chunk = context.Create(position, context);
            chunks.Add(position, chunk);
        }

        chunk.AddRequest();
    }

    /// <summary>
    ///     Release a chunk. If the chunk does not exist, nothing happens.
    /// </summary>
    /// <param name="position">The position of the chunk to release.</param>
    public void Release(ChunkPosition position)
    {
        if (chunks.TryGetValue(position, out Chunk? chunk)) chunk.RemoveRequest();
    }

    /// <summary>
    ///     Get a chunk. If the chunk does not exist, null is returned.
    /// </summary>
    /// <param name="position">The position of the chunk to get.</param>
    /// <returns>The chunk, or null if it does not exist.</returns>
    private Chunk? Get(ChunkPosition position)
    {
        if (Thread.CurrentThread == ApplicationInformation.Instance.MainThread)
            return chunks.TryGetValue(position, out Chunk? chunk) ? chunk : null;

        logger.LogWarning("Attempted to acquire chunk '{Position}' from non-main thread", position);
        Debug.Fail("Attempted to acquire chunk from non-main thread.");

        return null;
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
        return Get(position);
    }

    /// <summary>
    ///     Update all chunks.
    /// </summary>
    public void Update()
    {
        foreach (Chunk chunk in chunks.Values)
            chunk.Update();
    }

    /// <summary>
    ///     Unload a chunk.
    /// </summary>
    /// <param name="chunk">The chunk to unload.</param>
    public void Unload(Chunk chunk)
    {
        Debug.Assert(!chunk.IsActive);
        Debug.Assert(!chunk.IsRequested);

        chunks.Remove(chunk.Position);
        chunk.Dispose();
    }

    /// <summary>
    ///     Begin saving all chunks.
    /// </summary>
    public void BeginSaving()
    {
        foreach (Chunk chunk in chunks.Values)
        {
            chunk.BeginSaving();
            chunk.RemoveRequest();
        }
    }

    #region IDisposable Support

    private bool disposed;

    /// <summary>
    ///     Dispose of the chunks.
    /// </summary>
    /// <param name="disposing">True when disposing intentionally.</param>
    private void Dispose(bool disposing)
    {
        if (disposed) return;

        if (disposing)
            foreach ((ChunkPosition _, Chunk chunk) in chunks)
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
