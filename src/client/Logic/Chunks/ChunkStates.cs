// <copyright file="ChunkStates.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
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
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
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

    #endregion LOGGING

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
        private ChunkMeshData? meshData;

        private Future<ChunkMeshData>? meshing;

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

                meshing = WaitForCompletion(() => Task.FromResult(Chunk.CreateMeshData(context)));
            }
            else if (meshing.IsCompleted)
            {
                Debug.Assert(context != null);

                context.Dispose();
                context = null;

                meshing.Result?.Switch(
                    data =>
                    {
                        meshData = data;
                        Chunk.SetMeshData(meshData);

                        Cleanup();

                        TryActivation();
                    },
                    e =>
                    {
                        LogChunkMeshingError(logger, e, Chunk.Position);

                        ExceptionDispatchInfo.Capture(e).Throw();
                    }
                );
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
