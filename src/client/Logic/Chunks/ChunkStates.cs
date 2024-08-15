// <copyright file="ChunkStates.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;

namespace VoxelGame.Client.Logic.Chunks;

public partial class Chunk
{
    #region LOGGING

    [LoggerMessage(EventId = Events.ChunkMeshingError, Level = LogLevel.Critical, Message = "An exception (critical) occurred when meshing the chunk {Position} and will be re-thrown")]
    private static partial void LogChunkMeshingError(ILogger logger, Exception exception, ChunkPosition position);

    #endregion

    /// <summary>
    ///     Utility to allow easier access without casting.
    /// </summary>
    public abstract class ChunkState : Core.Logic.Chunks.ChunkState
    {
        /// <inheritdoc />
        protected ChunkState((Guard? core, Guard? extended) guards) : base(guards.core, guards.extended) {}

        /// <inheritdoc />
        protected ChunkState() {}

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
        private const Int32 EntryDelay = 5;
        private Int32 entryDelay = EntryDelay;

        private ChunkMeshingContext? context;

        private Future<ChunkMeshData>? meshing;
        private ChunkMeshData? meshData;

        /// <summary>
        ///     Meshes a chunk and sets the data to the GPU.
        /// </summary>
        /// <param name="context">The meshing context.</param>
        public Meshing(ChunkMeshingContext context) : base(context.TakeAccess())
        {
            this.context = context;
        }

        /// <inheritdoc />
        protected override Access CoreAccess => Access.Read;

        /// <inheritdoc />
        protected override Access ExtendedAccess => Access.Write;

        /// <inheritdoc />
        protected override Boolean DelayEnter()
        {
            return true;

            if (entryDelay <= 0)
                return ChunkMeshingContext.GetNumberOfNonAcquirablePossibleFutureMeshingPartners(Chunk, BlockSide.All) > 0;

            entryDelay -= 1;

            return true;
        }

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            if (meshing == null)
            {
                Debug.Assert(context != null);

                meshing = WaitForCompletion(() => Chunk.CreateMeshData(context));
            }
            else if (meshing.IsCompleted)
            {
                Debug.Assert(context != null);

                context.Dispose();
                context = null;

                if (meshing.Exception != null)
                {
                    Exception e = meshing.Exception.GetBaseException();

                    LogChunkMeshingError(logger, e, Chunk.Position);

                    throw e;
                }

                meshData = meshing.Value!;
                Chunk.SetMeshData(meshData);

                Cleanup();

                TrySettingNextActive();
            }
        }

        /// <inheritdoc />
        protected override void Cleanup()
        {
            context?.Dispose();
            meshData?.Dispose();
        }
    }
}
