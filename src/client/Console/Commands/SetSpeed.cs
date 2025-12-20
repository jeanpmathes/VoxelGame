// <copyright file="SetSpeed.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using JetBrains.Annotations;
using VoxelGame.Client.Actors.Components;

namespace VoxelGame.Client.Console.Commands;

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
                if (Context.Player.GetComponent<PlayerMovement>() is {} movement)
                    movement.SetFlyingSpeed(speed);

                break;
        }

    }
}
