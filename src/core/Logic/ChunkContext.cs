// <copyright file="ChunkContext.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using VoxelGame.Core.Generation;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic;

/// <summary>
///     The context in which chunks exist.
/// </summary>
public class ChunkContext
{
    /// <summary>
    ///     Manages the state transition for a ready chunk.
    /// </summary>
    public delegate ChunkState ChunkActivatorStrong(Chunk chunk);

    /// <summary>
    ///     Manages the state transition for a chunk transitioning to the active state.
    /// </summary>
    public delegate ChunkState? ChunkActivatorWeak(Chunk chunk);

    /// <summary>
    ///     Deactivates a chunk.
    /// </summary>
    public delegate void ChunkDeactivator(Chunk chunk);

    private readonly ChunkActivatorStrong activateStrongly;
    private readonly ChunkActivatorWeak activateWeakly;
    private readonly ChunkDeactivator deactivate;

    private readonly List<(Int32 max, Int32 current)> budgets = [];

    private readonly World world;

    /// <summary>
    ///     Create a new chunk context.
    /// </summary>
    /// <param name="world">The world in which the chunks exist.</param>
    /// <param name="strongActivator">Activates a chunk after a transition to the ready state.</param>
    /// <param name="weakActivator">Activates a chunk after a transition to the active state.</param>
    /// <param name="deactivator">Deactivates a chunk.</param>
    /// <param name="generator">The world generator used.</param>
    public ChunkContext(World world, IWorldGenerator generator, ChunkActivatorStrong strongActivator, ChunkActivatorWeak weakActivator, ChunkDeactivator deactivator)
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
    ///     Get a newly initialized chunk.
    ///     The chunks must be returned using <see cref="ReturnObject" />.
    /// </summary>
    public Chunk GetObject(ChunkPosition position)
    {
        return world.ChunkPool.Get(world, position);
    }

    /// <summary>
    ///     Return a chunk that was retrieved using <see cref="GetObject" />.
    /// </summary>
    /// <param name="chunk">The chunk to return.</param>
    public void ReturnObject(Chunk chunk)
    {
        world.ChunkPool.Return(chunk);
    }

    /// <summary>
    ///     Activate a chunk after a transition to the ready state.
    ///     The chunk was not active before.
    /// </summary>
    public ChunkState ActivateStrongly(Chunk chunk)
    {
        return activateStrongly(chunk);
    }

    /// <summary>
    ///     Activate a chunk after a transition to the active state.
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

    /// <summary>
    ///     Declare a new budget.
    /// </summary>
    /// <param name="maxValue">The maximum value of the budget.</param>
    /// <returns>The <see cref="Limit" /> representing the budget.</returns>
    public Limit DeclareBudget(Int32 maxValue)
    {
        Int32 index = budgets.Count;
        budgets.Add((maxValue, maxValue));

        return new Limit(this, index);
    }

    /// <summary>
    ///     Try to allocate in a budget.
    /// </summary>
    public Guard? TryAllocate(Limit limit)
    {
        Int32 index = limit.GetID(this);
        (Int32 max, Int32 current) = budgets[index];

        if (current == 0) return null;

        budgets[index] = (max, current - 1);

        return new Guard(limit, () => Free(limit));
    }

    /// <summary>
    ///     Free used budget.
    /// </summary>
    private void Free(Limit limit)
    {
        Int32 index = limit.GetID(this);
        (Int32 max, Int32 current) = budgets[index];
        Debug.Assert(current < max);

        budgets[index] = (max, current + 1);
    }
}
