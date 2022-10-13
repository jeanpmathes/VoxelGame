// <copyright file="ChunkStates.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;

namespace VoxelGame.Client.Logic;

public partial class ClientChunk
{
    /// <summary>
    ///     Meshes a chunk.
    /// </summary>
    public class Meshing : ChunkState
    {
        private ChunkMeshingContext? context;

        private Task<SectionMeshData[]>? task;

        /// <inheritdoc />
        protected override Access CoreAccess => Access.Read;

        /// <inheritdoc />
        protected override Access ExtendedAccess => Access.None;

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            var chunk = (ClientChunk) Chunk;
            var world = (ClientWorld) chunk.World;

            if (task == null)
            {
                if (!Context.TryAllocate(world.MaxMeshingTasks)) return;

                context = ChunkMeshingContext.Acquire(Chunk);
                task = chunk.CreateMeshDataAsync(context);
            }
            else if (task.IsCompleted)
            {
                Context.Free(world.MaxMeshingTasks);

                Debug.Assert(context != null);
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

                SetNextState(new MeshDataSending(task.Result), isRequired: true);
            }
        }
    }

    /// <summary>
    ///     Sends mesh data to the GPU.
    /// </summary>
    public class MeshDataSending : ChunkState
    {
        private readonly SectionMeshData[] meshData;
        private bool canSend;

        /// <summary>
        ///     Create a new mesh data sending state.
        /// </summary>
        /// <param name="meshData">The mesh data to send.</param>
        public MeshDataSending(SectionMeshData[] meshData)
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
            var chunk = (ClientChunk) Chunk;
            var world = (ClientWorld) chunk.World;

            if (!canSend) canSend = Context.TryAllocate(world.MaxMeshDataSends);

            if (!canSend) return;

            bool finished = chunk.DoMeshDataSetStep(meshData);

            if (!finished) return;

            Context.Free(world.MaxMeshDataSends);
            SetNextActive();
        }
    }
}
