// <copyright file="PlayerMovementStrategy.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;

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
    ///     Get the flying movement for a given orientable.
    /// </summary>
    protected Vector3d GetFlyingMovement(PlayerInput input, IOrientable orientable)
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
    /// <returns>The new camera position.</returns>
    internal abstract Vector3d GetCameraPosition();

    /// <summary>
    ///     Apply the calculated movement to the player or camera.
    ///     This method is allowed to directly modify positions, but to use physics it should return a vector.
    ///     Should be called once (or less) per update cycle.
    /// </summary>
    /// <param name="deltaTime">The time since the last update cycle.</param>
    /// <returns>
    ///     The target movement that is attempted to achieve using physics.
    ///     If no physics are used, this will be zero.
    /// </returns>
    internal abstract Vector3d ApplyMovement(Double deltaTime);
}
