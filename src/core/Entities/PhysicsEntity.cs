// <copyright file="PhysicsEntity.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using OpenToolkit.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Entities
{
    /// <summary>
    ///     An entity which is affected by gravity and forces.
    /// </summary>
    public abstract class PhysicsEntity : IDisposable
    {
        /// <summary>
        ///     The gravitational constant which accelerates all physics entities.
        /// </summary>
        public const float Gravity = -9.81f;

        private readonly int physicsIterations = 10;
        private BoundingBox boundingBox;

        private Vector3 force;

        /// <summary>
        ///     Create a new physics entity.
        /// </summary>
        /// <param name="world">The world in which the physics entity is located.</param>
        /// <param name="mass">The mass of the entity.</param>
        /// <param name="drag">The drag affecting the entity.</param>
        /// <param name="boundingBox">The bounding box of the entity.</param>
        protected PhysicsEntity(World world, float mass, float drag, BoundingBox boundingBox)
        {
            World = world;

            Rotation = Quaternion.Identity;

            Mass = mass;
            Drag = drag;
            this.boundingBox = boundingBox;

            boundingBox.Center = Position;
        }

        /// <summary>
        ///     Gets the mass of this physics entity.
        /// </summary>
        public float Mass { get; }

        /// <summary>
        ///     Gets the drag affecting the velocity of this physics entity.
        /// </summary>
        public float Drag { get; }

        /// <summary>
        ///     Gets or sets the velocity of the physics entity.
        /// </summary>
        public Vector3 Velocity { get; set; }

        /// <summary>
        ///     Get the position of the physics entity.
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        ///     Get the rotation of the physics entity.
        /// </summary>
        public Quaternion Rotation { get; set; }

        /// <summary>
        ///     Get whether the physics entity touches the ground.
        /// </summary>
        public bool IsGrounded { get; private set; }

        /// <summary>
        ///     Get whether the physics entity is in a liquid.
        /// </summary>
        public bool IsSwimming { get; private set; }

        /// <summary>
        ///     Get the forward vector of the physics entity.
        /// </summary>
        public Vector3 Forward => Rotation * Vector3.UnitX;

        /// <summary>
        ///     Get the right vector of the physics entity.
        /// </summary>
        public Vector3 Right => Rotation * Vector3.UnitZ;

        /// <summary>
        ///     Get the world in which the physics entity is located.
        /// </summary>
        public World World { get; }

        /// <summary>
        ///     Get the target movement of the physics entity.
        /// </summary>
        public abstract Vector3 Movement { get; }

        /// <summary>
        ///     Get the lookin direction of the physics entity.
        /// </summary>
        public abstract Vector3 LookingDirection { get; }

        /// <summary>
        ///     Get the block side targeted by the physics entity.
        /// </summary>
        public abstract BlockSide TargetSide { get; }

        /// <summary>
        ///     Get the block position targeted by the physics entity.
        /// </summary>
        public abstract Vector3i TargetPosition { get; }

        /// <summary>
        ///     Get the bounding box of the physics entity.
        /// </summary>
        public BoundingBox BoundingBox => boundingBox;

        /// <summary>
        ///     Applies force to this entity.
        /// </summary>
        /// <param name="additionalForce">The force to apply.</param>
        public void AddForce(Vector3 additionalForce)
        {
            force += additionalForce;
        }

        /// <summary>
        ///     Tries to move the entity in a certain direction using forces, but never using more force than specified.
        /// </summary>
        /// <param name="movement">The target movement, can be zero to try to stop moving.</param>
        /// <param name="maxForce">The maximum allowed force to use.</param>
        public void Move(Vector3 movement, Vector3 maxForce)
        {
            maxForce = maxForce.Absolute();

            Vector3 requiredForce = (movement - Velocity) * Mass;
            requiredForce -= force;
            AddForce(VMath.ClampComponents(requiredForce, -maxForce, maxForce));
        }

        /// <summary>
        ///     Tick this physics entity. An entity is ticked every update.
        /// </summary>
        /// <param name="deltaTime">The time since the last update.</param>
        public void Tick(float deltaTime)
        {
            IsGrounded = false;
            IsSwimming = false;

            force -= Velocity.Sign() * (Velocity * Velocity) * Drag;
            Velocity += force / Mass * deltaTime;

            Vector3 movement = Velocity * deltaTime;
            movement *= 1f / physicsIterations;

            HashSet<(Vector3i position, Block block)> blockIntersections = new();
            HashSet<(Vector3i position, Liquid liquid, LiquidLevel level)> liquidIntersections = new();

            for (var i = 0; i < physicsIterations; i++)
                DoPhysicsStep(ref movement, blockIntersections, liquidIntersections);

            foreach ((Vector3i position, Block block) in blockIntersections)
                if (block.ReceiveCollisions)
                    block.EntityCollision(this, position);

            Vector3 liquidDrag = Vector3.Zero;

            if (liquidIntersections.Count != 0)
            {
                var density = 0f;
                int maxLevel = -1;
                var noGas = false;

                foreach ((Vector3i position, Liquid liquid, LiquidLevel level) in liquidIntersections)
                {
                    if (liquid.ReceiveContact) liquid.EntityContact(this, position);

                    if ((int) level > maxLevel || maxLevel == 7 && liquid.Density > density)
                    {
                        density = liquid.Density;
                        maxLevel = (int) level;
                        noGas = liquid.IsLiquid;
                    }
                }

                liquidDrag = 0.5f * density * Velocity.Sign() * (Velocity * Velocity) * ((maxLevel + 1) / 8f) * 0.25f;

                if (!IsGrounded && noGas) IsSwimming = true;
            }

            boundingBox.Center = Position;

            force = new Vector3(x: 0f, Gravity * Mass, z: 0f);
            force -= liquidDrag;

            Update(deltaTime);
        }

        private void DoPhysicsStep(ref Vector3 movement,
            HashSet<(Vector3i position, Block block)> blockIntersections,
            HashSet<(Vector3i position, Liquid liquid, LiquidLevel level)> liquidIntersections)
        {
            boundingBox.Center += movement;

            if (BoundingBox.IntersectsTerrain(
                    World,
                    out bool xCollision,
                    out bool yCollision,
                    out bool zCollision,
                    blockIntersections,
                    liquidIntersections))
            {
                if (yCollision)
                {
                    Vector3i boundingBoxCenter = BoundingBox.Center.Floor();

                    IsGrounded = !World.GetBlock(
                            boundingBoxCenter + (0, (int) Math.Round(BoundingBox.Extents.Y), 0))
                        ?.Block.IsSolid ?? true;
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

        /// <summary>
        ///     Receives the entity update every tick.
        /// </summary>
        /// <param name="deltaTime"></param>
        protected abstract void Update(float deltaTime);

        #region IDisposable Support

        /// <summary>
        ///     Disposes this entity.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Finalizer.
        /// </summary>
        ~PhysicsEntity()
        {
            Dispose(disposing: false);
        }

        /// <summary>
        ///     Disposes this entity.
        /// </summary>
        /// <param name="disposing">True if called by code.</param>
        protected abstract void Dispose(bool disposing);

        #endregion IDisposable Support
    }
}