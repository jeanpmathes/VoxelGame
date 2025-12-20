// <copyright file="WorldState.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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
using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.App;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Updates;
using VoxelGame.Logging;
using Activity = VoxelGame.Core.Updates.Activity;

namespace VoxelGame.Core.Logic;

/// <summary>
///     Describes a state the world can be in.
/// </summary>
public abstract partial class WorldState
{
    /// <summary>
    ///     Whether the world is active.
    /// </summary>
    public virtual Boolean IsActive => false;

    /// <summary>
    ///     Whether the world is terminating.
    /// </summary>
    public virtual Boolean IsTerminating => false;

    /// <summary>
    ///     Update the curren state and return the next state if it has changed.
    /// </summary>
    /// <param name="world">The world to update.</param>
    /// <param name="deltaTime">The time since the last update.</param>
    /// <param name="updateTimer">An optional timer to measure the time it takes to update the world.</param>
    /// <returns>The next state if it has changed or <c>null</c> if it has not.</returns>
    public abstract WorldState? LogicUpdate(World world, Double deltaTime, Timer? updateTimer);

    /// <summary>
    ///     Apply the chunk update mode to the given list.
    /// </summary>
    /// <param name="list">The list to apply the update mode to.</param>
    public abstract void ApplyChunkUpdateMode(ChunkStateUpdateList list);

    /// <inheritdoc cref="IWorldStates.BeginTerminating" />
    public virtual Activity? BeginTerminating()
    {
        return null;
    }

    /// <inheritdoc cref="IWorldStates.BeginSaving" />
    public virtual Activity? BeginSaving()
    {
        return null;
    }

    /// <summary>
    ///     The state in which the world is working to become active.
    /// </summary>
    /// <param name="timer">
    ///     An optional timer to measure the time it takes to activate the world. Will be disposed of by this
    ///     class.
    /// </param>
    public class Activating(Timer? timer) : WorldState
    {
        private Int64 chunkUpdateCount;
        private Int64 worldUpdateCount;

        /// <inheritdoc />
        public override WorldState? LogicUpdate(World world, Double deltaTime, Timer? updateTimer)
        {
            worldUpdateCount += 1;
            chunkUpdateCount += world.ChunkStateUpdateCount;

            if (!world.Chunks.IsEveryChunkToSimulateActive())
                return null;

            var readyTime = (timer?.Elapsed ?? default).ToString(CultureInfo.InvariantCulture);
            LogWorldReady(logger, readyTime, worldUpdateCount, chunkUpdateCount);

            timer?.Dispose();

            return new Active();
        }

        /// <inheritdoc />
        public override void ApplyChunkUpdateMode(ChunkStateUpdateList list)
        {
            list.EnterHighThroughputMode();
        }
    }

    /// <summary>
    ///     The state in which the world is active.
    /// </summary>
    public class Active : WorldState
    {
        private WorldState? next;

        /// <inheritdoc />
        public override Boolean IsActive => true;

        /// <inheritdoc />
        public override WorldState? LogicUpdate(World world, Double deltaTime, Timer? updateTimer)
        {
            world.OnLogicUpdateInActiveState(deltaTime, updateTimer);

            return next;
        }

        /// <inheritdoc />
        public override void ApplyChunkUpdateMode(ChunkStateUpdateList list)
        {
            list.EnterLowImpactMode();
        }

        /// <inheritdoc />
        public override Activity? BeginTerminating()
        {
            if (next != null)
                return null;

            var activity = Activity.Create(out Action onComplete);

            next = new Terminating(onComplete);

            return activity;
        }

        /// <inheritdoc />
        public override Activity? BeginSaving()
        {
            if (next != null)
                return null;

            var activity = Activity.Create(out Action onComplete);

            next = new Saving(onComplete);

            return activity;
        }
    }

