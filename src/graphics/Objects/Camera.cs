// <copyright file="Camera.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Runtime.InteropServices.Marshalling;
using OpenTK.Mathematics;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Graphics.Core;
using VoxelGame.Graphics.Definition;
using VoxelGame.Graphics.Graphics;

namespace VoxelGame.Graphics.Objects;

/// <summary>
///     Represents the space camera.
/// </summary>
[NativeMarshalling(typeof(CameraMarshaller))]
public class Camera : NativeObject, IView
{
    private Double fovX = MathHelper.DegreesToRadians(degrees: 90.0);
    private Double fovY;

    private Boolean advancedDataDirty;

    private Double pitch;
    private Double yaw;

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
    ///     Get the up vector of the camera.
    /// </summary>
    public Vector3d Up { get; private set; } = Vector3d.UnitY;

    /// <summary>
    ///     Get or set the camera pitch.
    /// </summary>
    public Double Pitch
    {
        get => MathHelper.RadiansToDegrees(pitch);
        set
        {
            Double angle = MathHelper.Clamp(value, min: -89.0, max: 89.0);
            pitch = MathHelper.DegreesToRadians(angle);
            UpdateVectors();
        }
    }

    /// <summary>
    ///     Get or set the camera yaw.
    /// </summary>
    public Double Yaw
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
    public Double FovX
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
    private Double FovY => MathHelper.RadiansToDegrees(fovY);

    private static Double FarClipping => 10000.0;

    private static Double NearClipping => 0.05;

    /// <summary>
    ///     Gets or sets the camera position.
    /// </summary>
    public Vector3d Position { get; set; }

    /// <summary>
    ///     Get the front vector of the camera.
    /// </summary>
    public Vector3d Forward { get; private set; } = Vector3d.UnitX;

    /// <summary>
    ///     Get the right vector of the camera.
    /// </summary>
    public Vector3d Right { get; private set; } = Vector3d.UnitZ;

    /// <inheritdoc />
    public IView.Parameters Definition => new(fovY, Client.AspectRatio, (NearClipping, FarClipping), Position, (Forward, Up, Right));

    private void OnSizeChanged(Object? sender, SizeChangeEventArgs e)
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
    public Frustum GetPartialFrustum(Double near, Double far)
    {
        return new Frustum(fovY, Client.AspectRatio, (near, far), Position, Forward, Up, Right);
    }

    internal override void PrepareSynchronization()
    {
        const Single maxDistance = 528.0f;

        Vector3d adaptedPosition = new(
            Position.X % maxDistance,
            Position.Y % maxDistance,
            Position.Z % maxDistance);

        Vector3d offset = adaptedPosition - Position;
        Client.Space.SetAdjustment(offset);

        preparedPosition = ((Single) adaptedPosition.X, (Single) adaptedPosition.Y, (Single) adaptedPosition.Z);
    }

    internal override void Synchronize()
    {
        if (advancedDataDirty)
        {
            NativeMethods.UpdateAdvancedCameraData(this,
                new AdvancedCameraData
                {
                    Fov = (Single) FovY,
                    Near = (Single) NearClipping,
                    Far = (Single) FarClipping
                });

            advancedDataDirty = false;
        }

        NativeMethods.UpdateBasicCameraData(this,
            new BasicCameraData
            {
                Position = preparedPosition,
                Front = Forward.ToVector3(),
                Up = Up.ToVector3()
            });
    }

    private void UpdateVectors()
    {
        Vector3d front;

        front.X = Math.Cos(pitch) * Math.Cos(yaw);
        front.Y = Math.Sin(pitch);
        front.Z = Math.Cos(pitch) * Math.Sin(yaw);

        Forward = Vector3d.Normalize(front);

        Right = Vector3d.Normalize(Vector3d.Cross(Forward, Vector3d.UnitY));
        Up = Vector3d.Normalize(Vector3d.Cross(Right, Forward));
    }
}

#pragma warning disable S3242
[CustomMarshaller(typeof(Camera), MarshalMode.ManagedToUnmanagedIn, typeof(CameraMarshaller))]
internal static class CameraMarshaller
{
    internal static IntPtr ConvertToUnmanaged(Camera managed)
    {
        return managed.Self;
    }

    internal static void Free(IntPtr unmanaged)
    {
        // Nothing to do here.
    }
}
#pragma warning restore S3242
