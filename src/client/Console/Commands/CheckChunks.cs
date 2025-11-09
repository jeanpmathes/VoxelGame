// <copyright file="CheckChunks.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using VoxelGame.Core.Actors.Components;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Console.Commands;

/// <summary>
///     Checks for stale or missing chunks.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class CheckChunks : Command
{
    /// <inheritdoc />
    public override String Name => "check-chunks";

    /// <inheritdoc />
    public override String HelpText => "Finds stale or missing chunks.";

    /// <exclude />
    public void Invoke()
    {
        var found = false;

        foreach (ChunkPosition position in RequestAlgorithm.GetPositionsInManhattanRange(Context.Player.GetComponentOrThrow<ChunkLoader>().Chunk, RequestLevel.Range))
        {
            Chunk? chunk = Context.Player.World.Chunks.GetAny(position);

            if (chunk is not null) continue;

            Context.Output.WriteError($"Chunk at {position} in range of player is missing.");

            found = true;
        }

        foreach (Chunk chunk in Context.Player.World.Chunks.All)
        {
            if (chunk is {IsRequestedToActivate: true, IsActive: false})
            {
                ReportFoundChunk(chunk, $"{chunk} is requested to activate but is not active.");
                found = true;
            }

            if (chunk is {IsRequestedToLoad: false})
            {
                ReportFoundChunk(chunk, $"{chunk} is not requested to load but exists.");
                found = true;
            }
        }

        if (!found)
            Context.Output.WriteResponse("Chunks seem OK.");

        void ReportFoundChunk(Chunk chunk, String message)
        {
            Context.Output.WriteError(message,
            [
                new FollowUp("Break",
                    () =>
                    {
                        Debugger.Break();
                        Debugger.Log(level: 0, "CheckChunks", chunk.ToString());
                    })
            ]);
        }
    }
}
