// <copyright file="ChunkMeshingContext.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

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

    private ChunkMeshingContext(Chunk mid, (Chunk, Guard?)?[] neighbors, BlockSides availableSides)
    {
        this.mid = mid;
        this.neighbors = neighbors;

        AvailableSides = availableSides;
    }

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
    /// <returns>A context that can be used to mesh the chunk.</returns>
    public static ChunkMeshingContext Acquire(Chunk chunk)
    {
        var foundNeighbors = new (Chunk, Guard?)?[6];
        var availableSides = BlockSides.None;

        foreach (BlockSide side in BlockSide.All.Sides())
        {
            if (!chunk.World.TryGetChunk(side.Offset(chunk.Position), out Chunk? neighbor)) continue;

            Guard? guard = neighbor.AcquireCore(Access.Read);

            if (guard == null) continue;

            foundNeighbors[(int) side] = (neighbor, guard);
            availableSides |= side.ToFlag();
        }

        return new ChunkMeshingContext(chunk, foundNeighbors, availableSides);
    }

    /// <summary>
    ///     Create a meshing context using the given chunk. Use this method when meshing on the main thread.
    /// </summary>
    /// <param name="chunk">The chunk to mesh.</param>
    /// <returns>A context that can be used to mesh the chunk.</returns>
    public static ChunkMeshingContext UsingActive(Chunk chunk)
    {
        var foundNeighbors = new (Chunk, Guard?)?[6];
        var availableSides = BlockSides.None;

        foreach (BlockSide side in BlockSide.All.Sides())
        {
            Chunk? neighbor = chunk.World.GetActiveChunk(side.Offset(chunk.Position));

            if (neighbor == null) continue;

            foundNeighbors[(int) side] = (neighbor, null);
            availableSides |= side.ToFlag();
        }

        return new ChunkMeshingContext(chunk, foundNeighbors, availableSides);
    }

    /// <summary>
    ///     Get the block sides that could be meshed if the context would be acquired now.
    /// </summary>
    /// <param name="chunk">The chunk to get the sides of.</param>
    /// <returns>The sides that could be meshed.</returns>
    public static BlockSides DetermineAvailableSides(Chunk chunk)
    {
        var availableSides = BlockSides.None;

        foreach (BlockSide side in BlockSide.All.Sides())
        {
            if (!chunk.World.TryGetChunk(side.Offset(chunk.Position), out Chunk? neighbor)) continue;

            if (neighbor.CanAcquireCore(Access.Read)) availableSides |= side.ToFlag();
        }

        return availableSides;
    }

    /// <summary>
    ///     Check whether a set of sides is better than older set of sides.
    /// </summary>
    /// <param name="old">The old set of sides.</param>
    /// <param name="available">The new set of sides.</param>
    /// <returns>True if the new set of sides is better.</returns>
    public static bool IsImprovement(BlockSides old, BlockSides available)
    {
        if (old == available) return false;

        int oldCount = BitHelper.CountSetBits((int) old);
        int availableCount = BitHelper.CountSetBits((int) available);

        return availableCount >= oldCount;
    }

    private Chunk? GetChunk(ChunkPosition position)
    {
        return position == mid.Position
            ? mid
            : BlockSide.All.Sides()
                .Where(side => position == side.Offset(mid.Position))
                .Select(side => neighbors[(int) side]?.chunk)
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
