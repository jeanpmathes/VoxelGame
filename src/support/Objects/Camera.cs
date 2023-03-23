// <copyright file="Camera.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Support.Definition;

namespace VoxelGame.Support.Objects;

/// <summary>
///     Represents the space camera.
/// </summary>
public class Camera : NativeObject
{
    private Vector3 preparedPosition = Vector3.Zero;

    /// <summary>
    ///     Create a new camera.
    /// </summary>
    public Camera(IntPtr nativePointer, Space space) : base(nativePointer, space.Client) {}

    /// <summary>
    ///     Gets or sets the camera position.
    /// </summary>
    public Vector3d Position { get; set; }

    /// <inheritdoc />
    public override void PrepareSynchronization()
    {
        const float maxDistance = 1000.0f;

        Vector3d adaptedPosition = new(
            Position.X % maxDistance,
            Position.Y % maxDistance,
            Position.Z % maxDistance);

        Vector3d offset = adaptedPosition - Position;
        Client.Space.SetAdjustment(offset);

        preparedPosition = ((float) adaptedPosition.X, (float) adaptedPosition.Y, (float) adaptedPosition.Z);
    }

    /// <inheritdoc />
    public override void Synchronize()
    {
        Native.UpdateCameraData(this,
            new CameraData
            {
                Position = preparedPosition
            });
    }
}

