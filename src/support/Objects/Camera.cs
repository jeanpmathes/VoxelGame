// <copyright file="Camera.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Support.Core;
using VoxelGame.Support.Definition;
using VoxelGame.Support.Graphics;

namespace VoxelGame.Support.Objects;

/// <summary>
///     Represents the space camera.
/// </summary>
public class Camera : NativeObject, IView
{
    private bool advancedDataDirty;

    private double fov = MathHelper.DegreesToRadians(degrees: 70.0);

    private Vector3d front = Vector3d.UnitX;

    private double pitch;
    private Vector3 preparedPosition = Vector3.Zero;
    private double yaw;

    /// <summary>
    ///     Create a new camera.
    /// </summary>
    public Camera(IntPtr nativePointer, Space space) : base(nativePointer, space.Client)
    {
        advancedDataDirty = true;
    }

    /// <summary>
    ///     Gets or sets the camera position.
    /// </summary>
    public Vector3d Position { get; set; }

    /// <summary>
    ///     Get the front vector of the camera.
    /// </summary>
    public Vector3d Front => front;

    /// <summary>
    ///     Get the up vector of the camera.
    /// </summary>
    public Vector3d Up { get; private set; } = Vector3d.UnitY;

    /// <summary>
    ///     Get the right vector of the camera.
    /// </summary>
    public Vector3d Right { get; private set; } = Vector3d.UnitZ;

    /// <summary>
    ///     Get or set the camera pitch.
    /// </summary>
    public double Pitch
    {
        get => MathHelper.RadiansToDegrees(pitch);
        set
        {
            double angle = MathHelper.Clamp(value, min: -89.0, max: 89.0);
            pitch = MathHelper.DegreesToRadians(angle);
            UpdateVectors();
        }
    }

    /// <summary>
    ///     Get or set the camera yaw.
    /// </summary>
    public double Yaw
    {
        get => MathHelper.RadiansToDegrees(yaw);
        set
        {
            yaw = MathHelper.DegreesToRadians(value);
            yaw %= 360.0;

            UpdateVectors();
        }
    }

    /// <summary>
    ///     Get or set the field of view, in degrees.
    /// </summary>
    public double Fov
    {
        get => MathHelper.RadiansToDegrees(fov);
        set
        {
            double angle = MathHelper.Clamp(value, min: 1.0, max: 90.0);
            fov = MathHelper.DegreesToRadians(angle);
            advancedDataDirty = true;
        }
    }

    /// <inheritdoc />
    public double FarClipping => 500.0; // todo: use this distance for ray length, think about whether this makes sense

    /// <inheritdoc />
    public double NearClipping => 0.1;

    /// <inheritdoc />
    public Frustum Frustum => new(fov, Client.AspectRatio, (NearClipping, FarClipping), Position, front, Up, Right);

    /// <inheritdoc />
    public Matrix4d ViewMatrix => Matrix4d.LookAt(Position, Position + Front, Up);

    /// <inheritdoc />
    public Matrix4d ProjectionMatrix => Matrix4d.CreatePerspectiveFieldOfView(
        fov,
        Client.AspectRatio,
        NearClipping,
        FarClipping);

    /// <summary>
    ///     Get a partial frustum, which is the view frustum with changed near and far clipping planes.
    /// </summary>
    /// <param name="near">The near clipping plane.</param>
    /// <param name="far">The far clipping plane.</param>
    /// <returns>The partial frustum.</returns>
    public Frustum GetPartialFrustum(double near, double far)
    {
        return new Frustum(fov, Client.AspectRatio, (near, far), Position, front, Up, Right);
    }

    /// <summary>
    ///     Get the camera's frustum dimensions at a given distance.
    /// </summary>
    /// <param name="distance">The distance.</param>
    /// <returns>The width and height.</returns>
    public (double width, double height) GetDimensionsAt(double distance)
    {
        return Frustum.GetDimensionsAt(distance, fov, Client.AspectRatio);
    }

    internal override void PrepareSynchronization()
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

    internal override void Synchronize()
    {
        if (advancedDataDirty)
        {
            Native.UpdateAdvancedCameraData(this,
                new AdvancedCameraData
                {
                    Fov = (float) Fov,
                    Near = (float) NearClipping,
                    Far = (float) FarClipping
                });

            advancedDataDirty = false;
        }

        Native.UpdateBasicCameraData(this,
            new BasicCameraData
            {
                Position = preparedPosition,
                Front = Front.ToVector3(),
                Up = Up.ToVector3()
            });
    }

    private void UpdateVectors()
    {
        front.X = Math.Cos(pitch) * Math.Cos(yaw);
        front.Y = Math.Sin(pitch);
        front.Z = Math.Cos(pitch) * Math.Sin(yaw);

        front = Vector3d.Normalize(Front);

        Right = Vector3d.Normalize(Vector3d.Cross(Front, Vector3d.UnitY));
        Up = Vector3d.Normalize(Vector3d.Cross(Right, Front));
    }
}
