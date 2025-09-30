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

namespace VoxelGame.Core.Logic.Elements.Behaviors;

/// <summary>
///     A block which must be attached to another block in order to exist.
/// </summary>
public class Attached : BlockBehavior, IBehavior<Attached, BlockBehavior, Block>
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

    private Attached(Block subject) : base(subject)
    {
        AttachmentSidesInitializer = Aspect<Sides, Block>.New<Exclusive<Sides, Block>>(nameof(AttachmentSidesInitializer), this);
        ModeInitializer = Aspect<AttachmentMode, Block>.New<Exclusive<AttachmentMode, Block>>(nameof(ModeInitializer), this);

        AttachedSides = Aspect<Sides, State>.New<Exclusive<Sides, State>>(nameof(AttachedSides), this);
        AttachedState = Aspect<State?, (State, Sides)>.New<Exclusive<State?, (State, Sides)>>(nameof(AttachedState), this);

        IsOtherwiseAttached = Aspect<Boolean, (World, Vector3i, State)>.New<ORing<(World, Vector3i, State)>>(nameof(IsOtherwiseAttached), this);

        subject.IsPlacementAllowed.ContributeFunction(GetIsPlacementAllowed);
        subject.PlacementState.ContributeFunction(GetPlacementState);
    }

    /// <summary>
    ///     Which sides of the block can be attached to other blocks.
    /// </summary>
    public Sides AttachmentSides { get; private set; } = Sides.All;

    /// <summary>
    ///     Aspect used to initialize the <see cref="AttachmentSides" /> property.
    /// </summary>
    public Aspect<Sides, Block> AttachmentSidesInitializer { get; }

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

    /// <summary>
    ///     The attachment mode of the block.
    /// </summary>
    public AttachmentMode Mode { get; private set; } = AttachmentMode.Single;

    /// <summary>
    ///     Aspect used to initialize the <see cref="Mode" /> property.
    /// </summary>
    public Aspect<AttachmentMode, Block> ModeInitializer { get; }

    /// <inheritdoc />
    public static Attached Construct(Block input)
    {
        return new Attached(input);
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.NeighborUpdateMessage>(OnNeighborUpdate);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        AttachmentSides = AttachmentSidesInitializer.GetValue(Sides.All, Subject);
        Mode = ModeInitializer.GetValue(AttachmentMode.Single, Subject);
    }

    /// <inheritdoc />
    protected override void OnValidate(IValidator validator)
    {
        if (AttachmentSides == Sides.None)
            validator.ReportWarning("Block cannot be placed as it has no sides allowing attachment");
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

        if (Mode == AttachmentMode.Single)
        {
            if (side == null || !AttachmentSides.HasFlag(side.Value.ToFlag()))
                return original;

            return AttachedState.GetValue(original, (original, side.Value.ToFlag())) ?? original;
        }

        var attachableSides = Sides.None;

        foreach (Side possibleSide in Side.All.Sides())
        {
            if (AttachmentSides.HasFlag(possibleSide.ToFlag()) &&
                context.world.GetBlock(possibleSide.Offset(context.position))?.IsFullySolid == true)
            {
                attachableSides |= possibleSide.ToFlag();
            }
        }

        return AttachedState.GetValue(original, (original, attachableSides)) ?? original;
    }

    private void OnNeighborUpdate(Block.NeighborUpdateMessage message)
    {
        Sides sides = AttachedSides.GetValue(Sides.None, message.State);

        if (!sides.HasFlag(message.Side.ToFlag()) ||
            message.World.GetBlock(message.Side.Offset(message.Position))?.IsFullySolid == true)
            return;

        if (Mode == AttachmentMode.Single)
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

        if (Mode == AttachmentMode.Single)
        {
            if (IsOtherwiseAttached.GetValue(original: false, (world, position, state)))
                return;

            foreach (Side side in Side.All.Sides())
            {
                if (!sides.HasFlag(side.ToFlag()))
                    continue;

                if (world.GetBlock(side.Offset(position))?.IsFullySolid == true)
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
                    world.GetBlock(side.Offset(position))?.IsFullySolid == true)
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
}
