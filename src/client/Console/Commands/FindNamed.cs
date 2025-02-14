// <copyright file="FindNamed.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Console.Commands;
    #pragma warning disable CA1822

/// <summary>
///     Search and find any named generated entity in the world.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class FindNamed : Command
{
    /// <inheritdoc />
    public override String Name => "find-named";

    /// <inheritdoc />
    public override String HelpText => "Search and find any named generated entity in the world.";

    /// <exclude />
    public void Invoke(String name)
    {
        Search(name);
    }

    /// <exclude />
    public void Invoke(String name, Int32 count)
    {
        Search(name, count);
    }

    /// <exclude />
    public void Invoke(String name, Int32 count, UInt32 maxDistance)
    {
        Search(name, count, maxDistance);
    }

    private void Search(String name, Int32 count = 1, UInt32 maxDistance = World.BlockLimit * 2)
    {
        if (count < 1)
        {
            Context.Console.WriteError("Count must be greater than 0.");

            return;
        }

        Context.Console.WriteResponse($"Beginning search for {count} {name} elements...");

        IEnumerable<Vector3i>? positions = Context.Player.World
            .SearchNamedGeneratedElements(Context.Player.Position.Floor(), name, maxDistance);

        if (positions == null)
        {
            Context.Console.WriteError($"Search failed, name {name} not valid.");

            return;
        }

        Operations.Launch(() =>
        {
            foreach (Vector3i position in positions.Take(count))
                Context.Console.EnqueueResponse($"Found {name} at {position}.",
                    new FollowUp($"Teleport to {name}", () => Teleport.Do(Context, position)));

            Context.Console.EnqueueResponse($"Search for {name} finished.");
        });
    }
}
