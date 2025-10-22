// <copyright file="Attached.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Actors.Components;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Logic.Voxels.Behaviors;

/// <summary>
///     A block which must be attached to another block in order to exist.
/// </summary>
public partial class Attached : BlockBehavior, IBehavior<Attached, BlockBehavior, Block>
{
    /// <summary>
    ///     Describes how the block can be attached.
    /// </summary>
    public enum AttachmentMode
    {
        /// <summary>
        ///     The block can only be attached to one side at a time.
        ///     If that side is removed, the block will be destroyed.
        /// </summary>
        Single,

        /// <summary>
        ///     The block can be attached to multiple sides at a time.
        ///     If a single side is removed, the block will remain as long as it is still attached to at least one other side.
        ///     The block will not re-attach to sides it is not currently attached to.
        /// </summary>
        Multi
    }

    [Constructible]
    private Attached(Block subject) : base(subject)
    {
        AttachedSides = Aspect<Sides, State>.New<Exclusive<Sides, State>>(nameof(AttachedSides), this);
        AttachedState = Aspect<State?, (State, Sides)>.New<Exclusive<State?, (State, Sides)>>(nameof(AttachedState), this);

        IsOtherwiseAttached = Aspect<Boolean, (World, Vector3i, State)>.New<ORing<(World, Vector3i, State)>>(nameof(IsOtherwiseAttached), this);

        subject.IsPlacementAllowed.ContributeFunction(GetIsPlacementAllowed);
        subject.PlacementState.ContributeFunction(GetPlacementState);
    }

    /// <summary>
    ///     Which sides of the block can be attached to other blocks.
    /// </summary>
    public ResolvedProperty<Sides> AttachmentSides { get; } = ResolvedProperty<Sides>.New<Exclusive<Sides, Void>>(nameof(AttachmentSides), Sides.All);
    
    /// <summary>
    ///     The attachment mode of the block.
    /// </summary>
    public ResolvedProperty<AttachmentMode> Mode { get; } = ResolvedProperty<AttachmentMode>.New<Exclusive<AttachmentMode, Void>>(nameof(Mode));

    /// <summary>
    ///     Get the sides the block is currently attached at in a given state.
    /// </summary>
    public Aspect<Sides, State> AttachedSides { get; }

    /// <summary>
    ///     Get a state where the block is attached in the given sides, starting from a given other state.
    ///     May be <c>null</c> if the given sides are not supported by the block.
    /// </summary>
    public Aspect<State?, (State state, Sides sides)> AttachedState { get; }

