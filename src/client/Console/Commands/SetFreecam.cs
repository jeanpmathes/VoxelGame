// <copyright file="SetFreecam.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using JetBrains.Annotations;
using VoxelGame.Client.Actors.Components;

namespace VoxelGame.Client.Console.Commands;

/// <summary>
///     Enable free-cam mode, in which the player can move the camera independently of the player actor.
///     This serves to investigate issues without causing chunk operations.
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
        if (Context.Player.GetComponent<PlayerMovement>() is {} movement)
            movement.SetFreecamMode(freecam);
    }
}
