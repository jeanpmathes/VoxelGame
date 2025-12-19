// <copyright file="DefaultMovementStrategy.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors.Components;

namespace VoxelGame.Client.Actors.Components;

/// <summary>
///     Default player movement, using physics.
/// </summary>
internal sealed class DefaultMovement : MovementStrategy
{
    private const Single DiveSpeed = 8f;
    private const Single JumpForce = 25000f;
    private const Single Speed = 4f;
    private const Single SprintSpeed = 6f;
    private const Single SwimSpeed = 4f;

    private readonly Vector3d maxForce = new(x: 500f, y: 0f, z: 500f);
    private readonly Vector3d maxSwimForce = new(x: 0f, y: 2500f, z: 0f);

    private readonly Player player;
    private readonly PlayerInput input;

    /// <summary>
    ///     Default player movement, using physics.
    /// </summary>
    /// <param name="player">The player to move.</param>
    /// <param name="input">The player input to use for movement.</param>
    /// <param name="flyingSpeed">The initial flying speed.</param>
    internal DefaultMovement(Player player, PlayerInput input, Double flyingSpeed) : base(flyingSpeed)
    {
        this.player = player;
        this.input = input;

        player.Camera.Transform.SetParent(player.Body.Transform);
        player.Camera.Transform.LocalPosition = (0.0, 0.65, 0.0);
    }

    internal override void Move(Double pitch, Double yaw, Double deltaTime)
    {
        player.Body.Transform.LocalRotation = Quaterniond.FromAxisAngle(Vector3d.UnitY, MathHelper.DegreesToRadians(-yaw));
        player.Head.LocalRotation = Quaterniond.FromAxisAngle(Vector3d.UnitX, MathHelper.DegreesToRadians(pitch));

        if (player.Body.IsEnabled) player.Body.Movement = GetPhysicsBasedMovement();
        else player.Body.Transform.Position += GetFlyingMovement(input, player.Head) * deltaTime;
    }

    private Vector3d GetPhysicsBasedMovement()
    {
        Body body = player.Body;

        Vector3d movement = input.GetMovement(player.Body.Transform, Speed, SprintSpeed, allowFlying: false);

        body.Move(movement, maxForce);

        if (input.ShouldJump == input.ShouldCrouch) return movement;

        if (input.ShouldJump)
        {
            if (body.IsGrounded) body.AddForce(new Vector3d(x: 0, JumpForce, z: 0));
            else if (body.IsSwimming) body.Move(Vector3d.UnitY * SwimSpeed, maxSwimForce);
        }
        else
        {
            if (body.IsSwimming) body.Move(Vector3d.UnitY * -DiveSpeed, maxSwimForce);
        }

        return movement;
    }
}
