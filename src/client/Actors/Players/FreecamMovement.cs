// <copyright file="FreecamMovement.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;

namespace VoxelGame.Client.Actors.Players;

/// <summary>
///     Movement strategy that moves only the camera, keeping the player in place.
/// </summary>
/// <param name="input">The input to use for movement.</param>
/// <param name="flyingSpeed">The initial flying speed.</param>
internal class FreecamMovement(PhysicsActor actor, Input input, Double flyingSpeed) : MovementStrategy(flyingSpeed)
{
    private Vector3d cameraPosition = actor.Head.Position;

    /// <inheritdoc />
    internal override Vector3d GetCameraPosition(IOrientable head)
    {
        return cameraPosition;
    }

    /// <inheritdoc />
    internal override Vector3d ApplyMovement(PhysicsActor actor, Double deltaTime)
    {
        cameraPosition += GetFlyingMovement(input, actor.Head) * deltaTime;

        return Vector3d.Zero;
    }
}
