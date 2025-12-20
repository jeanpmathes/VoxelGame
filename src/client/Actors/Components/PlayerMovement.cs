// <copyright file="PlayerMovement.cs" company="VoxelGame">
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
using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Actors;

namespace VoxelGame.Client.Actors.Components;

/// <summary>
///     Implements the movement for the player.
/// </summary>
public partial class PlayerMovement : ActorComponent
{
    [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "Is only borrowed by this class.")]
    private readonly PlayerInput input;

    private readonly Player player;

    private MovementStrategy strategy;

    private Double pitch;
    private Double yaw;

    private Targeter? targeter;

    [Constructible]
    private PlayerMovement(Player player) : base(player)
    {
        this.player = player;

        input = player.GetRequiredComponent<PlayerInput, Player>();

        strategy = new DefaultMovement(player, input, flyingSpeed: 1.0);
    }

    /// <summary>
    ///     Set the flying speed of the player.
    /// </summary>
    public void SetFlyingSpeed(Double speed)
    {
        strategy.FlyingSpeed = speed;
    }

    /// <summary>
    ///     Set whether freecam mode is enabled or not.
    /// </summary>
    /// <param name="enabled"><c>true</c> to enable freecam mode, <c>false</c> to disable it.</param>
    public void SetFreecamMode(Boolean enabled)
    {
        strategy = enabled
            ? new FreecamMovement(player, input, strategy.FlyingSpeed)
            : new DefaultMovement(player, input, strategy.FlyingSpeed);
    }

    /// <inheritdoc />
    public override void OnLogicUpdate(Double deltaTime)
    {
        if (player.Input.CanHandleGameInput)
        {
            (Double yawDelta, Double pitchDelta) = player.Input.Keybinds.LookBind.Value;

            yaw += yawDelta;
            pitch += pitchDelta;

            pitch = MathHelper.Clamp(pitch, min: -89.0, max: 89.0);

            strategy.Move(pitch, yaw, deltaTime);
        }

        // The targeter is acquired here to ensure it is ordered after this component.
        // Targeting is update twice in total, as both camera movement and world manipulation can change the target.
        targeter ??= player.GetComponent<Targeter>();
        targeter?.Update();
    }
}
