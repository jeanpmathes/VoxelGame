﻿// <copyright file="ChunkMeshingContext.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK.Mathematics;
using VoxelGame.Core;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Generation.Worlds;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Visuals;

/// <summary>
///     Contains all the data needed to mesh the sections of a chunk.
/// </summary>
public sealed class ChunkMeshingContext : IDisposable, IChunkMeshingContext
{
    private static readonly IReadOnlyCollection<Int32>[] sideIndices;
    private static readonly IReadOnlyCollection<Int32> allIndices;

    private readonly Chunk mid;

    private readonly Side? exclusiveSide;

    private Guard? guard;
    private SideArray<(Chunk chunk, Guard? guard)?> neighbors;

    static ChunkMeshingContext()
    {
        const Int32 left = (Int32) Side.Left;
        const Int32 right = (Int32) Side.Right;
        const Int32 bottom = (Int32) Side.Bottom;
        const Int32 top = (Int32) Side.Top;
        const Int32 back = (Int32) Side.Back;
        const Int32 front = (Int32) Side.Front;

        List<Int32>[] sides = Enumerable.Range(start: 0, count: 6).Select(_ => new List<Int32>()).ToArray();
        List<Int32> all = [];

        for (var index = 0; index < Chunk.SectionCount; index++)
        {
            all.Add(index);

            (Int32 x, Int32 y, Int32 z) = Chunk.IndexToLocalSection(index);

            if (x == 0) sides[left].Add(index);
            if (x == Chunk.Size - 1) sides[right].Add(index);

            if (y == 0) sides[bottom].Add(index);
            if (y == Chunk.Size - 1) sides[top].Add(index);

            if (z == 0) sides[back].Add(index);
            if (z == Chunk.Size - 1) sides[front].Add(index);
        }

        sideIndices = sides.Cast<IReadOnlyCollection<Int32>>().ToArray();
        allIndices = all;
    }

    private ChunkMeshingContext(
        Chunk mid, Guard? guard,
        SideArray<(Chunk, Guard?)?> neighbors,
        Sides availableSides,
        Side? exclusiveSide,
        IMeshingFactory meshingFactory)
    {
        this.mid = mid;
        this.guard = guard;

        this.neighbors = neighbors;
        this.exclusiveSide = exclusiveSide;

        AvailableSides = availableSides;
        MeshingFactory = meshingFactory;
    }

    /// <summary>
    ///     Get the sides at which neighbors are available for meshing in this context.
    /// </summary>
    public Sides AvailableSides { get; }

    /// <summary>
    ///     Get the map of the world.
    /// </summary>
    public IMap Map => mid.World.Map;

    /// <summary>
    ///     Get the indices of all sections that should be meshed.
    /// </summary>
    public IReadOnlyCollection<Int32> SectionIndices => exclusiveSide switch
    {
        null => allIndices,
        {} side => sideIndices[(Int32) side]
    };

    /// <summary>
    ///     Get the meshing factory which can be used to create meshing instances.
    /// </summary>
    public IMeshingFactory MeshingFactory { get; }

    /// <inheritdoc />
    public Section? GetSection(SectionPosition position)
    {
        Chunk? chunk = GetChunk(position.Chunk);

        return chunk?.GetSection(position);
    }

    /// <inheritdoc />
    public (ColorS block, ColorS fluid) GetPositionTint(Vector3i position)
    {
        return Map.GetPositionTint(position);
    }

    /// <summary>
    ///     Take the access to the chunk from the context.
    ///     This transfers ownership of the guards to the caller.
    ///     If the context was created for meshing on the main thread, this call is not allowed.
    /// </summary>
    /// <returns>The guards for the chunk.</returns>
    public Guard TakeAccess()
    {
        Throw.IfDisposed(disposed);

        if (guard == null)
            throw Exceptions.InvalidOperation("Cannot take access from main thread meshing context.");

        Guard result = guard;
        guard = null;

        return result;
    }

    /// <summary>
    ///     Whether re-meshing would potentially be valuable, considering the neighbors.
    ///     If not, the chunk should not be re-meshed.
    /// </summary>
    /// <returns>Whether re-meshing would be valuable.</returns>
    public static Boolean IsReMeshingValuable(Logic.Chunks.Chunk chunk)
    {
        SideArray<(Chunk chunk, Guard? guard)?> neighbors = new();

        DetermineNeighborAvailability(chunk, neighbors, out Sides considered, out Sides acquirable);

        return !CanActivate(chunk, considered) && CanMeshNow(chunk, acquirable, ref considered, out _);
    }

