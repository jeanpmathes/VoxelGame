// <copyright file="SetBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using JetBrains.Annotations;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors.Components;
using VoxelGame.Core.Logic.Contents;
using VoxelGame.Core.Logic.Voxels;

namespace VoxelGame.Client.Console.Commands;

/// <summary>
///     Sets the block at the target position. Can cause invalid block state.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class SetBlock : Command
{
    /// <inheritdoc />
    public override String Name => "set-block";

    /// <inheritdoc />
    public override String HelpText => "Sets the block at the target position. Can cause invalid block state.";

    /// <exclude />
    public void Invoke(String contentID, Int32 x, Int32 y, Int32 z)
    {
        Set(new CID(contentID), (x, y, z));
    }

    /// <exclude />
    public void Invoke(String contentID)
    {
        if (Context.Player.GetComponentOrThrow<Targeting>().Position is {} targetPosition) Set(new CID(contentID), targetPosition);
        else Context.Output.WriteError("No position targeted.");
    }

    private void Set(CID contentID, Vector3i position)
    {
        Block? block = Blocks.Instance.TranslateContentID(contentID);

        if (block == null)
        {
            Context.Output.WriteError("Cannot find block.");

            return;
        }

        Context.Player.World.SetBlock(block.States.Default, position);
    }
}
