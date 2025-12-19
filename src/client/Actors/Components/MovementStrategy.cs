// <copyright file="PlayerMovementStrategy.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors.Components;

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
    /// <param name="deltaTime">The time since the last update cycle.</param>
    internal abstract void Move(Double pitch, Double yaw, Double deltaTime);
}
