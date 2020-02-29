// <copyright file="PhysicsEntity.cs" company="VoxelGame">
//     All rights reserved.
// </copyright>
// <author>pershingthesecond</author>
using OpenTK;
using System;

using VoxelGame.Physics;

namespace VoxelGame.Entities
{
    public abstract class PhysicsEntity
    {
        public const float Gravity = -20f;

        public float Mass { get; }
        public float Drag { get; }

        public Vector3 Velocity { get; set; }
        public Vector3 Position { get; set; }

        public Quaternion Rotation { get; set; }

        public Vector3 Forward
        {
            get
            {
                return Rotation * Vector3.UnitX;
            }
        }
        public Vector3 Right
        {
            get
            {
                return Rotation * Vector3.UnitZ;
            }
        }

        private BoundingBox boundingBox;
        private Vector3 force;

        public PhysicsEntity(float mass, float drag, BoundingBox boundingBox)
        {
            Rotation = Quaternion.Identity;

            Mass = mass;
            Drag = drag;
            this.boundingBox = boundingBox;

            boundingBox.Center = Position;
        }

        public void AddForce(Vector3 force)
        {
            this.force += force;
        }

        public void Tick(float deltaTime)
        {
            Velocity -= Velocity * Drag * Velocity.Length * deltaTime;
            Velocity += force / Mass * deltaTime;

            boundingBox.Center += Velocity;
            if (boundingBox.IntersectsTerrain(out bool xCollision, out bool yCollision, out bool zCollision))
            {
                Velocity = new Vector3(
                    xCollision ? 0f : Velocity.X,
                    yCollision ? 0f : Velocity.Y,
                    zCollision ? 0f : Velocity.Z);
            }

            Position += Velocity;
            boundingBox.Center = Position;

            force = new Vector3(0f, Gravity, 0f);

            Update();
        }

        protected abstract void Update();
    }
}