    /// <summary>
    ///     Try to acquire the chunks around the given chunk for meshing.
    ///     Use this method when meshing on a separate thread, but acquire on the main thread.
    /// </summary>
    /// <param name="chunk">The chunk to acquire the neighbors of. Must itself have sufficient access for meshing.</param>
    /// <param name="meshingFactory">The meshing factory to use.</param>
    /// <param name="allowActivation">Whether the chunk can be activated in the case that this method returns <c>null</c>.</param>
    /// <returns>A context that can be used to mesh the chunk, or null if meshing is either not possible or worthwhile.</returns>
    public static ChunkMeshingContext? TryAcquire(Logic.Chunks.Chunk chunk, IMeshingFactory meshingFactory, out Boolean allowActivation)
    {
        Debug.Assert(chunk.IsAbleToMesh());

        allowActivation = false;

        if (!chunk.CanAcquire(Access.Read)) return null;

        SideArray<(Chunk chunk, Guard? guard)?> neighbors = new();

        DetermineNeighborAvailability(chunk, neighbors, out Sides considered, out Sides acquirable);

        if (CanActivate(chunk, considered))
        {
            allowActivation = true;

            return null;
        }

        // Exclusive meshing only meshes a single side of a chunk.
        // Because a side still has width, all neighbors except the opposite side are needed.
        // Exclusive meshing serves to reduce the number of chunks that are deactivated on meshing.

        if (!CanMeshNow(chunk, acquirable, ref considered, out Side? exclusive))
            return null;

        foreach (Side side in Side.All.Sides())
            // If the side is considered, it is also acquirable because we checked that before.
            // If using an exclusive side, that was already removed from the considered sides.
            if (considered.HasFlag(side.ToFlag()))
            {
                Chunk? neighbor = neighbors[side]?.chunk;
                Debug.Assert(neighbor != null);

                Guard? guard = neighbor.Acquire(Access.Read);
                Debug.Assert(guard != null);

                Debug.Assert(neighbor.IsAbleToParticipateInMeshing());

                neighbors[side] = (neighbor, guard);
            }
            else
            {
                neighbors[side] = null;
            }

        return new ChunkMeshingContext(chunk, chunk.Acquire(Access.Read)!, neighbors, considered, exclusive, meshingFactory);
    }

    /// <summary>
    ///     If all wanted (considered) sides were used the last time, there is no need to mesh.
    /// </summary>
    private static Boolean CanActivate(Logic.Chunks.Chunk chunk, Sides considered)
    {
        return chunk.HasMeshData && chunk.MeshedSides.HasFlag(considered);
    }

    /// <summary>
    ///     If not all wanted (considered) sides are acquirable, it is preferable to mesh later.
    /// </summary>
    private static Boolean CanMeshNow(Logic.Chunks.Chunk chunk, Sides acquirable, ref Sides considered, out Side? exclusive)
    {
        exclusive = null;

        Sides additional = considered & ~chunk.MeshedSides;

        if (additional.Count() != 1)
            return acquirable.HasFlag(considered);

        Side added = additional.Single();
        Sides oppositeOfAdded = added.Opposite().ToFlag();

        // If no sides have been meshed yet, no exclusive meshing can be done as this would miss the center.
        // Otherwise, do exclusive meshing if the exclusive side is either already meshed or not considered at all.

        if (chunk.MeshedSides != Sides.None
            && (chunk.MeshedSides.HasFlag(oppositeOfAdded) || !considered.HasFlag(oppositeOfAdded)))
        {
            exclusive = added;
            considered &= ~oppositeOfAdded;
        }

        return acquirable.HasFlag(considered);
    }

    /// <summary>
    ///     Create a meshing context using the given chunk. Use this method when meshing on the main thread.
    /// </summary>
    /// <param name="chunk">The chunk to mesh.</param>
    /// <param name="meshingFactory">The meshing factory to use.</param>
    /// <returns>A context that can be used to mesh the chunk.</returns>
    public static ChunkMeshingContext UsingActive(Chunk chunk, IMeshingFactory meshingFactory)
    {
        ApplicationInformation.ThrowIfNotOnMainThread(chunk);

        SideArray<(Chunk, Guard?)?> neighbors = new();
        var availableSides = Sides.None;

        foreach (Side side in Side.All.Sides())
        {
            Chunk? neighbor = chunk.World.GetActiveChunk(side.Offset(chunk.Position));

            if (neighbor == null) continue;
            if (!neighbor.IsAbleToParticipateInMeshing()) continue;

            neighbors[side] = (neighbor, null);
            availableSides |= side.ToFlag();
        }

        return new ChunkMeshingContext(chunk, guard: null, neighbors, availableSides, exclusiveSide: null, meshingFactory);
    }

