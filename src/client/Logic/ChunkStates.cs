// <copyright file="ChunkStates.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Logic;
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
        private Task<SectionMeshData[]>? task;

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            var chunk = (ClientChunk) Chunk;
            var world = (ClientWorld) chunk.World;

            if (task == null)
            {
                if (Context.TryAllocate(world.MaxMeshingTasks)) task = chunk.CreateMeshDataAsync();
            }
            else if (task.IsCompleted)
            {
                Context.Free(world.MaxMeshingTasks);

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
