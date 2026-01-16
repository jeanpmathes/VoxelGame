// <copyright file="PlayerMovementStrategy.cs" company="VoxelGame">
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
using VoxelGame.Core.Actors.Components;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Client.Actors.Components;

/// <summary>
///     The strategy to use for moving the player and camera.
/// </summary>
/// <param name="flyingSpeed">The initial flying speed.</param>
internal abstract class MovementStrategy(Double flyingSpeed)
{
    private const Single FlyingSpeedFactor = 5f;
    private const Single FlyingSprintSpeedFactor = 25f;

    /// <summary>
    ///     Gets or sets the flying speed.
    /// </summary>
    internal Double FlyingSpeed { get; set; } = flyingSpeed;

    /// <summary>
    ///     Get the flying movement for a given transform.
    /// </summary>
    protected Vector3d GetFlyingMovement(PlayerInput input, Transform transform)
    {
        return input.GetMovement(
            transform,
            FlyingSpeed * FlyingSpeedFactor,
            FlyingSpeed * FlyingSprintSpeedFactor,
            allowFlying: true);
    }

    /// <summary>
    /// Perform the movement calculations and actions associated with the strategy.
    /// Should be called once (or less) per update cycle.
    /// </summary>
    /// <param name="pitch">The current look pitch.</param>
    /// <param name="yaw">The current look yaw.</param>
    /// <param name="delta">The time since the last update cycle.</param>
    internal abstract void Move(Double pitch, Double yaw, Delta delta);
}
