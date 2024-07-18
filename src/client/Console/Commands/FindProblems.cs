// <copyright file="Clear.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Logic;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Console.Commands;
    #pragma warning disable CA1822

/// <summary>
///     Gets the world seed.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class FindProblems : Command // todo: remove this command
{
    /// <inheritdoc />
    public override String Name => "find-problems";

    /// <inheritdoc />
    public override String HelpText => "Finds problems in the world.";

    /// <exclude />
    public void Invoke()
    {
        ChunkPosition center = Context.Player.Chunk;
        Int32 distance = Player.LoadDistance;

        for (Int32 x = -distance; x <= distance; x++)
        for (Int32 y = -distance; y <= distance; y++)
        for (Int32 z = -distance; z <= distance; z++)
        {
            ChunkPosition current = center.Offset(x, y, z);

            if (Context.Player.World.TryGetChunk(current, out Chunk? chunk))
            {
                if (!chunk.IsActive)
                    Context.Console.WriteResponse($"Chunk {current} is not active.",
                        new FollowUp("Break",
                            () =>
                            {
                                Debugger.Break();
                                System.Console.WriteLine(chunk);
                            }));
            }
            else
            {
                Context.Console.WriteResponse($"Chunk {current} does not exist.");
            }
        }

        distance += 1;

        foreach (Chunk chunk in Context.Player.World.LeChunks)
        {
            ChunkPosition position = chunk.Position;

            if (position.X < center.X - distance || position.X > center.X + distance ||
                position.Y < center.Y - distance || position.Y > center.Y + distance ||
                position.Z < center.Z - distance || position.Z > center.Z + distance)
                Context.Console.WriteResponse($"Chunk {position} is out of range.",
                    new FollowUp("Break",
                        () =>
                        {
                            Debugger.Break();
                            System.Console.WriteLine(chunk);
                        }));
        }
    }
}
