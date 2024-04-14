// <copyright file="PhysicsActor.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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

namespace VoxelGame.Core.Actors;

/// <summary>
///     An actor which is affected by gravity and forces.
/// </summary>
public abstract class PhysicsActor : IDisposable
{
    /// <summary>
    ///     The gravitational constant which accelerates all physics actors.
    /// </summary>
    private const Double Gravity = -9.81;

    private const Double AirDrag = 0.18;
    private const Double FluidDrag = 15.0;

    private static readonly ILogger logger = LoggingHelper.CreateLogger<PhysicsActor>();

    private readonly BoundingVolume boundingVolume;

    private readonly Int32 physicsIterations = 10;

    private Vector3d actualPosition;

    private Boolean doPhysics = true;

    private Vector3d force;

    /// <summary>
    ///     Create a new physics actor.
    /// </summary>
    /// <param name="world">The world in which the physics actor is located.</param>
    /// <param name="mass">The mass of the actor.</param>
    /// <param name="boundingVolume">The bounding box of the actor.</param>
    protected PhysicsActor(World world, Double mass, BoundingVolume boundingVolume)
    {
        World = world;

        Rotation = Quaterniond.Identity;

        Mass = mass;
        this.boundingVolume = boundingVolume;
    }

    /// <summary>
    ///     Gets the mass of this physics actor.
    /// </summary>
    private Double Mass { get; }

    /// <summary>
    ///     Gets or sets the velocity of the physics actor.
    /// </summary>
    public Vector3d Velocity { get; set; }

    /// <summary>
    ///     Get the position of the physics actor.
    /// </summary>
    public Vector3d Position
    {
        get => actualPosition;
        protected set => actualPosition = VMath.ClampComponents(value, -World.Extents, World.Extents);
    }

    /// <summary>
    ///     Get the rotation of the physics actor.
    /// </summary>
    protected Quaterniond Rotation { get; set; }

    /// <summary>
    ///     Get whether the physics actor touches the ground.
    /// </summary>
    protected Boolean IsGrounded { get; private set; }

    /// <summary>
    ///     Get whether the physics actor is in a fluid.
    /// </summary>
    protected Boolean IsSwimming { get; private set; }

    /// <summary>
    ///     Get the forward vector of the physics actor.
    /// </summary>
    public Vector3d Forward => Rotation * Vector3d.UnitX;

    /// <summary>
    ///     Get the right vector of the physics actor.
    /// </summary>
    public Vector3d Right => Rotation * Vector3d.UnitZ;

    /// <summary>
    ///     Get the world in which the physics actor is located.
    /// </summary>
    public World World { get; }

    /// <summary>
    ///     Get the target movement of the physics actor.
    /// </summary>
    public abstract Vector3d Movement { get; }

    /// <summary>
    ///     Get the looking direction of the physics actor, which is also the front vector of the view camera.
    /// </summary>
    public abstract Vector3d LookingDirection { get; }

    /// <summary>
    ///     Get the block side targeted by the physics actor.
    /// </summary>
    public abstract BlockSide TargetSide { get; }

    /// <summary>
    ///     Get the block position targeted by the physics actor.
    ///     If the actor is not targeting a block, this will be null.
    /// </summary>
    public abstract Vector3i? TargetPosition { get; }

    /// <summary>
    ///     Get the collider of this physics actor.
    /// </summary>
    public BoxCollider Collider => boundingVolume.GetColliderAt(Position);

    /// <summary>
    ///     Whether the physics actor should perform physics calculations.
    ///     If no physics calculations are performed, methods such as <see cref="Move" /> will have no effect.
    /// </summary>
    public Boolean DoPhysics
    {
        get => doPhysics;
        set
        {
            Throw.IfDisposed(disposed);

            Boolean oldValue = doPhysics;
            doPhysics = value;

            if (oldValue == value) return;

            logger.LogInformation(
                Events.PhysicsSystem,
                "Set actor physics to {State}",
                doPhysics ? "enabled" : "disabled");
        }
    }

    /// <summary>
    ///     Applies force to this actor.
    /// </summary>
    /// <param name="additionalForce">The force to apply.</param>
    protected void AddForce(Vector3d additionalForce)
    {
        Throw.IfDisposed(disposed);

        force += additionalForce;
    }

    /// <summary>
    ///     Tries to move the actor in a certain direction using forces, but never using more force than specified.
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
    ///     Tick this physics actor. An actor is ticked every update.
    /// </summary>
    /// <param name="deltaTime">The time since the last update.</param>
    public void Tick(Double deltaTime)
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

    private void CalculatePhysics(Double deltaTime)
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
                block.ActorCollision(this, position);

        Double drag = AirDrag;

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
                maxLevel = Math.Max(maxLevel, (Int32) level);
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
                out Boolean xCollision,
                out Boolean yCollision,
                out Boolean zCollision,
                blockIntersections,
                fluidIntersections))
        {
            if (yCollision)
            {
                Vector3i boundingBoxCenter = collider.Center.Floor();

                IsGrounded = !World.GetBlock(
                        boundingBoxCenter + (0, (Int32) Math.Round(collider.Volume.Extents.Y), 0))
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
    ///     Receives the actor update every tick.
    /// </summary>
    /// <param name="deltaTime"></param>
    protected abstract void Update(Double deltaTime);

    #region IDisposable Support

    private Boolean disposed;

    /// <summary>
    ///     Disposes this actor.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~PhysicsActor()
    {
        Dispose(disposing: false);
    }

    /// <summary>
    ///     Disposes this actor.
    /// </summary>
    /// <param name="disposing">True if called by code.</param>
    protected virtual void Dispose(Boolean disposing)
    {
        if (disposed) return;

        disposed = true;
    }

    #endregion IDisposable Support
}
