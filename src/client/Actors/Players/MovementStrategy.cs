// <copyright file="PlayerMovementStrategy.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;

namespace VoxelGame.Client.Actors.Players;

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
    ///     Get the flying movement for a given orientable.
    /// </summary>
    protected Vector3d GetFlyingMovement(Input input, IOrientable orientable)
    {
        return input.GetMovement(
            orientable,
            FlyingSpeed * FlyingSpeedFactor,
            FlyingSpeed * FlyingSprintSpeedFactor,
            allowFlying: true);
    }

    /// <summary>
    ///     Determine the camera position based on the player head.
    /// </summary>
    /// <param name="head">The head of the player.</param>
    /// <returns>The new camera position.</returns>
    internal abstract Vector3d GetCameraPosition(IOrientable head);

    /// <summary>
    ///     Apply the calculated movement to a physics actor.
    ///     Should be called once (or less) per update cycle.
    /// </summary>
    /// <param name="actor">The actor to apply the movement to, e.g. the player.</param>
    /// <param name="deltaTime">The time since the last update cycle.</param>
    /// <returns>
    ///     The target movement that is attempted to achieve using physics.
    ///     If no physics are used, this will be zero.
    /// </returns>
    internal abstract Vector3d ApplyMovement(PhysicsActor actor, Double deltaTime);
}
