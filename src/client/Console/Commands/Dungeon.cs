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
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels;
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
        Vector3i start = Context.Player.Body.Transform.Position.Floor();
        World world = Context.Player.World;

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
                    PlaceBlock(world, position, category: null);
                }
                else if (area.Category == AreaCategory.Corridor)
                {
                    PlaceBlock(world, position, area.Category);

                    foreach (Side side in Side.All.Sides())
                    {
                        if (!area.Connections.HasFlag(side.ToFlag())) continue;

                        PlaceBlock(world, position + side.Direction(), AreaCategory.Corridor);
                    }
                }
                else
                {
                    for (Int32 dx = -1; dx <= 1; dx++)
                    for (Int32 dy = -1; dy <= 1; dy++)
                    for (Int32 dz = -1; dz <= 1; dz++)
                        PlaceBlock(world, position + new Vector3i(dx, dy, dz), area.Category);
                }
            }
        }
    }

    private static void PlaceBlock(World world, Vector3i position, AreaCategory? category)
    {
        State state;

        if (category == null)
            state = Blocks.Instance.Construction.Glass.States.Default;
        else
        {
            Block block = category.Value switch
            {
                AreaCategory.Start => Blocks.Instance.Metals.Steel,
                AreaCategory.End => Blocks.Instance.Woods.Ebony.Planks,
                AreaCategory.Generic => Blocks.Instance.Stones.Limestone.Base,
                AreaCategory.Corridor => Blocks.Instance.Stones.Limestone.Bricks,
                _ => Blocks.Instance.Construction.Concrete
            };

            state = block.States.Default;
        }

        world.SetBlock(state, position);
    }
}
