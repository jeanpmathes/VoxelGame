// <copyright file="SetBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using JetBrains.Annotations;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;

namespace VoxelGame.Client.Console.Commands;
    #pragma warning disable CA1822

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
    public void Invoke(String namedID, Int32 data, Int32 x, Int32 y, Int32 z)
    {
        Set(namedID, data, (x, y, z));
    }

    /// <exclude />
    public void Invoke(String namedID, Int32 data)
    {
        if (Context.Player.TargetPosition is {} targetPosition) Set(namedID, data, targetPosition);
        else Context.Console.WriteError("No position targeted.");
    }

    private void Set(String namedID, Int32 data, Vector3i position)
    {
        Block? block = Blocks.Instance.TranslateNamedID(namedID);

        if (block == null)
        {
            Context.Console.WriteError("Cannot find block.");

            return;
        }

        if (data is < 0 or > 0b11_1111)
        {
            Context.Console.WriteError("Invalid data value.");

            return;
        }

        Context.Player.World.SetBlock(block.AsInstance((UInt32) data), position);
    }
}
