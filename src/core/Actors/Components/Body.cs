// <copyright file="Body.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;

namespace VoxelGame.Core.Actors.Components;

/// <summary>
///     Adds physics capabilities to an <see cref="Actor" />.
/// </summary>
public partial class Body : ActorComponent
{
    /// <summary>
    ///     The gravitational constant which accelerates all bodies.
    /// </summary>
    private const Double Gravity = -9.81;

    private const Double AirDrag = 0.18;
    private const Double FluidDrag = 15.0;

    private const Int32 PhysicsIterations = 10;
    private readonly BoundingVolume boundingVolume;

    private readonly Double mass;

    private Vector3d force;

    private Boolean isEnabled = true;

    private Body(Actor subject, Double mass, BoundingVolume boundingVolume) : base(subject)
    {
        this.mass = mass;
        this.boundingVolume = boundingVolume;

        Transform = subject.GetRequiredComponent<Transform>();
    }

    [Constructible]
    private Body(Actor actor, Characteristics characteristics) : this(actor, characteristics.Mass, characteristics.BoundingVolume) {}

    /// <summary>
    ///     Get the transform of the body, which contains the position and orientation in the world.
    /// </summary>
    public Transform Transform { get; }

    /// <summary>
    ///     Gets or sets the velocity of the body.
    /// </summary>
    public Vector3d Velocity { get; set; }

    /// <summary>
    ///     Get the target movement of the body.
    /// </summary>
    public Vector3d Movement { get; set; }

    /// <summary>
    ///     Get whether the body touches the ground.
    /// </summary>
    public Boolean IsGrounded { get; private set; }

    /// <summary>
    ///     Get whether the body is in a fluid.
    /// </summary>
    public Boolean IsSwimming { get; private set; }

    /// <summary>
    ///     Get the collider of this body.
    /// </summary>
    public BoxCollider Collider => boundingVolume.GetColliderAt(Transform.Position);

    /// <summary>
    ///     Whether the body should perform physics calculations.
    ///     If no physics calculations are performed, methods such as <see cref="Move" /> will have no effect.
    /// </summary>
    public Boolean IsEnabled
    {
        get => isEnabled;
        set
        {
            Boolean oldValue = isEnabled;
            isEnabled = value;

            if (oldValue == value) return;

            LogSetActorPhysics(logger, isEnabled);
        }
    }

    /// <summary>
    ///     Applies force to this actor.
    /// </summary>
    /// <param name="additionalForce">The force to apply.</param>
    public void AddForce(Vector3d additionalForce)
    {
        force += additionalForce;
    }

    /// <summary>
    ///     Tries to move the actor in a certain direction using forces, but never using more force than specified.
    /// </summary>
    /// <param name="movement">The target movement, can be zero to try to stop moving.</param>
    /// <param name="maxForce">The maximum allowed force to use.</param>
    public void Move(Vector3d movement, Vector3d maxForce)
    {
        maxForce = maxForce.Absolute();

        Vector3d requiredForce = (movement - Velocity) * mass;
        requiredForce -= force;
        AddForce(requiredForce.ClampComponents(-maxForce, maxForce));
    }

    /// <inheritdoc />
    public override void OnLogicUpdate(Double deltaTime)
    {
        if (IsEnabled)
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
    }

    private void CalculatePhysics(Double deltaTime)
    {
        IsGrounded = false;
        IsSwimming = false;

        Velocity += force / mass * deltaTime;

        BoxCollider collider = Collider;

        Vector3d movement = Velocity * deltaTime;
        movement *= 1f / PhysicsIterations;

        HashSet<(Vector3i position, Block block)> blockIntersections = [];
        HashSet<(Vector3i position, Fluid fluid, FluidLevel level)> fluidIntersections = [];

        for (var i = 0; i < PhysicsIterations; i++)
            DoPhysicsStep(ref collider, ref movement, blockIntersections, fluidIntersections);

        foreach ((Vector3i position, Block block) in blockIntersections)
            if (block.IsCollider || block.IsTrigger)
                block.OnActorCollision(this, position);

        Double drag = AirDrag;

        if (fluidIntersections.Count != 0)
        {
            var useFluidDrag = false;
            var noGas = false;
            FluidLevel maxLevel = FluidLevel.None;

            foreach ((Vector3i position, Fluid fluid, FluidLevel level) in fluidIntersections)
            {
                if (fluid.ReceiveContact) fluid.ActorContact(this, position);

                useFluidDrag |= fluid.IsLiquid;
                noGas = fluid.IsLiquid;
                maxLevel = FluidLevel.Max(maxLevel, level);
            }

            if (useFluidDrag) drag = MathHelper.Lerp(AirDrag, FluidDrag, maxLevel.Fraction);

#pragma warning disable S2589 // IsGrounded is set in DoPhysicsStep
            if (!IsGrounded && noGas) IsSwimming = true;
#pragma warning restore S2589
        }

        force = new Vector3d(x: 0, Gravity * mass, z: 0);
        force -= (Vector3d) Velocity.Sign() * drag * Velocity.LengthSquared;
    }

    private void DoPhysicsStep(ref BoxCollider collider, ref Vector3d movement,
        HashSet<(Vector3i position, Block block)> blockIntersections,
        HashSet<(Vector3i position, Fluid fluid, FluidLevel level)> fluidIntersections)
    {
        collider.Position += movement;

        if (collider.IntersectsTerrain(
                Subject.World,
                out Boolean xCollision,
                out Boolean yCollision,
                out Boolean zCollision,
                blockIntersections,
                fluidIntersections))
        {
            if (yCollision)
            {
                Vector3i boundingBoxCenter = collider.Center.Floor();

                IsGrounded = !Subject.World.GetBlock(
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

        Transform.Position += movement;
    }

    /// <summary>
    ///     Describes the important characteristics of a body required on creation.
    /// </summary>
    /// <param name="Mass">The mass of the body, in kilograms.</param>
    /// <param name="BoundingVolume">The bounding volume of the body, which is used for collision detection.</param>
    public record Characteristics(Double Mass, BoundingVolume BoundingVolume);

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<Body>();

    [LoggerMessage(EventId = LogID.PhysicsActor + 0, Level = LogLevel.Information, Message = "Set actor physics to {State} (enabled/disabled)")]
    private static partial void LogSetActorPhysics(ILogger logger, Boolean state);

    #endregion LOGGING
}
