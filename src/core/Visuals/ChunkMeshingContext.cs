﻿// <copyright file="ChunkMeshingContext.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Linq;
using VoxelGame.Core.Generation;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Contains all the data needed to mesh the sections of a chunk.
/// </summary>
public class ChunkMeshingContext
{
    private readonly Chunk mid;
    private (Chunk chunk, Guard? guard)?[] neighbors;

    private ChunkMeshingContext(Chunk mid, (Chunk, Guard?)?[] neighbors, BlockSides availableSides, IMeshingFactory meshingFactory)
    {
        this.mid = mid;
        this.neighbors = neighbors;

        AvailableSides = availableSides;

        MeshingFactory = meshingFactory;
    }

    /// <summary>
    ///     Get the meshing factory which can be used to create meshing instances.
    /// </summary>
    public IMeshingFactory MeshingFactory { get; }

    /// <summary>
    ///     Get the sides at which neighbors are considered.
    /// </summary>
    public BlockSides AvailableSides { get; }

    /// <summary>
    ///     Get the map of the world.
    /// </summary>
    public IMap Map => mid.World.Map;

    /// <summary>
    ///     Acquire the chunks around the given chunk.
    ///     Use this method when meshing on a separate thread, but acquire on the main thread.
    /// </summary>
    /// <param name="chunk">The chunk to acquire the neighbors of. Must itself have sufficient access for meshing.</param>
    /// <param name="meshingFactory">The meshing factory to use.</param>
    /// <returns>A context that can be used to mesh the chunk.</returns>
    public static ChunkMeshingContext Acquire(Chunk chunk, IMeshingFactory meshingFactory)
    {
        var foundNeighbors = new (Chunk, Guard?)?[6];
        var availableSides = BlockSides.None;

        foreach (BlockSide side in BlockSide.All.Sides())
        {
            if (!chunk.World.TryGetChunk(side.Offset(chunk.Position), out Chunk? neighbor) || !neighbor.IsFullyDecorated) continue;

            Guard? guard = neighbor.AcquireCore(Access.Read);

            if (guard == null) continue;

            foundNeighbors[(Int32) side] = (neighbor, guard);
            availableSides |= side.ToFlag();
        }

        return new ChunkMeshingContext(chunk, foundNeighbors, availableSides, meshingFactory);
    }

    /// <summary>
    ///     Create a meshing context using the given chunk. Use this method when meshing on the main thread.
    /// </summary>
    /// <param name="chunk">The chunk to mesh.</param>
    /// <param name="meshingFactory">The meshing factory to use.</param>
    /// <returns>A context that can be used to mesh the chunk.</returns>
    public static ChunkMeshingContext UsingActive(Chunk chunk, IMeshingFactory meshingFactory)
    {
        Throw.IfNotOnMainThread(chunk);

        var foundNeighbors = new (Chunk, Guard?)?[6];
        var availableSides = BlockSides.None;

        foreach (BlockSide side in BlockSide.All.Sides())
        {
            Chunk? neighbor = chunk.World.GetActiveChunk(side.Offset(chunk.Position));

            if (neighbor is not {IsFullyDecorated: true}) continue;

            foundNeighbors[(Int32) side] = (neighbor, null);
            availableSides |= side.ToFlag();
        }

        return new ChunkMeshingContext(chunk, foundNeighbors, availableSides, meshingFactory);
    }

    /// <summary>
    ///     Get the block sides at which chunk neighbours should be used to improve the mesh completeness.
    ///     Only chunks that are available and requested are considered.
    ///     Improvement is also only considered if all required and requested chunks are available at the same time.
    /// </summary>
    /// <param name="chunk">The chunk to get the sides of.</param>
    /// <param name="used">The sides which where used the last time the chunk was meshed.</param>
    /// <returns>
    ///     The sides that should be used. Is empty if no improvements are necessary or possible.
    /// </returns>
    public static BlockSides DetermineImprovementSides(Chunk chunk, BlockSides used)
    {
        var required = BlockSides.None;
        var improving = BlockSides.None;

        foreach (BlockSide side in BlockSide.All.Sides())
        {
            if (!chunk.World.TryGetChunk(side.Offset(chunk.Position), out Chunk? neighbor)) continue;
            if (!chunk.IsRequested || !neighbor.IsFullyDecorated) continue;

            required |= side.ToFlag();

            if (!neighbor.CanAcquireCore(Access.Read)) return BlockSides.None;

            improving |= side.ToFlag();
        }

        return used.HasFlag(required) ? BlockSides.None : improving;
    }

    private Chunk? GetChunk(ChunkPosition position)
    {
        return position == mid.Position
            ? mid
            : BlockSide.All.Sides()
                .Where(side => position == side.Offset(mid.Position))
                .Select(side => neighbors[(Int32) side]?.chunk)
                .FirstOrDefault();
    }

    /// <summary>
    ///     Gets the section with the given position, if it part of the context.
    /// </summary>
    /// <param name="position">The position of the section.</param>
    /// <returns>The section, or null if it is not part of the context.</returns>
    public Section? GetSection(SectionPosition position)
    {
        Chunk? chunk = GetChunk(position.Chunk);

        return chunk?.GetSection(position);
    }

    /// <summary>
    ///     Release all acquired chunks.
    /// </summary>
    public void Release()
    {
        foreach ((Chunk chunk, Guard? guard)? neighbor in neighbors) neighbor?.guard?.Dispose();

        neighbors = null!;
    }
}
