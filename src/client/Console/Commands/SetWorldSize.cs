// <copyright file="SetWorldSize.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

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
    public override string Name => "set-worldsize";

    /// <inheritdoc />
    public override string HelpText => "Sets the size of the current world.";

    /// <exclude />
    public void Invoke(uint size)
    {
        Context.Player.World.SizeInBlocks = size;
    }
}

