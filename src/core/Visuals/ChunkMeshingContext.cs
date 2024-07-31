// <copyright file="ChunkMeshingContext.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VoxelGame.Core.Generation;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Utilities;
using Chunk = VoxelGame.Core.Logic.Chunks.Chunk;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Contains all the data needed to mesh the sections of a chunk.
/// </summary>
public class ChunkMeshingContext
{
    private static readonly IReadOnlyCollection<Int32>[] sideIndices;
    private static readonly IReadOnlyCollection<Int32> allIndices;

    private readonly Chunk mid;

    private readonly BlockSide? exclusiveSide;
    private (Chunk chunk, Guard? guard)?[] neighbors;

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
        Chunk mid, (Chunk, Guard?)?[] neighbors,
        BlockSides availableSides,
        BlockSide? exclusiveSide,
        IMeshingFactory meshingFactory)
    {
        this.mid = mid;
        this.neighbors = neighbors;
        this.exclusiveSide = exclusiveSide;

        AvailableSides = availableSides;
        MeshingFactory = meshingFactory;
    }

    /// <summary>
    ///     Get the meshing factory which can be used to create meshing instances.
    /// </summary>
    public IMeshingFactory MeshingFactory { get; }

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
    ///     Acquire the chunks around the given chunk.
    ///     Use this method when meshing on a separate thread, but acquire on the main thread.
    /// </summary>
    /// <param name="chunk">The chunk to acquire the neighbors of. Must itself have sufficient access for meshing.</param>
    /// <param name="meshed">Sides that were used for the last mesh creation.</param>
    /// <param name="source">Side from which the meshing request came, or <see cref="BlockSide.All"/> if not applicable.</param>
    /// <param name="meshingFactory">The meshing factory to use.</param>
    /// <returns>A context that can be used to mesh the chunk.</returns>
    public static ChunkMeshingContext Acquire(Chunk chunk, BlockSides meshed, BlockSide source, IMeshingFactory meshingFactory)
    {
        var foundNeighbors = new (Chunk, Guard?)?[6];
        var availableSides = BlockSides.None;

        var acquirable = BlockSides.All;
        BlockSide? exclusive = null;

        if (source != BlockSide.All)
        {
            // If a new chunk is loaded, the neighbors are asked to mesh too.
            // When they do that, it causes them to re-mesh all their sides, even though they might have already meshed them.
            // Additionally, the neighbors would then also use their neighbors to mesh.
            // All of this causes chunks not directly connected to the new chunk to disappear because they are used.
            // As this is unwanted and unnecessary, this special case here prevents it.

            DetermineSideAvailability(chunk, out BlockSides considered, out acquirable, abortOnNotAcquirable: false);

            Boolean isImprovement = IsImprovement(meshed | source.ToFlag(), acquirable) && considered == acquirable;

            if (!isImprovement)
                exclusive = source;
        }

        foreach (BlockSide side in BlockSide.All.Sides())
        {
            // If we have previously determined that this side is not acquirable, we don't need to check again.
            if (!acquirable.HasFlag(side.ToFlag())) continue;

            // If we mesh a single side of a chunk, we do not need the neighbor on the opposite side.
            if (exclusive == side.Opposite()) continue;

            if (!chunk.World.TryGetChunk(side.Offset(chunk.Position), out Chunk? neighbor)) continue;
            if (!neighbor.IsViableForMeshing()) continue;

            Guard? guard = neighbor.AcquireCore(Access.Read);

            if (guard == null) continue;

            foundNeighbors[(Int32) side] = (neighbor, guard);
            availableSides |= side.ToFlag();
        }

        return new ChunkMeshingContext(chunk, foundNeighbors, availableSides, exclusive, meshingFactory);
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

            if (neighbor == null) continue;
            if (!neighbor.IsViableForMeshing()) continue;

            foundNeighbors[(Int32) side] = (neighbor, null);
            availableSides |= side.ToFlag();
        }

        return new ChunkMeshingContext(chunk, foundNeighbors, availableSides, exclusiveSide: null, meshingFactory);
    }

    /// <summary>
    ///     Get the number of neighbors that in the near future might be required for meshing but are not acquirable at the
    ///     moment.
    /// </summary>
    /// <param name="chunk">The chunk to calculate the number for.</param>
    /// <param name="exclusive">The side that is meshed exclusively, or <see cref="BlockSide.All"/> if not applicable.</param>
    /// <returns>The number.</returns>
    public static Int32 GetNumberOfNonAcquirablePossibleFutureMeshingPartners(Chunk chunk, BlockSide exclusive)
    {
        var count = 0;

        foreach (BlockSide side in BlockSide.All.Sides())
        {
            if (side.Opposite() == exclusive) continue;

            if (!chunk.World.TryGetChunk(side.Offset(chunk.Position), out Chunk? neighbor)) continue;

            // A requested chunk might become viable in the near future.
            if (!neighbor.IsRequestedToActivate) continue;

            if (!neighbor.CanAcquireCore(Access.Read)) count++;
        }

        if (count > 0)
            Debugger.Break();

        return count;
    }

    /// <summary>
    ///     Get the block sides at which chunk neighbours should be used to improve the mesh completeness.
    ///     Only chunks that are available and requested are considered.
    ///     Improvement is also only considered if all required and requested chunks are possible to acquire at the same time.
    /// </summary>
    /// <param name="chunk">The chunk to get the improvement sides of.</param>
    /// <param name="used">The sides which where used the last time the chunk was meshed.</param>
    /// <returns>
    ///     The sides that should be used. Is empty if no improvements are necessary or possible.
    /// </returns>
    public static BlockSides DetermineImprovementSides(Chunk chunk, BlockSides used)
    {
        // Without this check, chunks would be meshed a lot in a row.
        const Boolean abortOnNotAcquirable = true;

        DetermineSideAvailability(chunk, out BlockSides considered, out BlockSides acquirable, abortOnNotAcquirable);

        if (acquirable == BlockSides.None) return BlockSides.None;
        if (acquirable != considered) return BlockSides.None;

        // If all sides of the potentially acquirable set were already used, it is not an improvement.
        return IsImprovement(used, acquirable) ? acquirable : BlockSides.None;
    }

    private static Boolean IsImprovement(BlockSides used, BlockSides acquirable)
    {
        return !used.HasFlag(acquirable);
    }

    private static void DetermineSideAvailability(
        Chunk chunk,
        out BlockSides considered, out BlockSides acquirable,
        Boolean abortOnNotAcquirable)
    {
        considered = BlockSides.None;
        acquirable = BlockSides.None;

        foreach (BlockSide side in BlockSide.All.Sides())
        {
            if (!chunk.World.TryGetChunk(side.Offset(chunk.Position), out Chunk? neighbor)) continue;
            if (!neighbor.IsViableForMeshing()) continue;

            considered |= side.ToFlag();

            if (neighbor.CanAcquireCore(Access.Read)) acquirable |= side.ToFlag();
            else if (abortOnNotAcquirable) return;
        }
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
            // Because only the exclusive side is meshed fully, the other sides are considered meshed if two conditions are met:
            //   - They were already meshed, this is required for the part that was not touched now.
            //   - They were available now, this is required for the part that was touched now.
            // Additionally, we assume that the exclusive side is meshed as a neighbor requested it to be meshed so that neighbor should have been available.

            sides = (AvailableSides & meshed) | side.ToFlag();

            // Exclusive meshing of one side does not touch the opposite side at all.
            // If it was meshed before, it remains meshed now.

            BlockSides opposite = side.Opposite().ToFlag();
            if (meshed.HasFlag(opposite)) sides |= opposite;
        }

        return new ChunkMeshData(sectionMeshData, sides, SectionIndices);
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
    ///     Whether the chunk is viable for meshing.
    ///     This is relevant both for deciding whether the chunks should be meshed themselves
    ///     and whether they should be used when meshing their neighbors.
    /// </summary>
    /// <param name="chunk">The chunk to check.</param>
    /// <returns>Whether the chunk is viable for meshing.</returns>
    public static Boolean IsViableForMeshing(this Chunk chunk)
    {
        return chunk is {IsFullyDecorated: true, IsRequestedToActivate: true};
    }
}
