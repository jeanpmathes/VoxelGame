// <copyright file="ChunkSimulator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Profiling;
using VoxelGame.Logging;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic;

/// <summary>
///     Sends logic updates to all chunks in the world that require it.
/// </summary>
public class ChunkSimulator(World subject) : WorldComponent(subject), IConstructible<World, ChunkSimulator>
{
    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<ChunkSimulator>();

    #endregion LOGGING

    private readonly List<Chunk> chunksWithActors = [];

    /// <inheritdoc />
    /// 1
    public static ChunkSimulator Construct(World input)
    {
        return new ChunkSimulator(input);
    }

    /// <inheritdoc />
    public override void OnLogicUpdateInActiveState(Double deltaTime, Timer? timer)
    {
        using Timer? simTimer = logger.BeginTimedSubScoped("Chunk Simulation", timer);

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
