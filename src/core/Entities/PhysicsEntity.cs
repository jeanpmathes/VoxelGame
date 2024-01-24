﻿// <copyright file="PhysicsEntity.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;

namespace VoxelGame.Core.Entities;

/// <summary>
///     An entity which is affected by gravity and forces.
/// </summary>
public abstract class PhysicsEntity : IDisposable
{
    /// <summary>
    ///     The gravitational constant which accelerates all physics entities.
    /// </summary>
    private const double Gravity = -9.81;

    private const double AirDrag = 0.18;
    private const double FluidDrag = 15.0;

    private static readonly ILogger logger = LoggingHelper.CreateLogger<PhysicsEntity>();

    private readonly BoundingVolume boundingVolume;

    private readonly int physicsIterations = 10;

    private Vector3d actualPosition;

    private bool doPhysics = true;

    private Vector3d force;

    /// <summary>
    ///     Create a new physics entity.
    /// </summary>
    /// <param name="world">The world in which the physics entity is located.</param>
    /// <param name="mass">The mass of the entity.</param>
    /// <param name="boundingVolume">The bounding box of the entity.</param>
    protected PhysicsEntity(World world, double mass, BoundingVolume boundingVolume)
    {
        World = world;

        Rotation = Quaterniond.Identity;

        Mass = mass;
        this.boundingVolume = boundingVolume;
    }

    /// <summary>
    ///     Gets the mass of this physics entity.
    /// </summary>
    private double Mass { get; }

    /// <summary>
    ///     Gets or sets the velocity of the physics entity.
    /// </summary>
    public Vector3d Velocity { get; set; }

    /// <summary>
    ///     Get the position of the physics entity.
    /// </summary>
    public Vector3d Position
    {
        get => actualPosition;
        protected set => actualPosition = VMath.ClampComponents(value, -World.Extents, World.Extents);
    }

    /// <summary>
    ///     Get the rotation of the physics entity.
    /// </summary>
    protected Quaterniond Rotation { get; set; }

    /// <summary>
    ///     Get whether the physics entity touches the ground.
    /// </summary>
    protected bool IsGrounded { get; private set; }

    /// <summary>
    ///     Get whether the physics entity is in a fluid.
    /// </summary>
    protected bool IsSwimming { get; private set; }

    /// <summary>
    ///     Get the forward vector of the physics entity.
    /// </summary>
    public Vector3d Forward => Rotation * Vector3d.UnitX;

    /// <summary>
    ///     Get the right vector of the physics entity.
    /// </summary>
    public Vector3d Right => Rotation * Vector3d.UnitZ;

    /// <summary>
    ///     Get the world in which the physics entity is located.
    /// </summary>
    public World World { get; }

    /// <summary>
    ///     Get the target movement of the physics entity.
    /// </summary>
    public abstract Vector3d Movement { get; }

    /// <summary>
    ///     Get the looking direction of the physics entity, which is also the front vector of the view camera.
    /// </summary>
    public abstract Vector3d LookingDirection { get; }

    /// <summary>
    ///     Get the block side targeted by the physics entity.
    /// </summary>
    public abstract BlockSide TargetSide { get; }

    /// <summary>
    ///     Get the block position targeted by the physics entity.
    ///     If the entity is not targeting a block, this will be null.
    /// </summary>
    public abstract Vector3i? TargetPosition { get; }

    /// <summary>
    ///     Get the collider of this physics entity.
    /// </summary>
    public BoxCollider Collider => boundingVolume.GetColliderAt(Position);

    /// <summary>
    ///     Whether the physics entity should perform physics calculations.
    ///     If no physics calculations are performed, methods such as <see cref="Move" /> will have no effect.
    /// </summary>
    public bool DoPhysics
    {
        get => doPhysics;
        set
        {
            Throw.IfDisposed(disposed);

            bool oldValue = doPhysics;
            doPhysics = value;

            if (oldValue == value) return;

            logger.LogInformation(
                Events.PhysicsSystem,
                "Set entity physics to {State}",
                doPhysics ? "enabled" : "disabled");
        }
    }

    /// <summary>
    ///     Applies force to this entity.
    /// </summary>
    /// <param name="additionalForce">The force to apply.</param>
    protected void AddForce(Vector3d additionalForce)
    {
        Throw.IfDisposed(disposed);

        force += additionalForce;
    }

