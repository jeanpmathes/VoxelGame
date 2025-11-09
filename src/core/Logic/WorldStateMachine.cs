// <copyright file="WorldStateMachine.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Updates;

namespace VoxelGame.Core.Logic;

/// <summary>
///     The state machine for a world.
/// </summary>
/// <param name="world">The world of which this is the state machine.</param>
/// <param name="timer">
///     An optional timer to measure the time it takes to activate the world. Will be disposed of by this
///     class.
/// </param>
public class WorldStateMachine(World world, Timer? timer) : IWorldStates
{
    private WorldState state = new WorldState.Activating(timer);

    /// <inheritdoc />
    public Activity? BeginTerminating()
    {
        return state.BeginTerminating();
    }

    /// <inheritdoc />
    public Activity? BeginSaving()
    {
        return state.BeginSaving();
    }

    /// <inheritdoc />
    public Boolean IsActive => state.IsActive;

    /// <inheritdoc />
    public Boolean IsTerminating => state.IsTerminating;

    /// <inheritdoc />
    public event EventHandler<EventArgs>? Activating;

    /// <inheritdoc />
    public event EventHandler<EventArgs>? Deactivating;

    /// <inheritdoc />
    public event EventHandler<EventArgs>? Terminating;

    /// <summary>
    ///     Initialize the state machine when the world construction is complete.
    /// </summary>
    public void Initialize()
    {
        state.ApplyChunkUpdateMode(world.ChunkContext.UpdateList);
    }

    /// <summary>
    ///     Update the world state.
    /// </summary>
    public void LogicUpdate(Double deltaTime, Timer? updateTimer)
    {
        WorldState? next = state.LogicUpdate(world, deltaTime, updateTimer);

        if (next == null)
            return;

        if (next == state)
            return;

        if (state.IsActive)
            Deactivating?.Invoke(this, EventArgs.Empty);

        state = next;

        state.ApplyChunkUpdateMode(world.ChunkContext.UpdateList);

        if (next.IsTerminating)
            Terminating?.Invoke(this, EventArgs.Empty);

        if (next.IsActive)
            Activating?.Invoke(this, EventArgs.Empty);
    }
}
