// <copyright file="Climbable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Behaviors.Events;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Miscellaneous;

/// <summary>
///     Allows an actor to climb up and down on this block.
/// </summary>
public class Climbable : BlockBehavior, IBehavior<Climbable, BlockBehavior, Block>
{
    private Climbable(Block subject) : base(subject)
    {
        ClimbingVelocityInitializer = Aspect<Double, Block>.New<Exclusive<Double, Block>>(nameof(ClimbingVelocityInitializer), this);
        SlidingVelocityInitializer = Aspect<Double, Block>.New<Exclusive<Double, Block>>(nameof(SlidingVelocityInitializer), this);
    }

    /// <summary>
    ///     The velocity at which an actor climbs up or down this block.
    /// </summary>
    public Double ClimbingVelocity { get; private set; } = 1.0;

    /// <summary>
    ///     Aspect used to initialize the <see cref="ClimbingVelocity" /> property.
    /// </summary>
    public Aspect<Double, Block> ClimbingVelocityInitializer { get; }

    /// <summary>
    ///     The velocity at which an actor slides down this block when not climbing.
    /// </summary>
    public Double SlidingVelocity { get; private set; } = 1.0;

    /// <summary>
    ///     Aspect used to initialize the <see cref="SlidingVelocity" /> property.
    /// </summary>
    public Aspect<Double, Block> SlidingVelocityInitializer { get; }

    /// <inheritdoc />
    public static Climbable Construct(Block input)
    {
        return new Climbable(input);
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.IActorCollisionMessage>(OnActorCollision);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        ClimbingVelocity = ClimbingVelocityInitializer.GetValue(original: 1.0, Subject);
        SlidingVelocity = SlidingVelocityInitializer.GetValue(original: 1.0, Subject);
    }

    // todo: check if there is an animation system note in the extended plan
    // todo: if no, create one 
    // todo: add to the animation system note that climbing should not use the physics system but instead be an animation sort of, with the ladder serving as a rail

    private void OnActorCollision(Block.IActorCollisionMessage message)
    {
        Vector3d forwardMovement = Vector3d.Dot(message.Body.Movement, message.Body.Transform.Forward) * message.Body.Transform.Forward;
        Vector3d newVelocity;

        if (message.Body.Subject.Head != null &&
            forwardMovement.LengthSquared > 0.1f)
        {
            Double yVelocity = Vector3d.CalculateAngle(message.Body.Subject.Head.Forward, Vector3d.UnitY) < MathHelper.PiOver2
                ? ClimbingVelocity
                : -ClimbingVelocity;

            newVelocity = new Vector3d(message.Body.Velocity.X, yVelocity, message.Body.Velocity.Z);
        }
        else
        {
            newVelocity = new Vector3d(
                message.Body.Velocity.X,
                MathHelper.Clamp(message.Body.Velocity.Y, -SlidingVelocity, Double.MaxValue),
                message.Body.Velocity.Z);
        }

        message.Body.Velocity = newVelocity;
    }
}