    private static void DetermineNeighborAvailability(
        Chunk chunk, SideArray<(Chunk, Guard?)?>? neighbors,
        out Sides considered, out Sides acquirable)
    {
        considered = Sides.None;
        acquirable = Sides.None;

        foreach (Side side in Side.All.Sides())
        {
            if (!chunk.World.TryGetChunk(side.Offset(chunk.Position), out Chunk? neighbor))
                continue;

            neighbors?.Set(side, (neighbor, null));

            if (!neighbor.IsAbleToParticipateInMeshing()) continue;

            considered |= side.ToFlag();

            if (!neighbor.CanAcquire(Access.Read)) continue;

            acquirable |= side.ToFlag();
        }
    }

    /// <summary>
    ///     Get the chunk at a given side of the middle chunk, or null if there is no chunk on that side.
    /// </summary>
    /// <param name="side">The side to get the chunk of.</param>
    /// <returns>The chunk, or null if there is no chunk on that side.</returns>
    public Chunk? GetChunk(Side side)
    {
        return neighbors[side]?.chunk;
    }

    private Chunk? GetChunk(ChunkPosition position)
    {
        if (position == mid.Position) return mid;

        Vector3i offset = mid.Position.OffsetTo(position);

        return GetChunk(offset.ToSide());
    }

    /// <summary>
    ///     Create the mesh data as a result of the meshing process.
    /// </summary>
    /// <param name="sectionMeshData">The mesh data of the sections.</param>
    /// <param name="meshed">The sides that were meshed in the previous meshing process.</param>
    /// <returns>The mesh data for the chunk.</returns>
    public ChunkMeshData CreateMeshData(SectionMeshData?[] sectionMeshData, Sides meshed)
    {
        Sides sides = AvailableSides;

        if (exclusiveSide is {} side)
        {
            // Exclusive meshing of one side does not touch the sections of the opposite side.
            // If they were meshed before, the remain meshed now.

            Sides opposite = side.Opposite().ToFlag();
            if (meshed.HasFlag(opposite)) sides |= opposite;
        }

        return new ChunkMeshData(sectionMeshData, sides, SectionIndices);
    }

    /// <inheritdoc />
    public override String ToString()
    {
        var text = $"[{AvailableSides.ToCompactString()}]";

        if (exclusiveSide is {} side)
            text += $"+({side.ToCompactString()})";

        return text;
    }

    #region DISPOSABLE

    private Boolean disposed;

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~ChunkMeshingContext()
    {
        Dispose(disposing: false);
    }

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing)
        {
            foreach ((Chunk chunk, Guard? guard)? neighbor in neighbors)
                neighbor?.guard?.Dispose();

            neighbors = null!;

            guard?.Dispose();
            guard = null!;
        }
        else
        {
            Throw.ForMissedDispose(nameof(ChunkMeshingContext));
        }

        disposed = true;
    }

    #endregion DISPOSABLE
}

/// <summary>
///     Extension methods for chunk meshing.
/// </summary>
public static class ChunkMeshingExtensions
{
    /// <summary>
    ///     Whether the chunk has progressed far enough to be generally able to participate in meshing as a neighbor.
    ///     This means it should be considered by other meshing chunks when they look at their neighbors.
    /// </summary>
    public static Boolean IsAbleToParticipateInMeshing(this Chunk chunk)
    {
        return chunk.IsFullyDecorated;
    }

    /// <summary>
    ///     Whether the chunk is generally able to mesh itself.
    ///     A chunk is able if it is requested to activate and would be able to participate as a neighbor.
    ///     This is a stronger condition than <see cref="IsAbleToParticipateInMeshing" />.
    /// </summary>
    public static Boolean IsAbleToMesh(this Chunk chunk)
    {
        return chunk.IsRequestedToActivate && chunk.IsAbleToParticipateInMeshing();
    }

    /// <inheritdoc cref="ChunkMeshingContext.IsReMeshingValuable" />
    public static Boolean IsReMeshingValuable(this Logic.Chunks.Chunk chunk)
    {
        return ChunkMeshingContext.IsReMeshingValuable(chunk);
    }
}
