// <copyright file="Camera.cs" company="VoxelGame">
//     Code from https://github.com/opentk/LearnOpenTK
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Physics;

namespace VoxelGame.Client.Rendering;

/// <summary>
///     Represents a camera in the game.
/// </summary>
public class Camera : IView
{
    private double fov = MathHelper.DegreesToRadians(degrees: 70.0);

    private Vector3d front = Vector3d.UnitX;

    private double pitch;
    private double yaw;

    /// <summary>
    ///     Create a new camera.
    /// </summary>
    /// <param name="position">The initial position of the camera.</param>
    public Camera(Vector3d position)
    {
        Position = position;
    }

    /// <summary>
    ///     Get or set the position of the camera.
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
    ///     Get or set the field of view.
    /// </summary>
    public double Fov
    {
        get => MathHelper.RadiansToDegrees(fov);
        set
        {
            double angle = MathHelper.Clamp(value, min: 1.0, max: 45.0);
            fov = MathHelper.DegreesToRadians(angle);
        }
    }

    /// <inheritdoc />
    public double FarClipping => 500.0;

    /// <inheritdoc />
    public double NearClipping => 0.1;

    /// <inheritdoc />
    public Frustum Frustum => new(fov, Screen.AspectRatio, (NearClipping, FarClipping), Position, front, Up, Right);

    /// <inheritdoc />
    public Matrix4d ViewMatrix => Matrix4d.LookAt(Position, Position + Front, Up);

    /// <inheritdoc />
    public Matrix4d ProjectionMatrix => Matrix4d.CreatePerspectiveFieldOfView(
        fov,
        Screen.AspectRatio,
        NearClipping,
        FarClipping);

    private void UpdateVectors()
    {
        front.X = Math.Cos(pitch) * Math.Cos(yaw);
        front.Y = Math.Sin(pitch);
        front.Z = Math.Cos(pitch) * Math.Sin(yaw);

        front = Vector3d.Normalize(Front);

        Right = Vector3d.Normalize(Vector3d.Cross(Front, Vector3d.UnitY));
        Up = Vector3d.Normalize(Vector3d.Cross(Right, Front));
    }

    /// <summary>
    ///     Get the camera's frustum dimensions at a given distance.
    /// </summary>
    /// <param name="distance">The distance.</param>
    /// <returns>The width and height.</returns>
    public (double width, double height) GetDimensionsAt(double distance)
    {
        return Frustum.GetDimensionsAt(distance, fov, Screen.AspectRatio);
    }
}