    /// <summary>
    ///     The state in which the world is terminating.
    /// </summary>
    /// <param name="onComplete">Called when the world has successfully terminated.</param>
    public class Terminating(Action onComplete) : WorldState
    {
        private Boolean completed;
        private Operation? saving;

        /// <inheritdoc />
        public override Boolean IsTerminating => true;

        /// <inheritdoc />
        public override WorldState? LogicUpdate(World world, Double deltaTime, Timer? updateTimer)
        {
            Debug.Assert(!completed);

            if (saving == null)
            {
                world.Data.Information.Version = Application.Instance.Version.ToString();

                saving = Operations.Launch(async token =>
                {
                    await world.Data.SaveAsync(token).InAnyContext();
                });
            }

            if (saving.IsRunning || !world.Chunks.IsEmpty)
                return null;

            saving.Result?.Switch(
                () => {},
                exception => LogFailedToSaveWorldMetaInformation(logger, exception));

            LogUnloadedWorld(logger);

            onComplete();
            completed = true;

            return null;
        }

        /// <inheritdoc />
        public override void ApplyChunkUpdateMode(ChunkStateUpdateList list)
        {
            list.EnterHighThroughputMode();
        }
    }

    /// <summary>
    ///     The state in which the world is saving.
    /// </summary>
    /// <param name="onComplete">Called when the world has successfully saving.</param>
    public class Saving(Action onComplete) : WorldState
    {
        private Int32 progress;
        private Operation? saving;
        private Int32 total;

        /// <inheritdoc />
        public override WorldState? LogicUpdate(World world, Double deltaTime, Timer? updateTimer)
        {
            if (saving == null)
            {
                LogSavingWorld(logger);

                world.Data.Information.Version = Application.Instance.Version.ToString();

                saving = Operations.Launch(async token =>
                {
                    await world.Data.SaveAsync(token).InAnyContext();
                });

                foreach (Chunk chunk in world.Chunks.All)
                {
                    chunk.BeginSaving();

                    chunk.StateTransition += OnStateTransition;

                    total += 1;
                }
            }

            if (saving.IsRunning || progress < total)
                return null;

            saving.Result?.Switch(
                () => {},
                exception => LogFailedToSaveWorldMetaInformation(logger, exception));

            LogSavedWorld(logger);

            onComplete();

            return new Active();
        }

        private void OnStateTransition(Object? sender, StateTransitionEventArgs e)
        {
            var chunk = (Chunk) sender!;

            if (e.OldState is not Chunk.Saving)
                return;

            progress += 1;

            chunk.StateTransition -= OnStateTransition;
        }

        /// <inheritdoc />
        public override void ApplyChunkUpdateMode(ChunkStateUpdateList list)
        {
            list.EnterHighThroughputMode();
        }
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<WorldState>();

    [LoggerMessage(EventId = LogID.WorldState + 0, Level = LogLevel.Information, Message = "World ready after {ReadyTime}, using {WorldUpdates} world updates with {ChunkUpdates} chunk updates")]
    private static partial void LogWorldReady(ILogger logger, String readyTime, Int64 worldUpdates, Int64 chunkUpdates);

    [LoggerMessage(EventId = LogID.WorldState + 1, Level = LogLevel.Information, Message = "Unloaded world")]
    private static partial void LogUnloadedWorld(ILogger logger);

    [LoggerMessage(EventId = LogID.WorldState + 2, Level = LogLevel.Information, Message = "Saving world")]
    private static partial void LogSavingWorld(ILogger logger);

    [LoggerMessage(EventId = LogID.WorldState + 3, Level = LogLevel.Information, Message = "Saved world")]
    private static partial void LogSavedWorld(ILogger logger);

    [LoggerMessage(EventId = LogID.WorldState + 4, Level = LogLevel.Error, Message = "Failed to save world meta information")]
    private static partial void LogFailedToSaveWorldMetaInformation(ILogger logger, Exception exception);

    #endregion LOGGING
}
