// <copyright file="DefaultMovementStrategy.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Client.Actors.Players;
using VoxelGame.Core.Actors.Components;

namespace VoxelGame.Client.Actors.Components;

/// <summary>
///     Default player movement, using physics.
/// </summary>
/// <param name="player">The player to move.</param>
/// <param name="flyingSpeed">The initial flying speed.</param>
internal class DefaultMovement(Player player, Double flyingSpeed) : MovementStrategy(flyingSpeed)
{
    private const Single DiveSpeed = 8f;
    private const Single JumpForce = 25000f;
    private const Single Speed = 4f;
    private const Single SprintSpeed = 6f;
    private const Single SwimSpeed = 4f;

    private readonly Vector3d maxForce = new(x: 500f, y: 0f, z: 500f);
    private readonly Vector3d maxSwimForce = new(x: 0f, y: 2500f, z: 0f);

    /// <inheritdoc />
    internal override Vector3d GetCameraPosition()
    {
        return player.Head.Position;
    }

    internal override Vector3d ApplyMovement(Double deltaTime)
    {
        Vector3d movement = Vector3d.Zero;

        if (player.Body.IsEnabled) movement = GetPhysicsBasedMovement();
        else player.Body.Transform.Position += GetFlyingMovement(player.Input, player.Head) * deltaTime;

        return movement;
    }

    private Vector3d GetPhysicsBasedMovement()
    {
        Input input = player.Input;
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
