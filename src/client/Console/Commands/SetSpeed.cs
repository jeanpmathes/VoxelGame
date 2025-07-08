// <copyright file="SetSpeed.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using JetBrains.Annotations;
using VoxelGame.Client.Actors.Components;

namespace VoxelGame.Client.Console.Commands;
#pragma warning disable CA1822

/// <summary>
///     Sets the player flying speed.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class SetSpeed : Command
{
    /// <inheritdoc />
    public override String Name => "set-speed";

    /// <inheritdoc />
    public override String HelpText => "Sets the player flying speed.";

    /// <exclude />
    public void Invoke(Double speed)
    {
        switch (speed)
        {
            case < 0.25:
                Context.Output.WriteError("Speed must be at least 0.25");

                return;

            case > 25.0:
                Context.Output.WriteError("Speed must be at most 25.0");

                return;

            default:
                if (Context.Player.GetComponent<PlayerMovement>() is { } movement)
                    movement.SetFlyingSpeed(speed);

                break;
        }

    }
}
