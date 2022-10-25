// <copyright file="ChunkStates.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
            string dataPath = Path.Combine(Context.Directory, GetChunkFileName(Chunk.Position));

            if (File.Exists(dataPath)) SetNextState<Loading>();
            else SetNextState<Generating>();
        }
    }

    /// <summary>
    ///     Loads the chunk from disk.
    /// </summary>
    public class Loading : ChunkState
    {
        private (Task<Chunk?> task, Guard guard)? activity;

        /// <inheritdoc />
        protected override Access CoreAccess => Access.Write;

        /// <inheritdoc />
        protected override Access ExtendedAccess => Access.None;

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            if (activity is not {task: {} task, guard: {} guard})
            {
                guard = Context.TryAllocate(Chunk.World.MaxLoadingTasks);

                if (guard == null) return;

                string path = Path.Combine(Context.Directory, GetChunkFileName(Chunk.Position));
                activity = (LoadAsync(path, Chunk.Position), guard);
            }
            else if (task.IsCompleted)
            {
                guard.Dispose();

                if (task.IsFaulted)
                {
                    logger.LogError(
                        Events.ChunkLoadingError,
                        task.Exception!.GetBaseException(),
                        "An exception occurred when loading the chunk {Position}. " +
                        "The chunk has been scheduled for generation",
                        Chunk.Position);

                    SetNextState<Generating>();
                }
                else
                {
                    Chunk? loadedChunk = task.Result;

                    if (loadedChunk != null)
                    {
                        Chunk.Setup(loadedChunk);
                        SetNextReady();
                    }
                    else
                    {
                        logger.LogError(
                            Events.ChunkLoadingError,
                            "The chunk for {Position} could not be loaded, " +
                            "which can be caused by a corrupted chunk file. " +
                            "Position will be scheduled for generation",
                            Chunk.Position);

                        SetNextState<Generating>();
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Generates the chunk.
    /// </summary>
    public class Generating : ChunkState
    {
        private (Task task, Guard guard)? activity;

        /// <inheritdoc />
        protected override Access CoreAccess => Access.Write;

        /// <inheritdoc />
        protected override Access ExtendedAccess => Access.None;

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            if (activity is not {task: {} task, guard: {} guard})
            {
                guard = Context.TryAllocate(Chunk.World.MaxGenerationTasks);

                if (guard == null) return;

                activity = (Chunk.GenerateAsync(Context.Generator), guard);
            }
            else if (task.IsCompleted)
            {
                guard.Dispose();

                if (task.IsFaulted)
                {
                    logger.LogError(
                        Events.ChunkLoadingError,
                        task.Exception!.GetBaseException(),
                        "A critical exception occurred when generating the chunk {Position}",
                        Chunk.Position);

                    throw task.Exception!.GetBaseException();
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
        private (Task task, Guard guard)? activity;

        /// <inheritdoc />
        protected override Access CoreAccess => Access.Read;

        /// <inheritdoc />
        protected override Access ExtendedAccess => Access.None;

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            if (activity is not {task: {} task, guard: {} guard})
            {
                guard = Context.TryAllocate(Chunk.World.MaxSavingTasks);

                if (guard == null) return;

                activity = (Chunk.SaveAsync(Context.Directory), guard);
            }
            else if (task.IsCompleted)
            {
                guard.Dispose();

                if (task.IsFaulted)
                    logger.LogError(
                        Events.ChunkSavingError,
                        task.Exception!.GetBaseException(),
                        "An exception occurred when saving chunk {Position}. " +
                        "Chunk loss is possible",
                        Chunk.Position);

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
        protected override bool AllowSharingAccess => true;

        /// <inheritdoc />
        protected override bool AllowStealing => true;

        /// <inheritdoc />
        protected override void OnEnter()
        {
            Context.ActivateWeakly(Chunk);
        }

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            SetNextActive();
        }
    }

    /// <summary>
    ///     The chunk is used by a different
    /// </summary>
    public class Used : ChunkState
    {
        /// <inheritdoc />
        protected override Access CoreAccess => Access.None;

        /// <inheritdoc />
        protected override Access ExtendedAccess => Access.None;

        /// <inheritdoc />
        protected override void OnEnter()
        {
            Context.ActivateWeakly(Chunk);
        }

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            SetNextActive();
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

            ReleaseResources();
            Context.Deactivate(Chunk);
        }

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            SetNextReady();
        }
    }
}
