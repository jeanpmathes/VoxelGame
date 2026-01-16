// <copyright file="FreecamMovement.cs" company="VoxelGame">
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
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Client.Actors.Components;

/// <summary>
///     Movement strategy that moves only the camera, keeping the player in place.
/// </summary>
internal sealed class FreecamMovement : MovementStrategy
{
    private readonly Player player;
    private readonly PlayerInput input;

    /// <summary>
    ///     Movement strategy that moves only the camera, keeping the player in place.
    /// </summary>
    /// <param name="player">The player to which this movement strategy belongs.</param>
    /// <param name="input">The player input to use for movement.</param>
    /// <param name="flyingSpeed">The initial flying speed.</param>
    internal FreecamMovement(Player player, PlayerInput input, Double flyingSpeed) : base(flyingSpeed)
    {
        this.player = player;
        this.input = input;

        this.player.Camera.Transform.SetParent(newParent: null);
    }

    internal override void Move(Double pitch, Double yaw, Delta delta)
    {
        player.Camera.Transform.LocalRotation = Quaterniond.FromAxisAngle(Vector3d.UnitY, MathHelper.DegreesToRadians(-yaw))
                                                * Quaterniond.FromAxisAngle(Vector3d.UnitX, MathHelper.DegreesToRadians(pitch));

        player.Camera.Transform.Position += GetFlyingMovement(input, player.Head) * delta.RealTime;
    }
}
