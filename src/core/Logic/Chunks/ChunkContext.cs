// <copyright file="ChunkContext.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Generation.Worlds;
using VoxelGame.Toolkit.Memory;

namespace VoxelGame.Core.Logic.Chunks;

/// <summary>
///     The context in which chunks exist.
/// </summary>
public sealed class ChunkContext : IDisposable
{
    /// <summary>
    ///     Manages the state transition for a ready or active chunk.
    /// </summary>
    public delegate ChunkState? ChunkActivator(Chunk chunk);

    /// <summary>
    ///     Deactivates a chunk.
    /// </summary>
    public delegate void ChunkDeactivator(Chunk chunk);

    /// <summary>
    ///     Creates a new chunk.
    /// </summary>
    public delegate Chunk ChunkFactory(NativeSegment<UInt32> blocks, ChunkContext context);

    private readonly ChunkActivator activateStrongly;
    private readonly ChunkActivator activateWeakly;
    private readonly ChunkDeactivator deactivate;

    /// <summary>
    ///     Create a new chunk context.
    /// </summary>
    /// <param name="factory">Creates a new chunk.</param>
    /// <param name="strongActivator">Activates a chunk after a transition to the ready state.</param>
    /// <param name="weakActivator">Activates a chunk after a transition to the active state.</param>
    /// <param name="deactivator">Deactivates a chunk.</param>
    /// <param name="generator">The world generator used.</param>
    public ChunkContext(IWorldGenerator generator, ChunkFactory factory, ChunkActivator strongActivator, ChunkActivator weakActivator, ChunkDeactivator deactivator)
    {
        Generator = generator;

        activateStrongly = strongActivator;
        activateWeakly = weakActivator;
        deactivate = deactivator;

        Pool = new ChunkPool(segment => factory(segment, this));
    }

    /// <summary>
    ///     Get the used world generator.
    /// </summary>
    public IWorldGenerator Generator { get; }

    /// <summary>
    ///     The pool of chunks.
    /// </summary>
    private ChunkPool Pool { get; }

    /// <summary>
    ///     The update list for chunks that will receive state updates.
    /// </summary>
    public ChunkStateUpdateList UpdateList { get; } = new();

    /// <summary>
    ///     Get a newly initialized chunk.
    ///     The chunks must be returned using <see cref="ReturnObject" />.
    /// </summary>
    public Chunk GetObject(World world, ChunkPosition position)
    {
        Chunk chunk = Pool.Get(world, position);

        UpdateList.Add(chunk);

        return chunk;
    }

    /// <summary>
    ///     Return a chunk that was retrieved using <see cref="GetObject" />.
    /// </summary>
    /// <param name="chunk">The chunk to return.</param>
    public void ReturnObject(Chunk chunk)
    {
        chunk.OnRelease();

        UpdateList.Remove(chunk);

        Pool.Return(chunk);
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

    #region DISPOSABLE

    private Boolean disposed;

    /// <summary>
    ///     Dispose of the world.
    /// </summary>
    /// <param name="disposing">True when disposing intentionally.</param>
    private void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing)
        {
            Pool.Dispose();
            Generator.Dispose();
        }

        disposed = true;
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~ChunkContext()
    {
        Dispose(disposing: false);
    }

    /// <summary>
    ///     Dispose of the world.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion DISPOSABLE
}
