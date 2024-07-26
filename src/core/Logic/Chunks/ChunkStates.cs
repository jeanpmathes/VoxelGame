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

namespace VoxelGame.Core.Logic.Chunks;

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
        private Future<LoadingResult>? loading;

        /// <inheritdoc />
        protected override Access CoreAccess => Access.Write;

        /// <inheritdoc />
        protected override Access ExtendedAccess => Access.None;

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            if (loading == null)
            {
                FileInfo path = Context.Directory.GetFile(GetChunkFileName(Chunk.Position));
                loading = WaitForCompletion(() => Load(path, Chunk));
            }
            else if (loading.IsCompleted)
            {
                if (loading.Exception != null) HandleFaultedFuture(loading);
                else HandleSuccessfulFuture(loading);
            }
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
                    TrySettingNextReady();

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
        private Future? generating;

        /// <inheritdoc />
        protected override Access CoreAccess => Access.Write;

        /// <inheritdoc />
        protected override Access ExtendedAccess => Access.None;

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            if (generating == null)
            {
                generating = WaitForCompletion(() => Chunk.Generate(Context.Generator));
            }
            else if (generating.IsCompleted)
            {
                if (generating.Exception is {} exception)
                {
                    LogChunkGenerationError(logger, exception.GetBaseException(), Chunk.Position);

                    throw exception.GetBaseException();
                }

                TrySettingNextReady();
            }
        }
    }

    /// <summary>
    ///     Decorates the chunk.
    /// </summary>
    public class Decorating : ChunkState
    {
        private readonly Neighborhood<Chunk> chunks;
        private readonly PooledList<Guard> guards;

        private Future? decorating;

        /// <summary>
        ///     Creates a new decorating state.
        /// </summary>
        /// <param name="self">The guard for the core write access to the chunk itself.</param>
        /// <param name="guards">The guards for the core write access to the neighboring chunks.</param>
        /// <param name="chunks">The neighborhood of chunks.</param>
        public Decorating(Guard self, PooledList<Guard> guards, Neighborhood<Chunk> chunks) : base(self, extended: null)
        {
            this.chunks = chunks;
            this.guards = guards;
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
            if (decorating == null)
            {
                decorating = WaitForCompletion(() => Decorate(Context.Generator, chunks));
            }
            else if (decorating.IsCompleted)
            {
                Cleanup();

                if (decorating.Exception is {} exception)
                {
                    LogChunkDecorationError(logger, exception.GetBaseException(), Chunk.Position);

                    throw exception.GetBaseException();
                }

                TrySettingNextReady();
            }
        }

        /// <inheritdoc />
        protected override void Cleanup()
        {
            foreach (Guard guard in guards) guard.Dispose();

            guards.Dispose();
        }
    }

    /// <summary>
    ///     Saves the chunk to disk.
    /// </summary>
    public class Saving : ChunkState
    {
        private Future? saving;

        /// <inheritdoc />
        protected override Access CoreAccess => Access.Read;

        /// <inheritdoc />
        protected override Access ExtendedAccess => Access.None;

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            if (saving == null)
            {
                saving = WaitForCompletion(() => Chunk.Save(Context.Directory));
            }
            else if (saving.IsCompleted)
            {
                if (saving.Exception is {} exception)
                    LogChunkSavingError(logger, exception.GetBaseException(), Chunk.Position);

                TrySettingNextReady(new TransitionDescription
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

            // The wait will only have an effect if no transition happens.
            WaitForEvents(onTransitionRequest: true);
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
            var activated = false;

            if (Chunk.IsRequestedToActivate) activated = TrySettingNextReady();

            if (!activated)
                AllowTransition();

            // The wait will only have an effect if no transition happens.
            WaitForEvents(onNeighborUsable: true, onTransitionRequest: true);
        }
    }

    /// <summary>
    ///     The chunk is used by a different chunk or operation.
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
            if (wasActive) TrySettingNextActive();
            else TrySettingNextReady();
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
            if (Chunk.IsRequestedToLoad) return;

            Deactivate();
        }

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            TrySettingNextReady();
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
