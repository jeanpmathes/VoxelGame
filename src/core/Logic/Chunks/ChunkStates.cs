// <copyright file="ChunkStates.cs" company="VoxelGame">
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
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Generation.Worlds;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Chunks;

public partial class Chunk
{
    /// <summary>
    ///     Initial state. Tries to activate the chunk.
    /// </summary>
    public class Unloaded : ChunkState
    {
        /// <inheritdoc />
        protected override Access Access => Access.Write;

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
        protected override Access Access => Access.Write;

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            if (loading == null)
            {
                FileInfo path = Chunk.World.Data.ChunkDirectory.GetFile(GetChunkFileName(Chunk.Position));
                loading = WaitForCompletion(async () => await LoadAsync(path, Chunk).InAnyContext());
            }
            else if (loading.IsCompleted)
            {
                loading.Result?.Switch(HandleSuccessful, HandleFaulted);
            }
        }

        private void HandleFaulted(Exception exception)
        {
            LogChunkLoadingError(logger, exception, Chunk.Position);

            SetNextState<Generating>();
        }

        private void HandleSuccessful(LoadingResult result)
        {
            switch (result)
            {
                case LoadingResult.Success:
                    TryActivation();

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
                    throw Exceptions.UnsupportedEnumValue(result);
            }
        }
    }

    /// <summary>
    ///     Generates the chunk.
    /// </summary>
    public class Generating : ChunkState
    {
        private IDecorationContext? decorationContext;
        private Future? generating;

        private IGenerationContext? generationContext;

        /// <inheritdoc />
        protected override Access Access => Access.Write;

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            if (generating == null)
            {
                generationContext = Context.Generator.CreateGenerationContext(Chunk.Position);
                decorationContext = Context.Generator.CreateDecorationContext(Chunk.Position);

                generating = WaitForCompletion(() =>
                {
                    Chunk.Generate(generationContext, decorationContext);

                    return Task.CompletedTask;
                });
            }
            else if (generating.IsCompleted)
            {
                Cleanup();

                generating.Result?.Switch(
                    TryActivation,
                    e =>
                    {
                        LogChunkGenerationError(logger, e, Chunk.Position);

                        ExceptionDispatchInfo.Capture(e).Throw();

                        return false;
                    }
                );
            }
        }

        /// <inheritdoc />
        protected override void Cleanup()
        {
            generationContext?.Dispose();
            decorationContext?.Dispose();

            generationContext = null;
            decorationContext = null;
        }
    }

    /// <summary>
    ///     Decorates the chunk.
    /// </summary>
    public class Decorating : ChunkState
    {
        private readonly Neighborhood<Chunk?> chunks;
        private readonly PooledList<Guard> guards;

        private Boolean cleaned;

        private Future? decorating;

        private IDecorationContext? decorationContext;

        /// <summary>
        ///     Creates a new decorating state.
        /// </summary>
        /// <param name="self">The guard for the write access to the chunk itself.</param>
        /// <param name="guards">The guards for the write access to the neighboring chunks.</param>
        /// <param name="chunks">The neighborhood of chunks.</param>
        public Decorating(Guard self, PooledList<Guard> guards, Neighborhood<Chunk?> chunks) : base(self)
        {
            Debug.Assert(chunks.Center != null);

            this.chunks = chunks;
            this.guards = guards;
        }

        /// <inheritdoc />
        protected override Access Access => Access.Write;

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
                decorationContext = Context.Generator.CreateDecorationContext(Chunk.Position, extents: 1);

                decorating = WaitForCompletion(() =>
                {
                    Chunk.Decorate(chunks, decorationContext);

                    return Task.CompletedTask;
                });
            }
            else if (decorating.IsCompleted)
            {
                Cleanup();

                decorating.Result?.Switch(
                    TryActivation,
                    e =>
                    {
                        LogChunkDecorationError(logger, e, Chunk.Position);

                        ExceptionDispatchInfo.Capture(e).Throw();

                        return false;
                    }
                );
            }
        }

        /// <inheritdoc />
        protected override void Cleanup()
        {
            if (cleaned) return;

            foreach (Guard guard in guards) guard.Dispose();
            guards.Dispose();

            decorationContext?.Dispose();

            cleaned = true;
        }
    }

    /// <summary>
    ///     Saves the chunk to disk.
    /// </summary>
    public class Saving : ChunkState
    {
        private Future? saving;

        /// <inheritdoc />
        protected override Access Access => Access.Read;

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            if (saving == null) saving = WaitForCompletion(async () => await Chunk.SaveAsync(Chunk.World.Data.ChunkDirectory).InAnyContext());
            else if (saving.IsCompleted)
                saving.Result?.Switch(
                    () => TryActivation(),
                    exception =>
                    {
                        LogChunkSavingError(logger, exception, Chunk.Position);
                    }
                );
        }
    }

    /// <summary>
    ///     Active state. The chunk is ready to be used.
    ///     Because the state has write-access, it is safe to perform synchronous operations on the chunk during an update.
    /// </summary>
    public class Active : ChunkState
    {
        /// <inheritdoc />
        protected override Access Access => Access.Write;

        /// <inheritdoc />
        protected override Boolean AllowSharingAccess => true;

        /// <inheritdoc />
        protected override Boolean AllowStealing => true;

        /// <inheritdoc />
        protected override Boolean CanDiscard => true;

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
        protected override Access Access => Access.Write;

        /// <inheritdoc />
        protected override Boolean AllowStealing => true;

        /// <inheritdoc />
        protected override Boolean CanDiscard => true;

        /// <inheritdoc />
        protected override Boolean IsHidden => true;

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            var activated = false;

            if (Chunk.IsRequestedToActivate) activated = TryActivation();

            if (!activated)
                AllowTransition();

            // The wait will only have an effect if no transition happens.
            WaitForEvents(onNeighborUsable: true, onTransitionRequest: true, onRequestLevelChange: true);
        }
    }

    /// <summary>
    ///     Final state, the chunk is unloaded.
    /// </summary>
    public class Deactivating : ChunkState
    {
        /// <inheritdoc />
        protected override Access Access => Access.Write;

        /// <inheritdoc />
        protected override void OnEnter()
        {
            if (Chunk.IsRequestedToLoad)
                return;

            Deactivate();
        }

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            if (Chunk.IsRequestedToLoad) TryActivation();
            else Deactivate();
        }
    }

    #region LOGGING

    [LoggerMessage(EventId = LogID.ChunkStates + 0, Level = LogLevel.Error, Message = "An exception occurred when loading the chunk {Position} - the chunk has been scheduled for generation")]
    private static partial void LogChunkLoadingError(ILogger logger, Exception exception, ChunkPosition position);

    [LoggerMessage(EventId = LogID.ChunkStates + 1,
        Level = LogLevel.Debug,
        Message = "The chunk file for {Position} could not be loaded, which is likely because the file does not exist - position will be scheduled for generation")]
    private static partial void LogChunkFileNotFound(ILogger logger, ChunkPosition position);

    [LoggerMessage(EventId = LogID.ChunkStates + 2,
        Level = LogLevel.Error,
        Message = "The chunk for {Position} could not be loaded, which can be caused by a corrupted or manipulated chunk file - position will be scheduled for generation")]
    private static partial void LogChunkLoadingCorruptedFile(ILogger logger, ChunkPosition position);

    [LoggerMessage(EventId = LogID.ChunkStates + 3, Level = LogLevel.Error, Message = "A critical exception occurred when generating the chunk {Position}")]
    private static partial void LogChunkGenerationError(ILogger logger, Exception exception, ChunkPosition position);

    [LoggerMessage(EventId = LogID.ChunkStates + 4, Level = LogLevel.Error, Message = "A critical exception occurred when decorating the chunk {Position}")]
    private static partial void LogChunkDecorationError(ILogger logger, Exception exception, ChunkPosition position);

    [LoggerMessage(EventId = LogID.ChunkStates + 5, Level = LogLevel.Error, Message = "An exception occurred when saving chunk {Position} - chunk loss is possible")]
    private static partial void LogChunkSavingError(ILogger logger, Exception exception, ChunkPosition position);

    #endregion LOGGING
}
