// <copyright file="Camera.cs" company="VoxelGame">
//     All rights reserved.
// </copyright>
// <author>pershingthesecond</author>
using OpenTK;
using System;

namespace VoxelGame.Rendering
{
    public class Camera
    {
        private Vector3 front = -Vector3.UnitZ;
        private Vector3 up = Vector3.UnitY;
        private Vector3 right = Vector3.UnitX;

        private float pitch;
        private float yaw = -MathHelper.PiOver2;
        private float fov = MathHelper.PiOver2;

        public Camera(Vector3 position, float aspectRatio)
        {
            Position = position;
            AspectRatio = aspectRatio;
        }

        public Vector3 Position { get; set; }
        public float AspectRatio { private get; set; }

        public Vector3 Front => front;
        public Vector3 Up => up;
        public Vector3 Right => right;

        public float Pitch
        {
            get => MathHelper.RadiansToDegrees(pitch);
            set
            {
                var angle = MathHelper.Clamp(value, -89f, 89f);
                pitch = MathHelper.DegreesToRadians(angle);
                UpdateVectors();
            }
        }

        public float Yaw
        {
            get => MathHelper.RadiansToDegrees(yaw);
            set
            {
                yaw = MathHelper.DegreesToRadians(value);
                UpdateVectors();
            }
        }

        public float Fov
        {
            get => MathHelper.RadiansToDegrees(fov);
            set
            {
                var angle = MathHelper.Clamp(value, 1f, 45f);
                fov = MathHelper.DegreesToRadians(angle);
            }
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + Front, Up);
        }

        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(fov, AspectRatio, 0.01f, 1000f);
        }

        private void UpdateVectors()
        {
            front.X = (float)Math.Cos(pitch) * (float)Math.Cos(yaw);
            front.Y = (float)Math.Sin(pitch);
            front.Z = (float)Math.Cos(pitch) * (float)Math.Sin(yaw);

            front = Vector3.Normalize(Front);

            right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
            up = Vector3.Normalize(Vector3.Cross(Right, Front));
        }
    }
}