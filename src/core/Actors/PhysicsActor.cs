// <copyright file="PhysicsActor.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Actors;

/// <summary>
///     An actor which is affected by gravity and forces.
/// </summary>
public abstract partial class PhysicsActor : Actor, IOrientable
{
    /// <summary>
    ///     The gravitational constant which accelerates all physics actors.
    /// </summary>
    private const Double Gravity = -9.81;

    private const Double AirDrag = 0.18;
    private const Double FluidDrag = 15.0;

    private const Int32 PhysicsIterations = 10;

    private readonly BoundingVolume boundingVolume;

    private Vector3d actualPosition;
    private Vector3d force;

    private Boolean doPhysics = true;

    /// <summary>
    ///     Create a new physics-actor.
    /// </summary>
    /// <param name="mass">The mass of the actor.</param>
    /// <param name="boundingVolume">The bounding box of the actor.</param>
    protected PhysicsActor(Double mass, BoundingVolume boundingVolume)
    {
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
    ///     Get the rotation of the physics actor.
    /// </summary>
    protected Quaterniond Rotation { get; set; } = Quaterniond.Identity;

    /// <summary>
    ///     Get whether the physics actor touches the ground.
    /// </summary>
    public Boolean IsGrounded { get; private set; }

    /// <summary>
    ///     Get whether the physics actor is in a fluid.
    /// </summary>
    public Boolean IsSwimming { get; private set; }

    /// <summary>
    ///     Get the target movement of the physics actor.
    /// </summary>
    public abstract Vector3d Movement { get; }

    /// <summary>
    /// The head of the physics actor, which allows to determine where the actor is looking at.
    /// If an actor has no head or the concept of looking does not make sense, this will return the actor itself.
    /// </summary>
    public virtual IOrientable Head => this;

    /// <summary>
    ///     Get the block side targeted by the physics actor.
    /// </summary>
    public abstract Side TargetSide { get; }

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

            LogSetActorPhysics(logger, doPhysics);
        }
    }

    /// <summary>
    ///     Get the position of the physics actor.
    /// </summary>
    public Vector3d Position
    {
        get => actualPosition;
        set => actualPosition = VMath.ClampComponents(value, -World.Extents, World.Extents);
    }

    /// <summary>
    ///     Get the forward vector of the physics actor.
    /// </summary>
    public Vector3d Forward => Rotation * Vector3d.UnitX;

    /// <summary>
    ///     Get the right vector of the physics actor.
    /// </summary>
    public Vector3d Right => Rotation * Vector3d.UnitZ;

    /// <summary>
    ///     Applies force to this actor.
    /// </summary>
    /// <param name="additionalForce">The force to apply.</param>
    public void AddForce(Vector3d additionalForce)
    {
        Throw.IfDisposed(disposed);

        force += additionalForce;
    }

    /// <summary>
    ///     Tries to move the actor in a certain direction using forces, but never using more force than specified.
    /// </summary>
    /// <param name="movement">The target movement, can be zero to try to stop moving.</param>
    /// <param name="maxForce">The maximum allowed force to use.</param>
    public void Move(Vector3d movement, Vector3d maxForce)
    {
        Throw.IfDisposed(disposed);

        maxForce = maxForce.Absolute();

        Vector3d requiredForce = (movement - Velocity) * Mass;
        requiredForce -= force;
        AddForce(VMath.ClampComponents(requiredForce, -maxForce, maxForce));
    }

    /// <inheritdoc />
    public override void Tick(Double deltaTime)
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
        movement *= 1f / PhysicsIterations;

        HashSet<(Vector3i position, Block block)> blockIntersections = [];
        HashSet<(Vector3i position, Fluid fluid, FluidLevel level)> fluidIntersections = [];

        for (var i = 0; i < PhysicsIterations; i++)
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

#pragma warning disable S2589 // IsGrounded is set in DoPhysicsStep
            if (!IsGrounded && noGas) IsSwimming = true;
#pragma warning restore S2589
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

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<PhysicsActor>();

    [LoggerMessage(EventId = LogID.PhysicsActor + 0, Level = LogLevel.Information, Message = "Set actor physics to {State} (enabled/disabled)")]
    private static partial void LogSetActorPhysics(ILogger logger, Boolean state);

    #endregion LOGGING

    #region IDisposable Support

    private Boolean disposed;

    /// <inheritdoc />
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);

        if (disposed)
            return;

        disposed = true;
    }

    #endregion IDisposable Support
}