    /// <summary>
    ///     Get whether the block is attached by any other means not covered by this behavior.
    ///     Does not have an effect if the mode is <see cref="AttachmentMode.Multi" />.
    /// </summary>
    public Aspect<Boolean, (World world, Vector3i position, State state)> IsOtherwiseAttached { get; }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.INeighborUpdateMessage>(OnNeighborUpdate);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        AttachmentSides.Initialize(this);
        Mode.Initialize(this);
    }

    /// <inheritdoc />
    protected override void OnValidate(IValidator validator)
    {
        if (AttachmentSides.Get() == Sides.None)
            validator.ReportWarning("Block cannot be placed as it has no sides allowing attachment");
    }

    private Boolean GetIsPlacementAllowed(Boolean original, (World world, Vector3i position, Actor? actor) context)
    {
        (World world, Vector3i position, Actor? actor) = context;

        Side? side = actor?.GetTargetedSide()?.Opposite();

        if (side == null)
            return false;

        if (!AttachmentSides.Get().HasFlag(side.Value.ToFlag()))
            return false;

        return world.GetBlock(position.Offset(side.Value))?.IsFullySolid == true;
    }

    private State GetPlacementState(State original, (World world, Vector3i position, Actor? actor) context)
    {
        (World _, Vector3i _, Actor? actor) = context;

        Side? side = actor?.GetTargetedSide()?.Opposite();

        if (Mode.Get() == AttachmentMode.Single)
        {
            if (side == null || !AttachmentSides.Get().HasFlag(side.Value.ToFlag()))
                return original;

            return AttachedState.GetValue(original, (original, side.Value.ToFlag())) ?? original;
        }

        var attachableSides = Sides.None;

        foreach (Side possibleSide in Side.All.Sides())
        {
            if (AttachmentSides.Get().HasFlag(possibleSide.ToFlag()) &&
                context.world.GetBlock(context.position.Offset(possibleSide))?.IsFullySolid == true)
            {
                attachableSides |= possibleSide.ToFlag();
            }
        }

        return AttachedState.GetValue(original, (original, attachableSides)) ?? original;
    }

    private void OnNeighborUpdate(Block.INeighborUpdateMessage message)
    {
        Sides sides = AttachedSides.GetValue(Sides.None, message.State);

        if (!sides.HasFlag(message.Side.ToFlag()) ||
            message.World.GetBlock(message.Position.Offset(message.Side))?.IsFullySolid == true)
            return;

        if (Mode.Get() == AttachmentMode.Single)
        {
            if (IsOtherwiseAttached.GetValue(original: false, (message.World, message.Position, message.State)))
                return;

            Subject.ScheduleDestroy(message.World, message.Position);
        }
        else
        {
            Sides remainingSides = sides & ~message.Side.ToFlag();

            if (remainingSides == Sides.None)
            {
                Subject.ScheduleDestroy(message.World, message.Position);
            }
            else
            {
                State newState = AttachedState.GetValue(message.State, (message.State, remainingSides)) ?? message.State;
                message.World.SetBlock(newState, message.Position);
            }
        }
    }

    /// <summary>
    ///     Check if the block is still properly attached, and destroy it if not.
    ///     Use this if the value of <see cref="IsOtherwiseAttached" /> may have changed.
    /// </summary>
    /// <param name="world">The world the block is in.</param>
    /// <param name="position">The position of the block.</param>
    /// <param name="state">The state of the block.</param>
    public void CheckAttachment(World world, Vector3i position, State state)
    {
        Sides sides = AttachedSides.GetValue(Sides.None, state);

        if (Mode.Get() == AttachmentMode.Single)
        {
            if (IsOtherwiseAttached.GetValue(original: false, (world, position, state)))
                return;

            foreach (Side side in Side.All.Sides())
            {
                if (!sides.HasFlag(side.ToFlag()))
                    continue;

                if (world.GetBlock(position.Offset(side))?.IsFullySolid == true)
                    continue;

                Subject.ScheduleDestroy(world, position);

                return;
            }
        }
        else
        {
            var remainingSides = Sides.None;

            foreach (Side side in Side.All.Sides())
            {
                if (sides.HasFlag(side.ToFlag()) &&
                    world.GetBlock(position.Offset(side))?.IsFullySolid == true)
                {
                    remainingSides |= side.ToFlag();
                }
            }

            if (remainingSides == Sides.None)
            {
                Subject.ScheduleDestroy(world, position);
            }
            else
            {
                State newState = AttachedState.GetValue(state, (state, remainingSides)) ?? state;
                world.SetBlock(newState, position);
            }
        }
    }
    
    /// <summary>
    /// Set the attachment on the given side, returning the new state.
    /// </summary>
    /// <param name="state">The original state.</param>
    /// <param name="attachment">The side to attach to.</param>
    /// <returns>The new state with the attachment applied, or the original state if the attachment could not be applied.</returns>
    public State SetAttachment(State state, Side attachment)
    {
        Sides attachmentFlag = attachment.ToFlag();

        if (!AttachmentSides.Get().HasFlag(attachmentFlag))
            return state;

        if (Mode.Get() == AttachmentMode.Single)
        {
            return AttachedState.GetValue(state, (state, attachmentFlag)) ?? state;
        }
        else
        {
            Sides currentSides = AttachedSides.GetValue(Sides.None, state);
            Sides newSides = currentSides | attachmentFlag;

            return AttachedState.GetValue(state, (state, newSides)) ?? state;
        }
    }
}
