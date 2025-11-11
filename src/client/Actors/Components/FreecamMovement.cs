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
/// <param name="player">The player to which this movement strategy belongs.</param>
/// <param name="input">The player input to use for movement.</param>
/// <param name="flyingSpeed">The initial flying speed.</param>
internal sealed class FreecamMovement(Player player, PlayerInput input, Double flyingSpeed) : MovementStrategy(flyingSpeed)
{
    private Vector3d cameraPosition = player.Head.Position;

    /// <inheritdoc />
    internal override Vector3d GetCameraPosition()
    {
        return cameraPosition;
    }

    /// <inheritdoc />
    internal override Vector3d ApplyMovement(Double deltaTime)
    {
        cameraPosition += GetFlyingMovement(input, player.Head) * deltaTime;

        return Vector3d.Zero;
    }
}
