// <copyright file="ShowcaseConvention.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using JetBrains.Annotations;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Definitions;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Elements.Conventions;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Client.Console.Commands;

/// <summary>
///     Showcases all content of a convention by placing it in the world around the player.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class ShowcaseConvention : Command 
{
    /// <inheritdoc />
    public override String Name => "showcase-convention";

    /// <inheritdoc />
    public override String HelpText => "Place all content of a convention in the world around the player.";

    /// <exclude />
    public void Invoke(String convention)
    {
        switch (convention)
        {
            case nameof(Coal):
                ShowcaseCoal();
                break;

            case nameof(Crop):
                ShowcaseCrops();
                break;

            default:
                Context.Output.WriteError($"No known convention '{convention}'.");
                return;
        }
    }

    private void ShowcaseCoal()
    {
        Vector3i position = Context.Player.Body.Transform.Position.Floor();
        
        foreach (IContent content in Blocks.Instance.Coals.Contents)
        {
            if (content is not Coal coal) continue;
            
            position += Vector3i.UnitX;

            coal.Block.Place(Context.Player.World, position);
        }
    }

    private void ShowcaseCrops()
    {
        Vector3i position = Context.Player.Body.Transform.Position.Floor();
        World world = Context.Player.World;

        foreach (IContent content in Blocks.Instance.Crops.Contents)
        {
            if (content is not Crop crop) continue;

            position += Vector3i.UnitX * 3;

            Vector3i farmland = position;
            
            Blocks.Instance.Core.Dev.Place(world, farmland.Below());
            Blocks.Instance.Environment.Farmland.Place(world, farmland);
            
            crop.Plant.Place(world, farmland.Above());
            
            for (Int32 x = -1; x <= 1; x++)
            for (Int32 z = -1; z <= 1; z++)
            {
                if (x == 0 && z == 0) continue;

                Blocks.Instance.Core.Dev.Place(world, farmland + new Vector3i(x, y: +0, z));
                Blocks.Instance.Core.Dev.Place(world, farmland + new Vector3i(x, y: -1, z));
            }

            if (crop.Fruit == null) continue;

            Vector3i fruit = position + Vector3i.UnitZ * 2;
            Blocks.Instance.Core.Dev.Place(world, fruit.Below());
            crop.Fruit.Place(world, fruit);
        }
    }
}
