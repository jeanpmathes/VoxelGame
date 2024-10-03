// <copyright file="ChunkMeshingContext.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Generation;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Client.Visuals;

/// <summary>
///     Contains all the data needed to mesh the sections of a chunk.
/// </summary>
public sealed class ChunkMeshingContext : IDisposable, IChunkMeshingContext
{
    private static readonly IReadOnlyCollection<Int32>[] sideIndices;
    private static readonly IReadOnlyCollection<Int32> allIndices;

    private readonly Chunk mid;

    private readonly BlockSide? exclusiveSide;
    private (Guard core, Guard extended)? guards;
    private Sides<(Chunk chunk, Guard? guard)?> neighbors;

    static ChunkMeshingContext()
    {
        const Int32 left = (Int32) BlockSide.Left;
        const Int32 right = (Int32) BlockSide.Right;
        const Int32 bottom = (Int32) BlockSide.Bottom;
        const Int32 top = (Int32) BlockSide.Top;
        const Int32 back = (Int32) BlockSide.Back;
        const Int32 front = (Int32) BlockSide.Front;

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
        Chunk mid, (Guard core, Guard extended)? guards,
        Sides<(Chunk, Guard?)?> neighbors,
        BlockSides availableSides,
        BlockSide? exclusiveSide,
        IMeshingFactory meshingFactory)
    {
        this.mid = mid;
        this.guards = guards;

        this.neighbors = neighbors;
        this.exclusiveSide = exclusiveSide;

        AvailableSides = availableSides;
        MeshingFactory = meshingFactory;
    }

    /// <summary>
    ///     Get the sides at which neighbors are available for meshing in this context.
    /// </summary>
    public BlockSides AvailableSides { get; }

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
    public (TintColor block, TintColor fluid) GetPositionTint(Vector3i position)
    {
        return Map.GetPositionTint(position);
    }

    /// <summary>
    ///     Take the access to the chunk from the context.
    ///     This transfers ownership of the guards to the caller.
    ///     If the chunk was created for meshing on the main thread, this call is not allowed.
    /// </summary>
    /// <returns>The guards for the chunk.</returns>
    public (Guard core, Guard extended) TakeAccess()
    {
        Throw.IfDisposed(disposed);

        if (guards == null)
            throw new InvalidOperationException();

        (Guard core, Guard extended) result = guards.Value;
        guards = null;

        return result;
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
        Debug.Assert(chunk.IsUsableForMeshing());

        allowActivation = false;

        if (!chunk.CanAcquireCore(Access.Read)) return null;
        if (!chunk.CanAcquireExtended(Access.Write)) return null;

        Sides<(Chunk chunk, Guard? guard)?> neighbors = new();

        // Exclusive meshing only meshes a single side of a chunk.
        // Because a side still has width, all neighbors except the opposite side are needed.
        // Exclusive meshing serves to reduce the number of chunks that are deactivated on meshing.
        BlockSide? exclusive = null;

        DetermineNeighborAvailability(chunk, neighbors, out BlockSides considered, out BlockSides acquirable);

        // If all wanted (considered) sides were used the last time, there is no need to mesh.
        if (chunk.HasMeshData && chunk.MeshedSides.HasFlag(considered))
        {
            allowActivation = true;

            return null;
        }

        BlockSides additional = considered & ~chunk.MeshedSides;

        if (additional.Count() == 1)
        {
            BlockSide added = additional.Single();

            BlockSides oppositeOfAdded = added.Opposite().ToFlag();

            if (chunk.MeshedSides.HasFlag(oppositeOfAdded) || !considered.HasFlag(oppositeOfAdded))
            {
                exclusive = added;
                considered &= ~oppositeOfAdded;
            }
        }

        // If not all wanted sides are acquirable, it is preferable to mesh later.
        if (!acquirable.HasFlag(considered)) return null;

        foreach (BlockSide side in BlockSide.All.Sides())
        {
            // If the side is considered, it is also acquirable because we checked that before.
            // If using an exclusive side, that was already removed from the considered sides.

            if (considered.HasFlag(side.ToFlag()))
            {
                Chunk? neighbor = neighbors[side]?.chunk;
                Debug.Assert(neighbor != null);

                Guard? guard = neighbor.AcquireCore(Access.Read);
                Debug.Assert(guard != null);

                Debug.Assert(neighbor.IsUsableForMeshing());

                neighbors[side] = (neighbor, guard);
            }
            else
            {
                neighbors[side] = null;
            }
        }

        (Guard core, Guard extended)? guards = (chunk.AcquireCore(Access.Read)!, chunk.AcquireExtended(Access.Write)!);

        return new ChunkMeshingContext(chunk, guards, neighbors, considered, exclusive, meshingFactory);
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

        Sides<(Chunk, Guard?)?> neighbors = new();
        var availableSides = BlockSides.None;

        foreach (BlockSide side in BlockSide.All.Sides())
        {
            Chunk? neighbor = chunk.World.GetActiveChunk(side.Offset(chunk.Position));

            if (neighbor == null) continue;
            if (!neighbor.IsUsableForMeshing()) continue;

            neighbors[side] = (neighbor, null);
            availableSides |= side.ToFlag();
        }

        return new ChunkMeshingContext(chunk, guards: null, neighbors, availableSides, exclusiveSide: null, meshingFactory);
    }

