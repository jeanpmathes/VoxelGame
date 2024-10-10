// <copyright file="Dungeon.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections;
using JetBrains.Annotations;
using OpenTK.Mathematics;
using VoxelGame.Core.Generation.Dungeons;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Definitions.Blocks;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Toolkit.Collections;

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

        var concrete = (ConcreteBlock) Blocks.Instance.Concrete; // todo: fix this
        BlockInstance glass = Blocks.Instance.Glass.AsInstance();

        Coroutine.Start(GenerateDungeon);

        IEnumerable GenerateDungeon()
        {
            Random random = new();

            Generator generator = new(new Parameters(Levels: 1, Size: 9));
            Array2D<Area?> dungeon = generator.Generate(random.Next());

            for (var x = 0; x < dungeon.Length; x++)
            for (var z = 0; z < dungeon.Length; z++)
            {
                yield return null;

                Area? area = dungeon[x, z];
                Vector3i position = start + new Vector3i(x, y: 0, z) * 3;

                if (area == null)
                {
                    world.SetBlock(glass, position);
                }
                else if (area.Category == AreaCategory.Corridor)
                {
                    concrete.Place(world, FluidLevel.Eight, position);

                    foreach (BlockSide side in BlockSide.All.Sides())
                    {
                        if (!area.Connections.HasFlag(side.ToFlag())) continue;

                        Vector3i offset = side.Direction();
                        concrete.Place(world, FluidLevel.Eight, position + offset);
                    }
                }
                else
                {
                    BlockColor color = area.Category switch
                    {
                        AreaCategory.Start => BlockColor.Blue,
                        AreaCategory.Generic => BlockColor.Default,
                        AreaCategory.End => BlockColor.Orange,
                        _ => BlockColor.Red
                    };

                    for (Int32 dx = -1; dx <= 1; dx++)
                    for (Int32 dy = -1; dy <= 1; dy++)
                    for (Int32 dz = -1; dz <= 1; dz++)
                        concrete.Place(world, FluidLevel.Eight, position + (dx, dy, dz), color);
                }
            }
        }
    }
}
