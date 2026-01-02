// <copyright file="ChunkSimulator.cs" company="VoxelGame">
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
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Profiling;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic;

/// <summary>
///     Sends logic updates to all chunks in the world that require it.
/// </summary>
public partial class ChunkSimulator : WorldComponent
{
    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<ChunkSimulator>();

    #endregion LOGGING

    private readonly List<Chunk> chunksWithActors = [];

    [Constructible]
    private ChunkSimulator(World subject) : base(subject) {}

    /// <inheritdoc />
    public override void OnLogicUpdateInActiveState(Double deltaTime, Timer? updateTimer)
    {
        using Timer? simTimer = logger.BeginTimedSubScoped("Chunk Simulation", updateTimer);

        SendLogicUpdatesForSimulation(deltaTime, simTimer);
    }

    private void SendLogicUpdatesForSimulation(Double deltaTime, Timer? updateTimer)
    {
        chunksWithActors.Clear();

        using (logger.BeginTimedSubScoped("LogicUpdate Chunks", updateTimer))
        {
            Subject.Chunks.ForEachActive(SendLogicUpdateChunk);
        }

        using (logger.BeginTimedSubScoped("LogicUpdate Actors", updateTimer))
        {
            foreach (Chunk chunk in chunksWithActors)
                chunk.SendLogicUpdatesToActors(deltaTime);
        }
    }

    private void SendLogicUpdateChunk(Chunk chunk)
    {
        if (!chunk.IsRequestedToSimulate)
            return;

        chunk.LogicUpdate();

        if (chunk.HasActors)
            chunksWithActors.Add(chunk);
    }
}
