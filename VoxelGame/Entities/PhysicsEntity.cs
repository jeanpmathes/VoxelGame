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

        public bool IsGrounded { get; private set; }

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

        private readonly int physicsIterations = 10;

        private BoundingBox boundingBox;
        private Vector3 force;

        private bool addMovement = false;
        private Vector3 additionalMovement;

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

        public void Move(Vector3 movement)
        {
            additionalMovement += movement;
            addMovement = true;
        }

        public abstract void Render();

        public void Tick(float deltaTime)
        {
            IsGrounded = false;

            Velocity -= Velocity * Drag * Velocity.Length * deltaTime;
            Velocity += force / Mass * deltaTime;

            Vector3 movement = Velocity;

            if (addMovement)
            {
                movement += additionalMovement * deltaTime;
            }

            movement *= 1f / physicsIterations;

            for (int i = 0; i < physicsIterations; i++)
            {
                boundingBox.Center += movement;
                if (boundingBox.IntersectsTerrain(out bool xCollision, out bool yCollision, out bool zCollision))
                {
                    if (yCollision)
                    {
                        int xPos = (int)Math.Floor(boundingBox.Center.X);
                        int yPos = (int)Math.Floor(boundingBox.Center.Y);
                        int zPos = (int)Math.Floor(boundingBox.Center.Z);

                        IsGrounded = !Game.World.GetBlock(xPos, yPos + (int)Math.Round(boundingBox.Extents.Y), zPos)?.IsSolid ?? true;
                    }

                    movement = new Vector3(
                        xCollision ? 0f : movement.X,
                        yCollision ? 0f : movement.Y,
                        zCollision ? 0f : movement.Z);

                    Velocity = new Vector3(
                        xCollision ? 0f : Velocity.X,
                        yCollision ? 0f : Velocity.Y,
                        zCollision ? 0f : Velocity.Z);
                }

                Position += movement;
            }

            boundingBox.Center = Position;

            force = new Vector3(0f, Gravity, 0f);

            addMovement = false;
            additionalMovement = Vector3.Zero;

            Update();
        }

        protected abstract void Update();
    }
}