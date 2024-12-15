// <copyright file="WorldState.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities.Units;
using VoxelGame.Logging;

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
    public abstract WorldState? Update(World world, Double deltaTime, Timer? updateTimer);

    /// <summary>
    ///     Apply the chunk update mode to the given list.
    /// </summary>
    /// <param name="list">The list to apply the update mode to.</param>
    public abstract void ApplyChunkUpdateMode(ChunkStateUpdateList list);

    /// <inheritdoc cref="IWorldStates.BeginTerminating" />
    public virtual Boolean BeginTerminating(Action onComplete)
    {
        return false;
    }

    /// <inheritdoc cref="IWorldStates.BeginSaving" />
    public virtual Boolean BeginSaving(Action onComplete)
    {
        return false;
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
        private Int64 worldUpdateCount;
        private Int64 chunkUpdateCount;

        /// <inheritdoc />
        public override WorldState? Update(World world, Double deltaTime, Timer? updateTimer)
        {
            worldUpdateCount += 1;
            chunkUpdateCount += world.ChunkStateUpdateCount;

            if (!world.Chunks.IsEveryChunkToSimulateActive())
                return null;

            Duration readyTime = timer?.Elapsed ?? default;
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
        public override WorldState? Update(World world, Double deltaTime, Timer? updateTimer)
        {
            world.ActiveUpdate(deltaTime, updateTimer);

            return next;
        }

        /// <inheritdoc />
        public override void ApplyChunkUpdateMode(ChunkStateUpdateList list)
        {
            list.EnterLowImpactMode();
        }

        /// <inheritdoc />
        public override Boolean BeginTerminating(Action onComplete)
        {
            if (next != null)
                return false;

            next = new Terminating(onComplete);

            return true;
        }

        /// <inheritdoc />
        public override Boolean BeginSaving(Action onComplete)
        {
            if (next != null)
                return false;

            next = new Saving(onComplete);

            return true;
        }
    }

    /// <summary>
    ///     The state in which the world is terminating.
    /// </summary>
    /// <param name="onComplete">Called when the world has successfully terminated.</param>
    public class Terminating(Action onComplete) : WorldState
    {
        private Future? saving;
        private Boolean completed;

        /// <inheritdoc />
        public override Boolean IsTerminating => true;

        /// <inheritdoc />
        public override WorldState? Update(World world, Double deltaTime, Timer? updateTimer)
        {
            if (completed)
                throw new InvalidOperationException("The world has already been terminated.");

            if (saving == null)
            {
                world.Data.Information.Version = ApplicationInformation.Instance.Version;
                saving = Future.Create(world.Data.Save);
            }

            if (!saving.IsCompleted || !world.Chunks.IsEmpty)
                return null;

            if (saving.Exception is {} exception)
                LogFailedToSaveWorldMetaInformation(logger, exception);

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
        private Future? saving;

        private Int32 progress;
        private Int32 total;

        /// <inheritdoc />
        public override WorldState? Update(World world, Double deltaTime, Timer? updateTimer)
        {
            if (saving == null)
            {
                LogSavingWorld(logger);

                world.Data.Information.Version = ApplicationInformation.Instance.Version;
                saving = Future.Create(world.Data.Save);

                foreach (Chunk chunk in world.Chunks.All)
                {
                    chunk.BeginSaving();

                    chunk.StateTransition += OnStateTransition;

                    total += 1;
                }
            }

            if (!saving.IsCompleted || progress < total)
                return null;

            if (saving.Exception is {} exception)
                LogFailedToSaveWorldMetaInformation(logger, exception);

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
    private static partial void LogWorldReady(ILogger logger, Duration readyTime, Int64 worldUpdates, Int64 chunkUpdates);

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
