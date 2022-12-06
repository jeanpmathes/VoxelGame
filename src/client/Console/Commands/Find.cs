﻿// <copyright file="Find.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Client.Console.Commands;
    #pragma warning disable CA1822

/// <summary>
///     Search and find any named generated object in the world.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class Find : Command
{
    /// <inheritdoc />
    public override string Name => "find";

    /// <inheritdoc />
    public override string HelpText => "Search and find any named generated object in the world.";

    /// <exclude />
    public void Invoke(string name)
    {
        Search(name);
    }

    /// <exclude />
    public void Invoke(string name, int count)
    {
        Search(name, count);
    }

    /// <exclude />
    public void Invoke(string name, int count, uint maxDistance)
    {
        Search(name, count, maxDistance);
    }

    private void Search(string name, int count = 1, uint maxDistance = Chunk.BlockSize * 100)
    {
        if (count < 1)
        {
            Context.Console.WriteError("Count must be greater than 0.");

            return;
        }

        Context.Console.WriteResponse($"Beginning search for {count} {name} elements...");

        Task.Run(() =>
        {
            IEnumerable<Vector3i> positions = Context.Player.World
                .SearchNamedGeneratedElements(Context.Player.Position.Floor(), name, maxDistance)
                .Take(count);

            foreach (Vector3i position in positions)
                Context.Console.EnqueueResponse($"Found {name} at {position}.");

            Context.Console.EnqueueResponse($"Search for {name} finished.");
        });
    }
}
