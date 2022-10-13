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

    private ChunkMeshingContext(Chunk mid, (Chunk, Guard?)?[] neighbors)
    {
        this.mid = mid;
        this.neighbors = neighbors;
    }

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

        foreach (BlockSide side in BlockSide.All.Sides())
        {
            if (!chunk.World.TryGetChunk(side.Offset(chunk.Position), out Chunk? neighbor)) continue;

            Guard? guard = neighbor.CoreResource.TryAcquireReader();
            foundNeighbors[(int) side] = guard != null ? (neighbor, guard) : null;
        }

        return new ChunkMeshingContext(chunk, foundNeighbors);
    }

    /// <summary>
    ///     Create a meshing context from the given chunk. Use this method when meshing on the main thread.
    /// </summary>
    /// <param name="chunk">The chunk to mesh.</param>
    /// <returns>A context that can be used to mesh the chunk.</returns>
    public static ChunkMeshingContext FromActive(Chunk chunk)
    {
        var foundNeighbors = new (Chunk, Guard?)?[6];

        foreach (BlockSide side in BlockSide.All.Sides())
        {
            Chunk? neighbor = chunk.World.GetActiveChunk(side.Offset(chunk.Position));
            foundNeighbors[(int) side] = neighbor != null ? (neighbor, null) : null;
        }

        return new ChunkMeshingContext(chunk, foundNeighbors);
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
        Chunk? chunk = GetChunk(position.GetChunk());

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
