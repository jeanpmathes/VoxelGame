﻿// <copyright file="ChunkStates.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;
using VoxelGame.Support.Data;

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
        private const Int32 EntryDelay = 15;

        private Int32 entryDelay = EntryDelay;

        private BlockSide side;

        private (Future<ChunkMeshData> future, ChunkMeshingContext context)? activity;

        /// <summary>
        ///     Meshes a chunk.
        /// </summary>
        /// <param name="side">
        ///     The side of the chunk that caused the meshing to start, or <see cref="BlockSide.All" /> if not
        ///     applicable.
        /// </param>
        public Meshing(BlockSide side)
        {
            this.side = side;
        }

        /// <inheritdoc />
        protected override Access CoreAccess => Access.Read;

        /// <inheritdoc />
        protected override Access ExtendedAccess => Access.None;

        /// <inheritdoc />
        protected override Boolean DelayEnter()
        {
            if (entryDelay <= 0)
                return ChunkMeshingContext.GetNumberOfNonAcquirablePossibleFutureMeshingPartners(Chunk, side) > 0;

            entryDelay -= 1;

            return true;
        }

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            if (activity is not {future: {} future, context: {} context})
            {
                context = ChunkMeshingContext.Acquire(Chunk, Chunk.meshedSides, side, SpatialMeshingFactory.Shared);
                activity = (WaitForCompletion(() => Chunk.CreateMeshData(context)), context);
            }
            else if (future.IsCompleted)
            {
                context.Release();

                if (future.Exception != null)
                {
                    Exception e = future.Exception.GetBaseException();

                    LogChunkMeshingError(logger, e, Chunk.Position);

                    throw e;
                }

                SetNextState(new MeshDataSending(future.Value!),
                    new TransitionDescription
                    {
                        PrioritizeLoop = true,
                        PrioritizeDeactivation = true
                    });
            }
        }

        /// <inheritdoc />
        protected override Core.Logic.Chunks.ChunkState ResolveDuplicate(Core.Logic.Chunks.ChunkState other)
        {
            if (side == BlockSide.All) return this;
            if (((Meshing) other).side == BlockSide.All) return other;

            side = BlockSide.All;

            return this;
        }

        /// <inheritdoc />
        public override String ToString()
        {
            return $"Meshing({side})";
        }
    }

    /// <summary>
    ///     Sends mesh data to the GPU.
    /// </summary>
    public class MeshDataSending : ChunkState
    {
        private readonly ChunkMeshData meshData;

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
            Chunk.SetMeshData(meshData);

            Cleanup();

            TrySettingNextActive();
        }

        /// <inheritdoc />
        protected override void Cleanup()
        {
            meshData.Dispose();
        }
    }
}
