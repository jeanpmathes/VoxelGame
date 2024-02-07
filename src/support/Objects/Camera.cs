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
    private double fovX = MathHelper.DegreesToRadians(degrees: 90.0);
    private double fovY;

    private bool advancedDataDirty;

    private double pitch;
    private double yaw;

    private Vector3 preparedPosition = Vector3.Zero;

    /// <summary>
    ///     Create a new camera.
    /// </summary>
    public Camera(IntPtr nativePointer, Space space) : base(nativePointer, space.Client)
    {
        space.Client.OnSizeChange += OnSizeChanged;

        RecalculateFovY();
    }

    /// <summary>
    ///     Gets or sets the camera position.
    /// </summary>
    public Vector3d Position { get; set; }

    /// <summary>
    ///     Get the front vector of the camera.
    /// </summary>
    public Vector3d Front { get; private set; } = Vector3d.UnitX;

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
    ///     Set the horizontal (X) field of view, in degrees.
    /// </summary>
    public double FovX
    {
        get => MathHelper.RadiansToDegrees(fovX);

        set
        {
            fovX = MathHelper.DegreesToRadians(value);
            RecalculateFovY();
        }
    }

    /// <summary>
    ///     Get the vertical (Y) field of view, in degrees.
    /// </summary>
    private double FovY => MathHelper.RadiansToDegrees(fovY);

    /// <inheritdoc />
    public double FarClipping => 1000.0;

    /// <inheritdoc />
    public double NearClipping => 0.05;

    /// <inheritdoc />
    public Frustum Frustum => new(fovY, Client.AspectRatio, (NearClipping, FarClipping), Position, Front, Up, Right);

    /// <inheritdoc />
    public Matrix4d ViewMatrix => Matrix4d.LookAt(Position, Position + Front, Up);

    /// <inheritdoc />
    public Matrix4d ProjectionMatrix => Matrix4d.CreatePerspectiveFieldOfView(
        fovY,
        Client.AspectRatio,
        NearClipping,
        FarClipping).With(matrix =>
    {
        matrix[rowIndex: 3, columnIndex: 2] *= 0.5f;
    });

    private void OnSizeChanged(object? sender, SizeChangeEventArgs e)
    {
        RecalculateFovY();
        Synchronize();
    }

    private void RecalculateFovY()
    {
        fovY = 2.0 * Math.Atan(Math.Tan(fovX / 2.0) / Client.AspectRatio);
        advancedDataDirty = true;
    }

    /// <summary>
    ///     Get a partial frustum, which is the view frustum with changed near and far clipping planes.
    /// </summary>
    /// <param name="near">The near clipping plane.</param>
    /// <param name="far">The far clipping plane.</param>
    /// <returns>The partial frustum.</returns>
    public Frustum GetPartialFrustum(double near, double far)
    {
        return new Frustum(fovY, Client.AspectRatio, (near, far), Position, Front, Up, Right);
    }

    internal override void PrepareSynchronization()
    {
        const float maxDistance = 500.0f;

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
                    Fov = (float) FovY,
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
        Vector3d front;

        front.X = Math.Cos(pitch) * Math.Cos(yaw);
        front.Y = Math.Sin(pitch);
        front.Z = Math.Cos(pitch) * Math.Sin(yaw);

        Front = Vector3d.Normalize(front);

        Right = Vector3d.Normalize(Vector3d.Cross(Front, Vector3d.UnitY));
        Up = Vector3d.Normalize(Vector3d.Cross(Right, Front));
    }
}
