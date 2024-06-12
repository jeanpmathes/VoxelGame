// <copyright file="ChunkStates.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.IO;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic;

public partial class Chunk
{
    /// <summary>
    ///     Initial state. Tries to activate the chunk.
    /// </summary>
    public class Unloaded : ChunkState
    {
        /// <inheritdoc />
        protected override Access CoreAccess => Access.Write;

        /// <inheritdoc />
        protected override Access ExtendedAccess => Access.Write;

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            SetNextState<Loading>();
        }
    }

    /// <summary>
    ///     Loads the chunk from disk.
    /// </summary>
    public class Loading : ChunkState
    {
        private Future<LoadingResult>? activity;

        /// <inheritdoc />
        protected override Access CoreAccess => Access.Write;

        /// <inheritdoc />
        protected override Access ExtendedAccess => Access.None;

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            if (activity == null)
            {
                TryStartLoading();
            }
            else if (activity.IsCompleted)
            {
                if (activity.Exception != null) HandleFaultedFuture(activity);
                else HandleSuccessfulFuture(activity);
            }
        }

        private void TryStartLoading()
        {
            FileInfo path = Context.Directory.GetFile(GetChunkFileName(Chunk.Position));
            activity = Future.Create(() => Load(path, Chunk));
        }

        private void HandleFaultedFuture(Future future)
        {
            LogChunkLoadingError(logger, future.Exception!.GetBaseException(), Chunk.Position);

            SetNextState<Generating>();
        }

        private void HandleSuccessfulFuture(Future<LoadingResult> future)
        {
            switch (future.Value!)
            {
                case LoadingResult.Success:
                    SetNextReady();

                    break;

                case LoadingResult.IOError:
                {
                    LogChunkFileNotFound(logger, Chunk.Position);

                    SetNextState<Generating>();

                    break;
                }

                case LoadingResult.FormatError or LoadingResult.ValidationError:
                {
                    LogChunkLoadingCorruptedFile(logger, Chunk.Position);

                    SetNextState<Generating>();

                    break;
                }

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    /// <summary>
    ///     Generates the chunk.
    /// </summary>
    public class Generating : ChunkState
    {
        private Future? activity;

        /// <inheritdoc />
        protected override Access CoreAccess => Access.Write;

        /// <inheritdoc />
        protected override Access ExtendedAccess => Access.None;

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            if (activity == null)
            {
                activity = Future.Create(() => Chunk.Generate(Context.Generator));
            }
            else if (activity.IsCompleted)
            {
                if (activity.Exception is {} exception)
                {
                    LogChunkGenerationError(logger, exception.GetBaseException(), Chunk.Position);

                    throw exception.GetBaseException();
                }

                SetNextReady();
            }
        }
    }

    /// <summary>
    ///     Decorates the chunk.
    /// </summary>
    public class Decorating : ChunkState
    {
        private readonly Neighborhood<Chunk?> chunks;
        private readonly Neighborhood<(Chunk, Guard)?> neighbors;

        private Future? activity;

        /// <summary>
        ///     Creates a new decorating state.
        /// </summary>
        /// <param name="self">The guard for the core write access to the chunk itself.</param>
        /// <param name="neighbors">The neighbors of this chunk, with write access guards.</param>
        public Decorating(Guard self, Neighborhood<(Chunk, Guard)?> neighbors) : base(self, extended: null)
        {
            this.neighbors = neighbors;

            chunks = new Neighborhood<Chunk?>();

            foreach ((Int32 x, Int32 y, Int32 z) in Neighborhood.Indices)
            {
                (Chunk chunk, Guard)? neighbor = neighbors[x, y, z];

                if (neighbor is {chunk: {} chunk})
                    chunks[x, y, z] = chunk;
            }
        }

        /// <inheritdoc />
        protected override Access CoreAccess => Access.Write;

        /// <inheritdoc />
        protected override Access ExtendedAccess => Access.None;

        /// <inheritdoc />
        protected override void OnEnter()
        {
            chunks.Center = Chunk;
        }

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            if (activity == null)
            {
                activity = Future.Create(() => Decorate(Context.Generator, chunks));
            }
            else if (activity.IsCompleted)
            {
                foreach ((Chunk chunk, Guard guard)? potentialNeighbor in neighbors)
                    if (potentialNeighbor is {} neighbor)
                        neighbor.guard.Dispose();

                if (activity.Exception is {} exception)
                {
                    LogChunkDecorationError(logger, exception.GetBaseException(), Chunk.Position);

                    throw exception.GetBaseException();
                }

                SetNextReady();
            }
        }
    }

    /// <summary>
    ///     Saves the chunk to disk.
    /// </summary>
    public class Saving : ChunkState
    {
        private Future? activity;

        /// <inheritdoc />
        protected override Access CoreAccess => Access.Read;

        /// <inheritdoc />
        protected override Access ExtendedAccess => Access.None;

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            if (activity == null)
            {
                activity = Future.Create(() => Chunk.Save(Context.Directory));
            }
            else if (activity.IsCompleted)
            {
                if (activity.Exception is {} exception)
                    LogChunkSavingError(logger, exception.GetBaseException(), Chunk.Position);

                SetNextReady(new TransitionDescription
                {
                    PrioritizeDeactivation = true
                });
            }
        }
    }

    /// <summary>
    ///     Active state. The chunk is ready to be used.
    ///     Because the state has write-access, it is safe to perform synchronous operations on the chunk during an update.
    /// </summary>
    public class Active : ChunkState
    {
        /// <inheritdoc />
        protected override Access CoreAccess => Access.Write;

        /// <inheritdoc />
        protected override Access ExtendedAccess => Access.Write;

        /// <inheritdoc />
        protected override Boolean AllowSharingAccess => true;

        /// <inheritdoc />
        protected override Boolean AllowStealing => true;

        /// <inheritdoc />
        protected override void OnEnter()
        {
            Chunk.OnActiveState();
        }

        /// <inheritdoc />
        protected override void OnExit()
        {
            Chunk.OnInactiveState();
        }

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            AllowTransition();
        }
    }

    /// <summary>
    ///     Hidden state. The chunk is not completely ready, but in a state that allows some operations.
    /// </summary>
    public class Hidden : ChunkState
    {
        /// <inheritdoc />
        protected override Access CoreAccess => Access.Write;

        /// <inheritdoc />
        protected override Access ExtendedAccess => Access.Write;

        /// <inheritdoc />
        protected override Boolean AllowStealing => true;

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            if (Chunk.IsFullyDecorated) SetNextReady();

            AllowTransition();
        }
    }

    /// <summary>
    ///     The chunk is used by a different
    /// </summary>
    public class Used : ChunkState
    {
        private readonly Boolean wasActive;

        /// <summary>
        ///     Create the used state.
        /// </summary>
        /// <param name="wasActive">Whether the chunk was active before.</param>
        public Used(Boolean wasActive)
        {
            this.wasActive = wasActive;
        }

        /// <inheritdoc />
        protected override Access CoreAccess => Access.None;

        /// <inheritdoc />
        protected override Access ExtendedAccess => Access.None;

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            if (wasActive) SetNextActive();
            else SetNextReady();
        }
    }

    /// <summary>
    ///     Final state, the chunk is unloaded.
    /// </summary>
    public class Deactivating : ChunkState
    {
        /// <inheritdoc />
        protected override Access CoreAccess => Access.Write;

        /// <inheritdoc />
        protected override Access ExtendedAccess => Access.Write;

        /// <inheritdoc />
        protected override void OnEnter()
        {
            if (Chunk.IsRequested) return;

            Deactivate();
        }

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            SetNextReady();
        }
    }

    #region LOGGING

    [LoggerMessage(EventId = Events.ChunkLoadingError, Level = LogLevel.Error, Message = "An exception occurred when loading the chunk {Position} - the chunk has been scheduled for generation")]
    private static partial void LogChunkLoadingError(ILogger logger, Exception exception, ChunkPosition position);

    [LoggerMessage(EventId = Events.ChunkLoadingError,
        Level = LogLevel.Debug,
        Message = "The chunk file for {Position} could not be loaded, which is likely because the file does not exist - position will be scheduled for generation")]
    private static partial void LogChunkFileNotFound(ILogger logger, ChunkPosition position);

    [LoggerMessage(EventId = Events.ChunkLoadingError,
        Level = LogLevel.Error,
        Message = "The chunk for {Position} could not be loaded, which can be caused by a corrupted or manipulated chunk file - position will be scheduled for generation")]
    private static partial void LogChunkLoadingCorruptedFile(ILogger logger, ChunkPosition position);

    [LoggerMessage(EventId = Events.ChunkLoadingError, Level = LogLevel.Error, Message = "A critical exception occurred when generating the chunk {Position}")]
    private static partial void LogChunkGenerationError(ILogger logger, Exception exception, ChunkPosition position);

    [LoggerMessage(EventId = Events.ChunkLoadingError, Level = LogLevel.Error, Message = "A critical exception occurred when decorating the chunk {Position}")]
    private static partial void LogChunkDecorationError(ILogger logger, Exception exception, ChunkPosition position);

    [LoggerMessage(EventId = Events.ChunkSavingError, Level = LogLevel.Error, Message = "An exception occurred when saving chunk {Position} - chunk loss is possible")]
    private static partial void LogChunkSavingError(ILogger logger, Exception exception, ChunkPosition position);

    #endregion LOGGING
}
