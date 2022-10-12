// <copyright file="ChunkContext.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using System.Diagnostics;
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
    public delegate ChunkState ChunkActivator(Chunk chunk);

    /// <summary>
    ///     Deactivates a chunk.
    /// </summary>
    public delegate void ChunkDeactivator(Chunk chunk);

    /// <summary>
    ///     Creates a chunk for the given position.
    /// </summary>
    public delegate Chunk ChunkFactory(ChunkPosition position, ChunkContext context);

    private readonly ChunkActivator activate;

    private readonly List<(int max, int current)> budgets = new();

    private readonly ChunkFactory create;
    private readonly ChunkDeactivator deactivate;

    /// <summary>
    ///     Create a new chunk context.
    /// </summary>
    public ChunkContext(string directory, ChunkFactory factory, ChunkActivator activator, ChunkDeactivator deactivator, IWorldGenerator generator)
    {
        Directory = directory;
        Generator = generator;

        create = factory;
        activate = activator;
        deactivate = deactivator;
    }

    /// <summary>
    ///     The directory in which chunks are stored.
    /// </summary>
    public string Directory { get; }

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
    ///     Activates a chunk.
    /// </summary>
    public ChunkState Activate(Chunk chunk)
    {
        return activate(chunk);
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
    public bool TryAllocate(Limit limit)
    {
        int index = limit.GetID(this);
        (int max, int current) = budgets[index];

        if (current == 0) return false;

        budgets[index] = (max, current - 1);

        return true;
    }

    /// <summary>
    ///     Free used budget.
    /// </summary>
    public void Free(Limit limit)
    {
        int index = limit.GetID(this);
        (int max, int current) = budgets[index];
        Debug.Assert(current < max);

        budgets[index] = (max, current + 1);
    }
}
