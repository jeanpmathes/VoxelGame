// <copyright file="WorldChunkManagement.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic;

public abstract partial class World
{
    private const uint ChunkLimit = BlockLimit / Chunk.BlockSize;

    private readonly ChunkSet chunks;

    /// <summary>
    ///     Get the active chunk count.
    /// </summary>
    protected int ActiveChunkCount => chunks.ActiveCount;

    /// <summary>
    ///     All active chunks.
    /// </summary>
    protected IEnumerable<Chunk> ActiveChunks => chunks.AllActive;

    /// <summary>
    /// The max generation task limit.
    /// </summary>
    public Limit MaxGenerationTasks { get; }

    /// <summary>
    /// The max loading task limit.
    /// </summary>
    public Limit MaxLoadingTasks { get; }

    /// <summary>
    /// The max saving task limit.
    /// </summary>
    public Limit MaxSavingTasks { get; }

    /// <summary>
    ///     Creates a chunk for a chunk position.
    /// </summary>
    protected abstract Chunk CreateChunk(ChunkPosition position, ChunkContext context);

    private static bool IsInLimits(ChunkPosition position)
    {
        return Math.Abs(position.X) <= ChunkLimit && Math.Abs(position.Y) <= ChunkLimit && Math.Abs(position.Z) <= ChunkLimit;
    }

    /// <summary>
    ///     Process a chunk that has been just activated.
    /// </summary>
    protected abstract ChunkState ProcessNewlyActivatedChunk(Chunk activatedChunk);

    /// <summary>
    ///     Process a chunk that has just switched to the active state trough a weak activation.
    /// </summary>
    protected abstract void ProcessActivatedChunk(Chunk activatedChunk);

    /// <summary>
    ///     Requests the activation of a chunk. This chunk will either be loaded or generated.
    /// </summary>
    /// <param name="position">The position of the chunk.</param>
    public void RequestChunk(ChunkPosition position)
    {
        if (!IsInLimits(position)) return;

        chunks.Request(position);

        logger.LogDebug(Events.ChunkRequest, "Chunk {Position} has been requested successfully", position);
    }

    /// <summary>
    ///     Notifies the world that a chunk is no longer needed. The world decides if the chunk is deactivated.
    /// </summary>
    /// <param name="position">The position of the chunk.</param>
    public void ReleaseChunk(ChunkPosition position)
    {
        if (!IsInLimits(position)) return;

        // Check if the chunk can be released
        if (position == ChunkPosition.Origin) return; // The chunk at (0|0|0) cannot be released.

        chunks.Release(position);

        logger.LogDebug(Events.ChunkRelease, "Released chunk {Position}", position);
    }

    /// <summary>
    ///     Gets an active chunk.
    ///     See <see cref="ChunkSet.GetActive"/> for the restrictions.
    /// </summary>
    /// <param name="position">The position of the chunk.</param>
    /// <returns>The chunk at the given position or null if no active chunk was found.</returns>
    public Chunk? GetActiveChunk(ChunkPosition position)
    {
        return !IsInLimits(position) ? null : chunks.GetActive(position);
    }

    /// <summary>
    ///     Get the chunk that contains the specified block/fluid position.
    ///     See <see cref="ChunkSet.GetActive"/> for the restrictions.
    /// </summary>
    /// <param name="position">The block/fluid position.</param>
    /// <returns>The chunk, or null the position is not in an active chunk.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Chunk? GetActiveChunk(Vector3i position)
    {
        return IsInLimits(position) ? GetActiveChunk(ChunkPosition.From(position)) : null;
    }

    /// <summary>
    ///     Check if a chunk is active.
    /// </summary>
    /// <param name="position">The position of the chunk.</param>
    /// <returns>True if the chunk is active.</returns>
    protected bool IsChunkActive(ChunkPosition position)
    {
        return GetActiveChunk(position) != null;
    }

    /// <summary>
    ///     Try to get a chunk. The chunk is possibly not active.
    ///     See <see cref="ChunkSet.GetAny"/> for the restrictions.
    /// </summary>
    /// <param name="position">The position of the chunk.</param>
    /// <param name="chunk">The chunk at the given position or null if no chunk was found.</param>
    /// <returns>True if a chunk was found.</returns>
    public bool TryGetChunk(ChunkPosition position, [NotNullWhen(returnValue: true)] out Chunk? chunk)
    {
        chunk = chunks.GetAny(position);

        return chunk != null;
    }
}
