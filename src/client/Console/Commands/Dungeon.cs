// <copyright file="Dungeon.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Client.Console.Commands;
    #pragma warning disable CA1822

/// <summary>
///     Clears the console.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class Dungeon : Command
{
    /// <inheritdoc />
    public override String Name => "dungeon";

    /// <inheritdoc />
    public override String HelpText => "Generate a dungeon.";

    /// <exclude />
    public void Invoke()
    {
        Vector3i start = Context.Player.Position.Floor();
        World world = Context.Player.World;

        Coroutine.Start(GenerateDungeon);

        IEnumerable GenerateDungeon()
        {
            Random random = new();

            var directions = new List<Vector3i>
            {
                (1, 0, 0), // Move right
                (-1, 0, 0), // Move left
                (0, 1, 0), // Move up
                (0, -1, 0), // Move down
                (0, 0, 1), // Move forward
                (0, 0, -1) // Move backward
            };

            Vector3i direction = directions[random.Next(directions.Count)];
            Vector3i position = start;

            while (true)
            {
                world.SetBlock(Blocks.Instance.Wood.AsInstance(), position);

                position += direction;

                if (random.Next(minValue: 0, maxValue: 10) < 2) direction = directions[random.Next(directions.Count)];

                yield return null;
            }
        }
    }
}
