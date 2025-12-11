// <copyright file="Climbable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Behaviors.Events;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Miscellaneous;

/// <summary>
///     Allows an actor to climb up and down on this block.
/// </summary>
public partial class Climbable : BlockBehavior, IBehavior<Climbable, BlockBehavior, Block>
{
    [Constructible]
    private Climbable(Block subject) : base(subject) {}

    /// <summary>
    ///     The velocity at which an actor climbs up or down this block.
    /// </summary>
    public ResolvedProperty<Double> ClimbingVelocity { get; } = ResolvedProperty<Double>.New<Exclusive<Double, Void>>(nameof(ClimbingVelocity), initial: 1.0);

    /// <summary>
    ///     The velocity at which an actor slides down this block when not climbing.
    /// </summary>
    public ResolvedProperty<Double> SlidingVelocity { get; } = ResolvedProperty<Double>.New<Exclusive<Double, Void>>(nameof(SlidingVelocity), initial: 1.0);

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.IActorCollisionMessage>(OnActorCollision);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        ClimbingVelocity.Initialize(this);
        SlidingVelocity.Initialize(this);
    }

    private void OnActorCollision(Block.IActorCollisionMessage message)
    {
        Vector3d forwardMovement = Vector3d.Dot(message.Body.Movement, message.Body.Transform.Forward) * message.Body.Transform.Forward;
        Vector3d newVelocity;

        if (message.Body.Subject.Head != null &&
            forwardMovement.LengthSquared > 0.1f)
        {
            Double yVelocity = Vector3d.CalculateAngle(message.Body.Subject.Head.Forward, Vector3d.UnitY) < MathHelper.PiOver2
                ? ClimbingVelocity.Get()
                : -ClimbingVelocity.Get();

            newVelocity = new Vector3d(message.Body.Velocity.X, yVelocity, message.Body.Velocity.Z);
        }
        else
        {
            newVelocity = new Vector3d(
                message.Body.Velocity.X,
                MathHelper.Clamp(message.Body.Velocity.Y, -SlidingVelocity.Get(), Double.MaxValue),
                message.Body.Velocity.Z);
        }

        message.Body.Velocity = newVelocity;
    }
}
