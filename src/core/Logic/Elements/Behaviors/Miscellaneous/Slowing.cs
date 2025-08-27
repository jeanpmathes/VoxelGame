﻿// <copyright file="Slowing.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Logic.Elements.Behaviors.Miscellaneous;

/// <summary>
/// Slows down all entities that come into contact with the block.
/// </summary>
public class Slowing : BlockBehavior, IBehavior<Slowing, BlockBehavior, Block>
{
    private Slowing(Block subject) : base(subject)
    {
        MaxVelocityInitializer = Aspect<Double, Block>.New<Minimum<Double, Block>>(nameof(MaxVelocityInitializer), this);
    }
    
    /// <inheritdoc/>
    public static Slowing Construct(Block input)
    {
        return new Slowing(input);
    }
    
    /// <summary>
    /// The maximum velocity that entities can have when in contact with this block.
    /// </summary>
    public Double MaxVelocity { get; set; } = 1f;
    
    /// <summary>
    /// Aspect used to initialize the <see cref="MaxVelocity"/> property.
    /// </summary>
    public Aspect<Double, Block> MaxVelocityInitializer { get; }

    /// <inheritdoc/>
    public override void OnInitialize(BlockProperties properties)
    {
        MaxVelocity = MaxVelocityInitializer.GetValue(original: 1.0, Subject);
    }

    /// <inheritdoc/>
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.ActorCollisionMessage>(OnActorCollision);
    }

    private void OnActorCollision(Block.ActorCollisionMessage message)
    {
        // todo: multiply by height of the block if it has a height
        
        message.Body.Velocity = MathTools.Clamp(message.Body.Velocity, min: -1.0, MaxVelocity);
    }
}
