﻿// <copyright file="ChunkStates.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;
using VoxelGame.Support.Data;

namespace VoxelGame.Client.Logic;

public partial class Chunk
{
    /// <summary>
    ///     Utility to allow easier access without casting.
    /// </summary>
    public abstract class ChunkState : Core.Logic.ChunkState
    {
        /// <summary>
        ///     Access the client chunk.
        /// </summary>
        protected new Chunk Chunk => base.Chunk.Cast();
    }

    /// <summary>
    ///     Meshes a chunk.
    /// </summary>
    public class Meshing : ChunkState
    {
        private (Task<ChunkMeshData> task, Guard guard, ChunkMeshingContext context)? activity;

        /// <inheritdoc />
        protected override Access CoreAccess => Access.Read;

        /// <inheritdoc />
        protected override Access ExtendedAccess => Access.None;

        /// <inheritdoc />
        protected override Boolean WaitOnNeighbors => true;

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            if (activity is not {task: {} task, guard: {} guard, context: {} context})
            {
                guard = Context.TryAllocate(Chunk.World.MaxMeshingTasks);

                if (guard == null) return;

                context = ChunkMeshingContext.Acquire(Chunk, SpatialMeshingFactory.Shared);
                activity = (Chunk.CreateMeshDataAsync(context), guard, context);
            }
            else if (task.IsCompleted)
            {
                guard.Dispose();
                context.Release();

                if (task.IsFaulted)
                {
                    Exception e = task.Exception?.GetBaseException() ?? new NullReferenceException();

                    logger.LogCritical(
                        Events.ChunkMeshingError,
                        e,
                        "An exception (critical) occurred when meshing the chunk {Position} and will be re-thrown",
                        Chunk.Position);

                    throw e;
                }

                SetNextState(new MeshDataSending(task.Result),
                    new TransitionDescription
                    {
                        Cleanup = () =>
                        {
                            task.Result.Dispose();
                        },
                        PrioritizeLoop = true,
                        PrioritizeDeactivation = true
                    });
            }
        }
    }

    /// <summary>
    ///     Sends mesh data to the GPU.
    /// </summary>
    public class MeshDataSending : ChunkState
    {
        private readonly ChunkMeshData meshData;
        private Guard? guard;

        /// <summary>
        ///     Create a new mesh data sending state.
        /// </summary>
        /// <param name="meshData">The mesh data to send.</param>
        public MeshDataSending(ChunkMeshData meshData)
        {
            this.meshData = meshData;
        }

        /// <inheritdoc />
        protected override Access CoreAccess => Access.None;

        /// <inheritdoc />
        protected override Access ExtendedAccess => Access.Write;

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            guard ??= Context.TryAllocate(Chunk.World.MaxMeshDataSends);

            if (guard == null) return;

            Boolean finished = Chunk.DoMeshDataSetStep(meshData);

            if (!finished) return;

            meshData.Dispose();

            guard.Dispose();
            SetNextActive();
        }
    }
}
