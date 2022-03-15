// <copyright file="Camera.cs" company="VoxelGame">
//     Code from https://github.com/opentk/LearnOpenTK
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Physics;

namespace VoxelGame.Client.Rendering
{
    /// <summary>
    ///     Represents a camera in the game.
    /// </summary>
    public class Camera
    {
        private readonly float farClipping = 1000f;
        private readonly float nearClipping = 0.1f;
        private float fov = MathHelper.PiOver2 / 90f * 70f;

        private Vector3 front = Vector3.UnitX;

        private float pitch;
        private float yaw;

        /// <summary>
        ///     Create a new camera.
        /// </summary>
        /// <param name="position">The initial position of the camera.</param>
        public Camera(Vector3 position)
        {
            Position = position;
        }

        /// <summary>
        ///     Get or set the position of the camera.
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        ///     Get the view frustum of the camera.
        /// </summary>
        public Frustum Frustum => new(fov, Screen.AspectRatio, (nearClipping, farClipping), Position, front, Up, Right);

        /// <summary>
        ///     Get the front vector of the camera.
        /// </summary>
        public Vector3 Front => front;

        /// <summary>
        ///     Get the up vector of the camera.
        /// </summary>
        public Vector3 Up { get; private set; } = Vector3.UnitY;

        /// <summary>
        ///     Get the right vector of the camera.
        /// </summary>
        public Vector3 Right { get; private set; } = Vector3.UnitZ;

        /// <summary>
        ///     Get or set the camera pitch.
        /// </summary>
        public float Pitch
        {
            get => MathHelper.RadiansToDegrees(pitch);
            set
            {
                float angle = MathHelper.Clamp(value, min: -89f, max: 89f);
                pitch = MathHelper.DegreesToRadians(angle);
                UpdateVectors();
            }
        }

        /// <summary>
        ///     Get or set the camera yaw.
        /// </summary>
        public float Yaw
        {
            get => MathHelper.RadiansToDegrees(yaw);
            set
            {
                yaw = MathHelper.DegreesToRadians(value);
                UpdateVectors();
            }
        }

        /// <summary>
        ///     Get or set the field of view.
        /// </summary>
        public float Fov
        {
            get => MathHelper.RadiansToDegrees(fov);
            set
            {
                float angle = MathHelper.Clamp(value, min: 1f, max: 45f);
                fov = MathHelper.DegreesToRadians(angle);
            }
        }

        /// <summary>
        ///     Get the camera's view matrix.
        /// </summary>
        public Matrix4 ViewMatrix => Matrix4.LookAt(Position, Position + Front, Up);

        /// <summary>
        ///     Get the camera's projection matrix.
        /// </summary>
        public Matrix4 ProjectionMatrix => Matrix4.CreatePerspectiveFieldOfView(
            fov,
            Screen.AspectRatio,
            nearClipping,
            farClipping);

        private void UpdateVectors()
        {
            front.X = (float) Math.Cos(pitch) * (float) Math.Cos(yaw);
            front.Y = (float) Math.Sin(pitch);
            front.Z = (float) Math.Cos(pitch) * (float) Math.Sin(yaw);

            front = Vector3.Normalize(Front);

            Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
            Up = Vector3.Normalize(Vector3.Cross(Right, Front));
        }
    }
}
