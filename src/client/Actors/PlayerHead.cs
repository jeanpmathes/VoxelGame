// <copyright file="PlayerHead.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Actors;

namespace VoxelGame.Client.Actors;

/// <summary>
///     The head of a player. Has the same orientation as the camera, but can have a different position.
/// </summary>
public class PlayerHead(IOrientable camera, IOrientable body) : IOrientable
{
    private readonly Vector3d headOffset = new(x: 0f, y: 0.65f, z: 0f);

    /// <inheritdoc />
    public Vector3d Forward => camera.Forward;

    /// <inheritdoc />
    public Vector3d Right => camera.Right;

    /// <inheritdoc />
    public Vector3d Position => body.Position + headOffset;
}
