﻿using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Elements.Behaviors.Height;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Elements.Behaviors;

/// <summary>
/// Adapts the Grounded behavior for composite blocks.
/// </summary>
public class CompositeGrounded : BlockBehavior, IBehavior<CompositeGrounded, BlockBehavior, Block>
{
    private CompositeGrounded(Block subject) : base(subject)
    {
        subject.Require<Composite>().IsPlacementAllowed.ContributeFunction(GetIsPlacementAllowed);
    }

    /// <inheritdoc />
    public static CompositeGrounded Construct(Block input)
    {
        return new CompositeGrounded(input);
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Composite.PlacementCompletedMessage>(OnPlacementCompleted);
        bus.Subscribe<Composite.NeighborUpdateMessage>(OnNeighborUpdate);
    }

    private static Boolean GetIsPlacementAllowed(Boolean original, (World world, Vector3i position, Vector3i part, Actor? actor) context)
    {
        (World world, Vector3i position, Vector3i part, Actor? _) = context;

        return part.Y != 0 || Grounded.IsGrounded(world, position);
    }
    
    private static void OnPlacementCompleted(Composite.PlacementCompletedMessage message)
    {
        Vector3i positionBelow = message.Position.Below();
        BlockInstance blockBelow = message.World.GetBlock(positionBelow) ?? BlockInstance.Default;

        if (blockBelow.IsFullySolid) 
            return;

        if (blockBelow.Block.Get<CompletableGround>() is {} completableGround)
            completableGround.BecomeComplete(message.World, positionBelow);
    }

    private void OnNeighborUpdate(Composite.NeighborUpdateMessage message)
    {
        if (message.Part.Y != 0 || message.Side != Side.Bottom) return;

        if (!Grounded.IsGrounded(message.World, message.Position))
            Subject.ScheduleDestroy(message.World, message.Position);
    }
}
