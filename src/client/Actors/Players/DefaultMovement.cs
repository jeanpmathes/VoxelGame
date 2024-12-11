// <copyright file="DefaultMovementStrategy.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;

namespace VoxelGame.Client.Actors.Players;

/// <summary>
///     Default player movement, using physics.
/// </summary>
/// <param name="input">The input to use for movement.</param>
/// <param name="flyingSpeed">The initial flying speed.</param>
internal class DefaultMovement(Input input, Double flyingSpeed) : MovementStrategy(flyingSpeed)
{
    private const Single DiveSpeed = 8f;
    private const Single JumpForce = 25000f;
    private const Single Speed = 4f;
    private const Single SprintSpeed = 6f;
    private const Single SwimSpeed = 4f;

    private readonly Vector3d maxForce = new(x: 500f, y: 0f, z: 500f);
    private readonly Vector3d maxSwimForce = new(x: 0f, y: 2500f, z: 0f);

    /// <inheritdoc />
    internal override Vector3d GetCameraPosition(IOrientable head)
    {
        return head.Position;
    }

    internal override Vector3d ApplyMovement(PhysicsActor actor, Double deltaTime)
    {
        Vector3d movement = Vector3d.Zero;

        if (actor.DoPhysics) movement = ApplyPhysicsBasedMovement(actor);
        else actor.Position += GetFlyingMovement(input, actor.Head) * deltaTime;

        return movement;
    }

    private Vector3d ApplyPhysicsBasedMovement(PhysicsActor actor)
    {
        Vector3d movement = input.GetMovement(actor, Speed, SprintSpeed, allowFlying: false);

        actor.Move(movement, maxForce);

        if (input.ShouldJump == input.ShouldCrouch) return movement;

        if (input.ShouldJump)
        {
            if (actor.IsGrounded) actor.AddForce(new Vector3d(x: 0, JumpForce, z: 0));
            else if (actor.IsSwimming) actor.Move(Vector3d.UnitY * SwimSpeed, maxSwimForce);
        }
        else
        {
            if (actor.IsSwimming) actor.Move(Vector3d.UnitY * -DiveSpeed, maxSwimForce);
        }

        return movement;
    }
}
