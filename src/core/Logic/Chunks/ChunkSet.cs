// <copyright file="ChunkSet.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Collections;
using VoxelGame.Logging;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Chunks;

/// <summary>
///     Stores all chunks currently handled by the game.
///     Handles requesting and releasing of chunks.
/// </summary>
public sealed partial class ChunkSet : IDisposable
{
    private readonly Dictionary<ChunkPosition, Chunk> chunks = new();

    private readonly World world;
    private readonly ChunkContext context;

    private readonly Bag<Chunk> active = new(null!);
    private readonly Bag<Chunk> complete = new(null!);

    private readonly RequestAlgorithm requests;

    private readonly HashSet<Request> pendingRequests = [];
    private readonly HashSet<Request> pendingReleases = [];

    /// <summary>
    ///     Create a new chunk set.
    /// </summary>
    /// <param name="world">The world in which the chunks exist.</param>
    /// <param name="context">The context in which to create chunks.</param>
    public ChunkSet(World world, ChunkContext context)
    {
        this.world = world;
        this.context = context;

        requests = new RequestAlgorithm(
            position => Get(position)?.Requests,
            position => GetOrCreate(position).Requests
        );
    }

    /// <summary>
    ///     Get all currently existing chunks.
    /// </summary>
    public IEnumerable<Chunk> All => chunks.Values;

    /// <summary>
    ///     Get whether there are no chunks, neither active nor inactive.
    /// </summary>
    public Boolean IsEmpty => chunks.Count == 0;

    /// <summary>
    ///     Request that a position has an active chunk.
    ///     This will spread out to the neighbors.
    /// </summary>
    /// <param name="position">The position to request.</param>
    /// <param name="requester">The actor requesting the chunk.</param>
    public Request? Request(ChunkPosition position, Actor requester)
    {
        Throw.IfDisposed(disposed);

        Request request = new(position, requester);

        if (pendingRequests.Add(request))
            return request;

        LogDuplicateChunkRequest(logger, position, requester);

        return null;
    }

    /// <summary>
    ///     Release a previously made request.
    ///     All chunks that were requested by this request may be released as well.
    /// </summary>
    /// <param name="request">The request to release.</param>
    public void Release(Request request)
    {
        Throw.IfDisposed(disposed);

        if (pendingRequests.Remove(request))
            return;

        pendingReleases.Add(request);
    }

    /// <summary>
    ///     Process all requests.
    /// </summary>
    public void ProcessRequests()
    {
        requests.Process(pendingRequests, pendingReleases);

        pendingRequests.Clear();
        pendingReleases.Clear();
    }

    /// <summary>
    ///     Get a chunk. If the chunk does not exist, null is returned.
    /// </summary>
    /// <param name="position">The position of the chunk to get.</param>
    /// <returns>The chunk, or null if it does not exist.</returns>
    private Chunk? Get(ChunkPosition position)
    {
        Throw.IfDisposed(disposed);
        ApplicationInformation.ThrowIfNotOnMainThread(this);

        return chunks.GetValueOrDefault(position);
    }

    private Chunk GetOrCreate(ChunkPosition position)
    {
        Chunk? chunk = Get(position);

        if (chunk != null)
            return chunk;

        chunk = context.GetObject(world, position);

        chunks[position] = chunk;

        return chunk;
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
    /// </summary>
    /// <returns></returns>
    public Boolean IsEveryChunkToSimulateActive()
    {
        foreach (Chunk chunk in chunks.Values)
        {
            if (!chunk.IsRequestedToSimulate)
                continue;

            if (!chunk.IsActive)
                return false;
        }

        return true;
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

    #region DISPOSABLE

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

    #endregion DISPOSABLE

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<ChunkSet>();

    [LoggerMessage(EventId = LogID.ChunkSet + 0, Level = LogLevel.Warning, Message = "Chunk {Chunk} already requested by {Actor}, ignoring")]
    private static partial void LogDuplicateChunkRequest(ILogger logger, ChunkPosition chunk, Actor actor);

    #endregion LOGGING
}
