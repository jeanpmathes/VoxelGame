// <copyright file="Camera.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Runtime.InteropServices.Marshalling;
using OpenTK.Mathematics;
using VoxelGame.Core.Physics;
using VoxelGame.Graphics.Core;
using VoxelGame.Graphics.Definition;

namespace VoxelGame.Graphics.Objects;

/// <summary>
///     Represents the space camera.
/// </summary>
[NativeMarshalling(typeof(CameraMarshaller))]
public class Camera : NativeObject
{
    private Boolean advancedDataDirty;

    private Double fovX = MathHelper.DegreesToRadians(degrees: 90.0);
    private Double fovY;

    private Vector3 preparedPosition = Vector3.Zero;

    /// <summary>
    ///     Create a new camera.
    /// </summary>
    public Camera(IntPtr nativePointer, Space space) : base(nativePointer, space.Client)
    {
        space.Client.SizeChanged += OnSizeChanged;

        RecalculateFovY();
    }

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

    /// <summary>
    ///     Get the up vector of the camera.
    /// </summary>
    public Vector3d Up { get; private set; } = Vector3d.UnitY;

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
    /// The definition of the camera view.
    /// </summary>
    public Parameters Definition => new(fovY, Client.AspectRatio, (NearClipping, FarClipping), Position, (Forward, Up, Right));

    /// <summary>
    /// Set the orientation of the camera, defined by the forward, right and up vectors.
    /// </summary>
    /// <param name="forward">The forward vector.</param>
    /// <param name="right">The right vector.</param>
    /// <param name="up">The up vector.</param>
    public void SetOrientation(Vector3d forward, Vector3d right, Vector3d up)
    {
        Forward = Vector3d.Normalize(forward);
        Right = Vector3d.Normalize(right);
        Up = Vector3d.Normalize(up);
    }

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
                Front = (Vector3) Forward,
                Up = (Vector3) Up
            });
    }

    /// <summary>
    ///     Get the parameters that define the view.
    /// </summary>
    public record Parameters(Double FieldOfView, Double AspectRatio, (Double near, Double far) Clipping, Vector3d Position, (Vector3d front, Vector3d up, Vector3d right) Orientation)
    {
        /// <summary>
        ///     Create a frustum from the view parameters.
        /// </summary>
        public Frustum Frustum => new(FieldOfView, AspectRatio, Clipping, Position, Orientation.front, Orientation.up, Orientation.right);
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
