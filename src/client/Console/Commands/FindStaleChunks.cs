// <copyright file="FindStaleChunks.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Console.Commands;
    #pragma warning disable CA1822

/// <summary>
///     Gets the world seed.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class FindStaleChunks : Command
{
    /// <inheritdoc />
    public override String Name => "find-stale-chunks";

    /// <inheritdoc />
    public override String HelpText => "Finds chunks that seem to be stale.";

    /// <exclude />
    public void Invoke()
    {
        var found = false;

        foreach (Chunk chunk in Context.Player.World.Chunks)
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

        if (!found) Context.Console.WriteResponse("No stale chunks found.");

        void ReportFoundChunk(Chunk chunk, String message)
        {
            Context.Console.WriteError(message,
                new FollowUp("Break",
                    () =>
                    {
                        Debugger.Break();
                        Debugger.Log(level: 0, "FindStaleChunks", chunk.ToString());
                    }));
        }
    }
}
