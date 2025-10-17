// <copyright file="Slowing.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Utilities;
using VoxelGame.Toolkit.Utilities;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Miscellaneous;

/// <summary>
///     Slows down all entities that come into contact with the block.
/// </summary>
public class Slowing : BlockBehavior, IBehavior<Slowing, BlockBehavior, Block>
{
    private Slowing(Block subject) : base(subject)
    {
    }

    /// <summary>
    ///     The maximum velocity that entities can have when in contact with this block.
    /// </summary>
    public ResolvedProperty<Double> MaxVelocity { get; } = ResolvedProperty<Double>.New<Exclusive<Double, Void>>(nameof(MaxVelocity), initial: 1.0);

    /// <inheritdoc />
    public static Slowing Construct(Block input)
    {
        return new Slowing(input);
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.IActorCollisionMessage>(OnActorCollision);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        MaxVelocity.Initialize(this);
    }

    private void OnActorCollision(Block.IActorCollisionMessage message)
    {
        // todo: multiply by height of the block if it has a height

        message.Body.Velocity = MathTools.Clamp(message.Body.Velocity, min: -1.0, MaxVelocity.Get());
    }
}
