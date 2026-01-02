// <copyright file="SetFreecam.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
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
