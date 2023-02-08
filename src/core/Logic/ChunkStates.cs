// <copyright file="ChunkStates.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Collections;
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
        public override bool IsIntendingToGetReady => true;

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
        public override bool IsIntendingToGetReady => true;

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
    ///     Decorates the chunk.
    /// </summary>
    public class Decorating : ChunkState
    {
        private readonly Array3D<Chunk?> chunks;
        private readonly Array3D<(Chunk, Guard)?> neighbors;
        private (Task task, Guard guard)? activity;

        /// <summary>
        ///     Creates a new decorating state.
        /// </summary>
        /// <param name="self">The guard for the core write access to the chunk itself.</param>
        /// <param name="neighbors">The neighbors of this chunk, with write access guards.</param>
        public Decorating(Guard self, Array3D<(Chunk, Guard)?> neighbors) : base(self, extended: null)
        {
            Debug.Assert(neighbors.Length == 3);

            this.neighbors = neighbors;

            chunks = new Array3D<Chunk?>(length: 3);

            foreach ((int x, int y, int z) in VMath.Range3(x: 3, y: 3, z: 3))
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
        public override bool IsIntendingToGetReady => true;

        /// <inheritdoc />
        protected override void OnEnter()
        {
            chunks[x: 1, y: 1, z: 1] = Chunk;
        }

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            if (activity is not {task: {} task, guard: {} guard})
            {
                guard = Context.TryAllocate(Chunk.World.MaxDecorationTasks);

                if (guard == null) return;

                activity = (Chunk.DecorateAsync(Context.Generator, chunks), guard);
            }
            else if (task.IsCompleted)
            {
                guard.Dispose();

                foreach ((Chunk chunk, Guard guard)? potentialNeighbor in neighbors)
                    if (potentialNeighbor is {} neighbor)
                        neighbor.guard.Dispose();

                if (task.IsFaulted)
                {
                    logger.LogError(
                        Events.ChunkLoadingError,
                        task.Exception!.GetBaseException(),
                        "A critical exception occurred when decorating the chunk {Position}",
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
        protected override bool AllowStealing => true;

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
        private readonly bool wasActive;

        /// <summary>
        ///     Create the used state.
        /// </summary>
        /// <param name="wasActive">Whether the chunk was active before.</param>
        public Used(bool wasActive)
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
}

