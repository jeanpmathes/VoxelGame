// <copyright file="ChunkStates.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Diagnostics;
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

            if (File.Exists(dataPath)) SetNextState<Loading>(isRequired: true);
            else SetNextState<Generating>(isRequired: true);
        }
    }

    /// <summary>
    ///     Loads the chunk from disk.
    /// </summary>
    public class Loading : ChunkState
    {
        private Task<Chunk?>? task;

        /// <inheritdoc />
        protected override Access CoreAccess => Access.Write;

        /// <inheritdoc />
        protected override Access ExtendedAccess => Access.None;

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            if (task == null)
            {
                if (Context.TryAllocate(Chunk.World.MaxLoadingTasks))
                {
                    string path = Path.Combine(Context.Directory, GetChunkFileName(Chunk.Position));
                    task = LoadAsync(path, Chunk.Position);
                }
            }
            else if (task.IsCompleted)
            {
                Context.Free(Chunk.World.MaxLoadingTasks);

                if (task.IsFaulted)
                {
                    logger.LogError(
                        Events.ChunkLoadingError,
                        task.Exception!.GetBaseException(),
                        "An exception occurred when loading the chunk {Position}. " +
                        "The chunk has been scheduled for generation",
                        Chunk.Position);

                    SetNextState<Generating>(isRequired: true);
                }
                else
                {
                    Chunk? loadedChunk = task.Result;

                    if (loadedChunk != null)
                    {
                        Chunk.Setup(loadedChunk);
                        SetNextReady(isRequired: true);
                    }
                    else
                    {
                        logger.LogError(
                            Events.ChunkLoadingError,
                            "The chunk for {Position} could not be loaded, " +
                            "which can be caused by a corrupted chunk file. " +
                            "Position will be scheduled for generation",
                            Chunk.Position);

                        SetNextState<Generating>(isRequired: true);
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
        private Task? task;

        /// <inheritdoc />
        protected override Access CoreAccess => Access.Write;

        /// <inheritdoc />
        protected override Access ExtendedAccess => Access.None;

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            if (task == null)
            {
                if (Context.TryAllocate(Chunk.World.MaxGenerationTasks)) task = Chunk.GenerateAsync(Context.Generator);
            }
            else if (task.IsCompleted)
            {
                Context.Free(Chunk.World.MaxGenerationTasks);

                if (task.IsFaulted)
                {
                    logger.LogError(
                        Events.ChunkLoadingError,
                        task.Exception!.GetBaseException(),
                        "A critical exception occurred when generating the chunk {Position}",
                        Chunk.Position);

                    throw task.Exception!.GetBaseException();
                }

                SetNextReady(isRequired: true);
            }
        }
    }

    /// <summary>
    ///     Saves the chunk to disk.
    /// </summary>
    public class Saving : ChunkState
    {
        private Task? task;

        /// <inheritdoc />
        protected override Access CoreAccess => Access.Read;

        /// <inheritdoc />
        protected override Access ExtendedAccess => Access.None;

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            if (task == null)
            {
                if (Context.TryAllocate(Chunk.World.MaxSavingTasks)) task = Chunk.SaveAsync(Context.Directory);
            }
            else if (task.IsCompleted)
            {
                Context.Free(Chunk.World.MaxSavingTasks);

                if (task.IsFaulted)
                    logger.LogError(
                        Events.ChunkSavingError,
                        task.Exception!.GetBaseException(),
                        "An exception occurred when saving chunk {Position}. " +
                        "Chunk loss is possible",
                        Chunk.Position);

                SetNextReady(isRequired: true);
            }
        }
    }

    /// <summary>
    ///     Active state. The chunk is ready to be used.
    ///     Because the state has write-access, it is safe to perform operations on the chunk during one update.
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
        protected override bool IsFinal => true;

        /// <inheritdoc />
        protected override void OnEnter()
        {
            Context.Deactivate(Chunk);
        }

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            Debug.Fail("Deactivating state should never be updated.");
        }
    }
}
