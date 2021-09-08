﻿// <copyright file="Camera.cs" company="VoxelGame">
//     Code from https://github.com/opentk/LearnOpenTK
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenToolkit.Mathematics;
using VoxelGame.Core.Physics;

namespace VoxelGame.Client.Rendering
{
    public class Camera
    {
        private readonly float farClipping = 1000f;
        private readonly float nearClipping = 0.1f;
        private float fov = MathHelper.PiOver2 / 90f * 70f;

        private Vector3 front = Vector3.UnitX;

        private float pitch;
        private float yaw;

        public Camera(Vector3 position)
        {
            Position = position;
        }

        public Vector3 Position { get; set; }

        public Frustum Frustum => new(fov, Screen.AspectRatio, nearClipping, farClipping, Position, front, Up, Right);

        public Vector3 Front => front;
        public Vector3 Up { get; private set; } = Vector3.UnitY;

        public Vector3 Right { get; private set; } = Vector3.UnitZ;

        public float Pitch
        {
            get => MathHelper.RadiansToDegrees(pitch);
            set
            {
                float angle = MathHelper.Clamp(value, -89f, 89f);
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
                float angle = MathHelper.Clamp(value, 1f, 45f);
                fov = MathHelper.DegreesToRadians(angle);
            }
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + Front, Up);
        }

        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(fov, Screen.AspectRatio, nearClipping, farClipping);
        }

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