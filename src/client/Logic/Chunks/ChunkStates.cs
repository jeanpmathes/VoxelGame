﻿// <copyright file="ChunkStates.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using VoxelGame.Client.Visuals;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Client.Logic.Chunks;

public partial class Chunk
{
    #region LOGGING

    [LoggerMessage(EventId = LogID.ChunkStates + 0, Level = LogLevel.Critical, Message = "An exception (critical) occurred when meshing the chunk {Position} and will be re-thrown")]
    private static partial void LogChunkMeshingError(ILogger logger, Exception exception, ChunkPosition position);

    #endregion

    /// <summary>
    ///     Utility to allow easier access without casting.
    /// </summary>
    public abstract class ChunkState : Core.Logic.Chunks.ChunkState
    {
        /// <inheritdoc />
        protected ChunkState(Guard? guard) : base(guard) {}

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
        protected override Access Access => Access.Read;

        /// <inheritdoc />
        protected override Boolean CanDiscard => true;

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

                TryActivation();
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
