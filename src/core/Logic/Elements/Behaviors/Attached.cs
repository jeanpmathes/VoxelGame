// <copyright file="Attached.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Actors.Components;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Logic.Elements.Behaviors;

/// <summary>
/// A block which must be attached to another block in order to exist.
/// </summary>
public class Attached : BlockBehavior, IBehavior<Attached, BlockBehavior, Block>
{
    /// <summary>
    /// Which sides of the block can be attached to other blocks.
    /// </summary>
    public Sides AttachmentSides { get; private set; } = Sides.All;
    
    /// <summary>
    /// Aspect used to initialize the <see cref="AttachmentSides"/> property.
    /// </summary>
    public Aspect<Sides, Block> AttachmentSidesInitializer { get; }
    
    /// <summary>
    /// Get the sides the block is currently attached at in a given state.
    /// </summary>
    public Aspect<Sides, State> AttachedSides { get; }
    
    /// <summary>
    /// Get a state where the block is attached in the given sides, starting from a given other state.
    /// May be <c>null</c> if the given sides are not supported by the block.
    /// </summary>
    public Aspect<State?, (State state, Sides sides)> AttachedState { get; }
    
    /// <summary>
    /// Get whether the block is attached by any other means not covered by this behavior.
    /// </summary>
    public Aspect<Boolean, (World world, Vector3i position, State state)> IsOtherwiseAttached { get; } // todo: go through tuples like this and enforce uniform order, or maybe add it to the analyzer note

    private Attached(Block subject) : base(subject)
    {
        AttachmentSidesInitializer = Aspect<Sides, Block>.New<Exclusive<Sides, Block>>(nameof(AttachmentSidesInitializer), this);
        
        AttachedSides = Aspect<Sides, State>.New<Exclusive<Sides, State>>(nameof(AttachedSides), this);
        AttachedState = Aspect<State?, (State, Sides)>.New<Exclusive<State?, (State, Sides)>>(nameof(AttachedState), this);
        
        IsOtherwiseAttached = Aspect<Boolean, (World, Vector3i, State)>.New<ORing<(World, Vector3i, State)>>(nameof(IsOtherwiseAttached), this);
        
        subject.IsPlacementAllowed.ContributeFunction(GetIsPlacementAllowed);
        subject.PlacementState.ContributeFunction(GetPlacementState);
    }
    
    /// <inheritdoc />
    public static Attached Construct(Block input)
    {
        return new Attached(input);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        AttachmentSides = AttachmentSidesInitializer.GetValue(Sides.All, Subject);
    }
    
    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.NeighborUpdateMessage>(OnNeighborUpdate);
    }

    /// <inheritdoc />
    protected override void OnValidate(IResourceContext context)
    {
        if (AttachmentSides == Sides.None)
            context.ReportWarning(this, "Block cannot be placed as it has no sides allowing attachment");
    }
    
    private Boolean GetIsPlacementAllowed(Boolean original, (World world, Vector3i position, Actor? actor) context)
    {
        (World world, Vector3i position, Actor? actor) = context;
        
        Side? side = actor?.GetTargetedSide()?.Opposite();
        
        if (side == null)
            return false;
        
        if (!AttachmentSides.HasFlag(side.Value.ToFlag()))
            return false;
        
        return world.GetBlock(side.Value.Offset(position))?.IsFullySolid == true;
    }
    
    private State GetPlacementState(State original, (World world, Vector3i position, Actor? actor) context)
    {
        (World _, Vector3i _, Actor? actor) = context;
        
        Side? side = actor?.GetTargetedSide()?.Opposite();
        
        if (side == null || !AttachmentSides.HasFlag(side.Value.ToFlag()))
            return original;

        return AttachedState.GetValue(original, (original, side.Value.ToFlag())) ?? original;
    }
    
    private void OnNeighborUpdate(Block.NeighborUpdateMessage message)
    {
        Sides sides = AttachedSides.GetValue(Sides.None, message.State);
        Side updatedSide = message.Side;

        if (!sides.HasFlag(message.Side.ToFlag()) ||
            message.World.GetBlock(message.Side.Offset(message.Position))?.IsFullySolid == true) 
            return;

        if (IsOtherwiseAttached.GetValue(original: false, (message.World, message.Position, message.State)))
            return;

        Subject.ScheduleDestroy(message.World, message.Position);
    }
}