    private static void DetermineNeighborAvailability(
        Chunk chunk, Sides<(Chunk, Guard?)?>? neighbors,
        out BlockSides considered, out BlockSides acquirable)
    {
        considered = BlockSides.None;
        acquirable = BlockSides.None;

        foreach (BlockSide side in BlockSide.All.Sides())
        {
            if (!chunk.World.TryGetChunk(side.Offset(chunk.Position), out Chunk? neighbor)) continue;

            neighbors?.Set(side, (neighbor, null));

            if (!neighbor.IsWantedForMeshing()) continue;

            considered |= side.ToFlag();

            if (!neighbor.IsUsableForMeshing()) continue;
            if (!neighbor.CanAcquireCore(Access.Read)) continue;

            acquirable |= side.ToFlag();
        }
    }

    /// <summary>
    ///     Get the chunk at a given side of the middle chunk, or null if there is no chunk on that side.
    /// </summary>
    /// <param name="side">The side to get the chunk of.</param>
    /// <returns>The chunk, or null if there is no chunk on that side.</returns>
    public Chunk? GetChunk(BlockSide side)
    {
        return neighbors[side]?.chunk;
    }

    private Chunk? GetChunk(ChunkPosition position)
    {
        if (position == mid.Position) return mid;

        Vector3i offset = mid.Position.OffsetTo(position);

        return GetChunk(offset.ToBlockSide());
    }

    /// <summary>
    ///     Create the mesh data as a result of the meshing process.
    /// </summary>
    /// <param name="sectionMeshData">The mesh data of the sections.</param>
    /// <param name="meshed">The sides that were meshed in the previous meshing process.</param>
    /// <returns>The mesh data for the chunk.</returns>
    public ChunkMeshData CreateMeshData(SectionMeshData?[] sectionMeshData, BlockSides meshed)
    {
        BlockSides sides = AvailableSides;

        if (exclusiveSide is {} side)
        {
            // Exclusive meshing of one side does not touch the sections of the opposite side.
            // If they were meshed before, the remain meshed now.

            BlockSides opposite = side.Opposite().ToFlag();
            if (meshed.HasFlag(opposite)) sides |= opposite;
        }

        return new ChunkMeshData(sectionMeshData, sides, SectionIndices);
    }

    #region IDisposable Support

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

            guards?.core.Dispose();
            guards?.extended.Dispose();
            guards = null!;
        }
        else
        {
            Throw.ForMissedDispose(nameof(ChunkMeshingContext));
        }

        disposed = true;
    }

    #endregion IDisposable Support
}

/// <summary>
///     Extension methods for chunk meshing.
/// </summary>
public static class ChunkMeshingExtensions
{
    /// <summary>
    ///     Check whether the chunk should mesh - depending on the state of its neighbors.
    ///     Only needs to be checked if the chunk wants to mesh the first time
    ///     and is not relevant for meshing caused by outside requests.
    ///     If there are any neighbors that still have to be decorated, meshing should not start.
    ///     This constraint is meant to reduce the amount of meshing work but is not necessary for correctness.
    /// </summary>
    public static Boolean ShouldMeshAccordingToNeighborState(this Chunk chunk)
    {
        // todo: check if this method is still needed as chunks would not mesh anyway
        // todo: then check if strong-mesh-option and weak-mesh-option have same code, if yes, merge them

        foreach (BlockSide side in BlockSide.All.Sides())
        {
            ChunkPosition neighborPosition = side.Offset(chunk.Position);

            if (!chunk.World.TryGetChunk(neighborPosition, out Chunk? neighbor)) continue;

            if (neighbor is {IsRequestedToActivate: true, IsFullyDecorated: false})
                return false;
        }

        return true;
    }

    /// <summary>
    /// Whether the chunk is generally wanted to be included in the meshing process.
    /// A chunk is wanted if it is requested to activate.
    /// </summary>
    public static Boolean IsWantedForMeshing(this Chunk chunk)
    {
        return chunk.IsRequestedToActivate;
    }

    /// <summary>
    ///     Whether the chunk has progressed far enough to be generally usable for meshing.
    ///     A chunk is usable if it is wanted and fully decorated.
    /// </summary>
    public static Boolean IsUsableForMeshing(this Chunk chunk)
    {
        return chunk.IsWantedForMeshing() && chunk.IsFullyDecorated;
    }
}
