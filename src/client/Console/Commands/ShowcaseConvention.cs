// <copyright file="ShowcaseConvention.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using JetBrains.Annotations;
using OpenTK.Mathematics;
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
}
