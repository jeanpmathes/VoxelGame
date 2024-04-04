// <copyright file="SetWorldSize.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using JetBrains.Annotations;

namespace VoxelGame.Client.Console.Commands;
    #pragma warning disable CA1822

/// <summary>
///     Sets the size of the current world.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class SetWorldSize : Command
{
    /// <inheritdoc />
    public override String Name => "set-worldsize";

    /// <inheritdoc />
    public override String HelpText => "Sets the size of the current world.";

    /// <exclude />
    public void Invoke(UInt32 size)
    {
        Context.Player.World.SizeInBlocks = size;
    }
}
