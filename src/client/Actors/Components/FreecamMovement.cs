// <copyright file="FreecamMovement.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;

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

    internal override void Move(Double pitch, Double yaw, Double deltaTime)
    {
        player.Camera.Transform.LocalRotation = Quaterniond.FromAxisAngle(Vector3d.UnitY, MathHelper.DegreesToRadians(-yaw))
                                                * Quaterniond.FromAxisAngle(Vector3d.UnitX, MathHelper.DegreesToRadians(pitch));

        player.Camera.Transform.Position += GetFlyingMovement(input, player.Head) * deltaTime;
    }
}
