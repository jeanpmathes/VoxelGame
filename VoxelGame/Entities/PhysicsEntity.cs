// <copyright file="PhysicsEntity.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenTK;
using System;
using System.Collections.Generic;
using VoxelGame.Physics;
using VoxelGame.Utilities;

namespace VoxelGame.Entities
{
    /// <summary>
    /// An entity which is affected by gravity and forces.
    /// </summary>
    public abstract class PhysicsEntity : IDisposable
    {
        /// <summary>
        /// The gravitational constant which accelerates all physics entities.
        /// </summary>
        public const float Gravity = -9.81f;

        /// <summary>
        /// Gets the mass of this physics entity.
        /// </summary>
        public float Mass { get; }

        /// <summary>
        /// Gets the drag affecting the velocity of this physics entity.
        /// </summary>
        public float Drag { get; }

        /// <summary>
        /// Gets or sets the velocity of the physics entity.
        /// </summary>
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

        public abstract Vector3 Movement { get; }
        public abstract Vector3 LookingDirection { get; }
        public abstract Logic.BlockSide TargetSide { get; }

        private readonly int physicsIterations = 10;

        private Vector3 force;
        private BoundingBox boundingBox;

        public BoundingBox BoundingBox
        {
            get => boundingBox;
        }

        protected PhysicsEntity(float mass, float drag, BoundingBox boundingBox)
        {
            Rotation = Quaternion.Identity;

            Mass = mass;
            Drag = drag;
            this.boundingBox = boundingBox;

            boundingBox.Center = Position;
        }

        /// <summary>
        /// Applies force to this entity.
        /// </summary>
        /// <param name="force">The force to apply.</param>
        public void AddForce(Vector3 force)
        {
            this.force += force;
        }

        /// <summary>
        /// Tries to move the entity in a certain direction using forces, but never using more force than specified.
        /// </summary>
        /// <param name="movement">The target movement, can be null to try to stop moving.</param>
        /// <param name="maxForce">The maximum allowed force to use.</param>
        public void Move(Vector3 movement, Vector3 maxForce)
        {
            maxForce = maxForce.Absolute();

            Vector3 requiredForce = (movement - Velocity) * Mass;
            AddForce(VMath.ClampComponents(requiredForce, -maxForce, maxForce));
        }

        public abstract void Render();

        public void Tick(float deltaTime)
        {
            IsGrounded = false;

            //force -= Velocity.Sign() * Drag * Velocity.LengthSquared * deltaTime;
            force -= Velocity.Sign() * (Velocity * Velocity) * Drag;
            Velocity += force / Mass * deltaTime;

            Vector3 movement = Velocity * deltaTime;
            movement *= 1f / physicsIterations;

            for (int i = 0; i < physicsIterations; i++)
            {
                boundingBox.Center += movement;
                if (BoundingBox.IntersectsTerrain(out bool xCollision, out bool yCollision, out bool zCollision, out List<(int x, int y, int z, Logic.Block block)> intersections))
                {
                    if (yCollision)
                    {
                        int xPos = (int)Math.Floor(BoundingBox.Center.X);
                        int yPos = (int)Math.Floor(BoundingBox.Center.Y);
                        int zPos = (int)Math.Floor(BoundingBox.Center.Z);

                        IsGrounded = !Game.World.GetBlock(xPos, yPos + (int)Math.Round(BoundingBox.Extents.Y), zPos, out _)?.IsSolid ?? true;
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

                for (int j = 0; j < intersections.Count; j++)
                {
                    if (intersections[j].block.RecieveCollisions)
                    {
                        intersections[j].block.OnCollision(this, intersections[j].x, intersections[j].y, intersections[j].z);
                    }
                }

                Position += movement;
            }

            boundingBox.Center = Position;

            force = new Vector3(0f, Gravity * Mass, 0f);

            Update(deltaTime);
        }

        #region IDisposable Support

        protected abstract void Update(float deltaTime);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~PhysicsEntity()
        {
            Dispose(false);
        }

        protected abstract void Dispose(bool disposing);

        #endregion IDisposable Support
    }
}