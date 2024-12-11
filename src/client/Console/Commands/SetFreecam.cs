// <copyright file="SetFreecam.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using JetBrains.Annotations;

namespace VoxelGame.Client.Console.Commands;
#pragma warning disable CA1822

/// <summary>
///     Sets the player flying speed.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class SetFreecam : Command
{
    /// <inheritdoc />
    public override String Name => "set-freecam";

    /// <inheritdoc />
    public override String HelpText => "Sets whether the player uses freecam.";

    /// <exclude />
    public void Invoke(Boolean freecam)
    {
        Context.Player.SetFreecam(freecam);
    }
}
