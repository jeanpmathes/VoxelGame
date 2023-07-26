// <copyright file="ChunkContext.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

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

    /// <summary>
    ///     Creates a chunk for the given position.
    /// </summary>
    public delegate Chunk ChunkFactory(ChunkPosition position, ChunkContext context);

    private readonly ChunkActivatorStrong activateStrongly;
    private readonly ChunkActivatorWeak activateWeakly;

    private readonly List<(int max, int current)> budgets = new();

    private readonly ChunkFactory create;
    private readonly ChunkDeactivator deactivate;

    /// <summary>
    ///     Create a new chunk context.
    /// </summary>
    /// <param name="directory">The directory in which the chunks are stored.</param>
    /// <param name="factory">A factory for creating chunks.</param>
    /// <param name="strongActivator">Activates a chunk after a transition to the ready state.</param>
    /// <param name="weakActivator">Activates a chunk after a transition to the active state.</param>
    /// <param name="deactivator">Deactivates a chunk.</param>
    /// <param name="generator">The world generator used.</param>
    public ChunkContext(DirectoryInfo directory, ChunkFactory factory, ChunkActivatorStrong strongActivator, ChunkActivatorWeak weakActivator, ChunkDeactivator deactivator, IWorldGenerator generator)
    {
        Directory = directory;
        Generator = generator;

        create = factory;
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
    ///     Create a new chunk.
    /// </summary>
    public Chunk Create(ChunkPosition position, ChunkContext context)
    {
        return create(position, context);
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
    public Limit DeclareBudget(int maxValue)
    {
        int index = budgets.Count;
        budgets.Add((maxValue, maxValue));

        return new Limit(this, index);
    }

    /// <summary>
    ///     Try to allocate in a budget.
    /// </summary>
    public Guard? TryAllocate(Limit limit)
    {
        int index = limit.GetID(this);
        (int max, int current) = budgets[index];

        if (current == 0) return null;

        budgets[index] = (max, current - 1);

        return new Guard(limit, () => Free(limit));
    }

    /// <summary>
    ///     Free used budget.
    /// </summary>
    private void Free(Limit limit)
    {
        int index = limit.GetID(this);
        (int max, int current) = budgets[index];
        Debug.Assert(current < max);

        budgets[index] = (max, current + 1);
    }
}