    /// <summary>
    ///     Tries to move the entity in a certain direction using forces, but never using more force than specified.
    /// </summary>
    /// <param name="movement">The target movement, can be zero to try to stop moving.</param>
    /// <param name="maxForce">The maximum allowed force to use.</param>
    protected void Move(Vector3d movement, Vector3d maxForce)
    {
        Throw.IfDisposed(disposed);

        maxForce = maxForce.Absolute();

        Vector3d requiredForce = (movement - Velocity) * Mass;
        requiredForce -= force;
        AddForce(VMath.ClampComponents(requiredForce, -maxForce, maxForce));
    }

    /// <summary>
    ///     Tick this physics entity. An entity is ticked every update.
    /// </summary>
    /// <param name="deltaTime">The time since the last update.</param>
    public void Tick(double deltaTime)
    {
        Throw.IfDisposed(disposed);

        if (DoPhysics)
        {
            CalculatePhysics(deltaTime);
        }
        else
        {
            force = Vector3d.Zero;
            Velocity = Vector3d.Zero;

            IsGrounded = false;
            IsSwimming = false;
        }

        Update(deltaTime);
    }

    private void CalculatePhysics(double deltaTime)
    {
        IsGrounded = false;
        IsSwimming = false;

        Velocity += force / Mass * deltaTime;

        BoxCollider collider = Collider;

        Vector3d movement = Velocity * deltaTime;
        movement *= 1f / physicsIterations;

        HashSet<(Vector3i position, Block block)> blockIntersections = new();
        HashSet<(Vector3i position, Fluid fluid, FluidLevel level)> fluidIntersections = new();

        for (var i = 0; i < physicsIterations; i++)
            DoPhysicsStep(ref collider, ref movement, blockIntersections, fluidIntersections);

        foreach ((Vector3i position, Block block) in blockIntersections)
            if (block.ReceiveCollisions)
                block.EntityCollision(this, position);

        double drag = AirDrag;

        if (fluidIntersections.Count != 0)
        {
            var useFluidDrag = false;
            var noGas = false;
            var maxLevel = 0;

            foreach ((Vector3i position, Fluid fluid, FluidLevel level) in fluidIntersections)
            {
                if (fluid.ReceiveContact) fluid.EntityContact(this, position);

                useFluidDrag |= fluid.IsFluid;
                noGas = fluid.IsFluid;
                maxLevel = Math.Max(maxLevel, (int) level);
            }

            if (useFluidDrag) drag = MathHelper.Lerp(AirDrag, FluidDrag, (maxLevel + 1) / 8.0);

            if (!IsGrounded && noGas) IsSwimming = true;
        }

        force = new Vector3d(x: 0, Gravity * Mass, z: 0);
        force -= Velocity.Sign().ToVector3d() * drag * Velocity.LengthSquared;
    }

    private void DoPhysicsStep(ref BoxCollider collider, ref Vector3d movement,
        HashSet<(Vector3i position, Block block)> blockIntersections,
        HashSet<(Vector3i position, Fluid fluid, FluidLevel level)> fluidIntersections)
    {
        collider.Position += movement;

        if (collider.IntersectsTerrain(
                World,
                out bool xCollision,
                out bool yCollision,
                out bool zCollision,
                blockIntersections,
                fluidIntersections))
        {
            if (yCollision)
            {
                Vector3i boundingBoxCenter = collider.Center.Floor();

                IsGrounded = !World.GetBlock(
                        boundingBoxCenter + (0, (int) Math.Round(collider.Volume.Extents.Y), 0))
                    ?.Block.IsSolid ?? true;
            }

            movement = new Vector3d(
                xCollision ? 0 : movement.X,
                yCollision ? 0 : movement.Y,
                zCollision ? 0 : movement.Z);

            Velocity = new Vector3d(
                xCollision ? 0 : Velocity.X,
                yCollision ? 0 : Velocity.Y,
                zCollision ? 0 : Velocity.Z);
        }

        Position += movement;
    }

    /// <summary>
    ///     Receives the entity update every tick.
    /// </summary>
    /// <param name="deltaTime"></param>
    protected abstract void Update(double deltaTime);

    #region IDisposable Support

    /// <summary>
    ///     Whether this entity is disposed.
    /// </summary>
    protected bool disposed;

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
