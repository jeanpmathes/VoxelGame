// <copyright file="ChunkContext.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.IO;
using VoxelGame.Core.Generation;

namespace VoxelGame.Core.Logic.Chunks;

/// <summary>
///     The context in which chunks exist.
/// </summary>
public class ChunkContext
{
    /// <summary>
    ///     Manages the state transition for a ready or active chunk.
    /// </summary>
    public delegate ChunkState? ChunkActivator(Chunk chunk);

    /// <summary>
    ///     Deactivates a chunk.
    /// </summary>
    public delegate void ChunkDeactivator(Chunk chunk);

    private readonly ChunkActivator activateStrongly;
    private readonly ChunkActivator activateWeakly;
    private readonly ChunkDeactivator deactivate;

    private readonly World world;

    /// <summary>
    ///     Create a new chunk context.
    /// </summary>
    /// <param name="world">The world in which the chunks exist.</param>
    /// <param name="strongActivator">Activates a chunk after a transition to the ready state.</param>
    /// <param name="weakActivator">Activates a chunk after a transition to the active state.</param>
    /// <param name="deactivator">Deactivates a chunk.</param>
    /// <param name="generator">The world generator used.</param>
    public ChunkContext(World world, IWorldGenerator generator, ChunkActivator strongActivator, ChunkActivator weakActivator, ChunkDeactivator deactivator)
    {
        this.world = world;

        Directory = world.Data.ChunkDirectory;
        Generator = generator;

        activateStrongly = strongActivator;
        activateWeakly = weakActivator;
        deactivate = deactivator;
    }

    /// <summary>
    ///     The directory in which chunks are stored.
    /// </summary>
    public DirectoryInfo Directory { get; }

    /// <summary>
    ///     Get the used world generator.
    /// </summary>
    public IWorldGenerator Generator { get; }

    /// <summary>
    ///     The update list for chunks that will receive state updates.
    /// </summary>
    public ChunkUpdateList UpdateList { get; } = new();

    /// <summary>
    ///     Get a newly initialized chunk.
    ///     The chunks must be returned using <see cref="ReturnObject" />.
    /// </summary>
    public Chunk GetObject(ChunkPosition position)
    {
        Chunk chunk = world.ChunkPool.Get(world, position);

        UpdateList.Add(chunk);

        return chunk;
    }

    /// <summary>
    ///     Return a chunk that was retrieved using <see cref="GetObject" />.
    /// </summary>
    /// <param name="chunk">The chunk to return.</param>
    public void ReturnObject(Chunk chunk)
    {
        UpdateList.Remove(chunk);

        world.ChunkPool.Return(chunk);
    }

    /// <summary>
    ///     Activate a chunk after a transition to the ready state.
    ///     The activator can return null if no transition can be made at this time.
    ///     The chunk was not active before.
    /// </summary>
    public ChunkState? ActivateStrongly(Chunk chunk)
    {
        return activateStrongly(chunk);
    }

    /// <summary>
    ///     Activate a chunk after a transition to the active state.
    ///     The activator can return null if no transition can be made at this time.
    ///     The chunk has been activated before.
    /// </summary>
    public ChunkState? ActivateWeakly(Chunk chunk)
    {
        return activateWeakly(chunk);
    }

    /// <summary>
    ///     Deactivates a chunk.
    /// </summary>
    public void Deactivate(Chunk chunk)
    {
        deactivate(chunk);
    }
}
