// <copyright file="Composite.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Logic.Voxels.Behaviors.Orienting;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Logic.Voxels.Behaviors;

/// <summary>
///     A block that is a composite of multiple parts, occupying multiple block positions.
/// </summary>
public partial class Composite : BlockBehavior, IBehavior<Composite, BlockBehavior, Block>
{
    private LateralRotatableComposite? rotatable;

    [Constructible]
    private Composite(Block subject) : base(subject)
    {
        Size = Aspect<Vector3i, State>.New<Exclusive<Vector3i, State>>(nameof(Size), this);
        IsPlacementAllowed = Aspect<Boolean, (World, Vector3i, Vector3i, Actor?)>.New<LogicalAnd<(World, Vector3i, Vector3i, Actor?)>>(nameof(IsPlacementAllowed), this);

        subject.IsPlacementAllowed.ContributeFunction(GetPlacementAllowed);

        subject.Require<Constraint>().IsValid.ContributeFunction(GetIsValid);

        subject.RequireIfPresent<LateralRotatableComposite, LateralRotatable>(lateralRotatableComposite =>
        {
            rotatable = lateralRotatableComposite;

            lateralRotatableComposite.PartState.ContributeFunction((original, context) => original.With(Part, context.part), exclusive: true);

            // While it would be possible to use an aspect for this, there are a few reasons not to:
            // - Using aspects for delegates with side effects goes against the core idea of aspects, even if the delegates will not be run when the aspect is evaluated.
            // - This behavior is the only one intended to set these delegates.
            lateralRotatableComposite.PublishPlacementCompleted = PublishPlacementCompletedMessage;
            lateralRotatableComposite.PublishNeighborUpdate = PublishNeighborUpdateMessage;
        });
    }

    [LateInitialization] private partial IAttributeData<Vector3i> Part { get; set; }

    /// <summary>
    ///     Maximum size of the composite block in block positions.
    ///     Individual states can occupy a smaller size than this, but not larger.
    ///     If the block can be rotated, this size is interpreted as for the default orientation (north).
    ///     The size must be greater than zero in every dimension and not exceed the section size.
    /// </summary>
    public ResolvedProperty<Vector3i> MaximumSize { get; } = ResolvedProperty<Vector3i>.New<Exclusive<Vector3i, Void>>(nameof(MaximumSize), Vector3i.One);

    /// <summary>
    ///     The actual size of a given state of the block.
    /// </summary>
    public Aspect<Vector3i, State> Size { get; }

    /// <summary>
    ///     Aspect to check whether the block can be placed at a given position in the world.
    /// </summary>
    public Aspect<Boolean, (World world, Vector3i position, Vector3i part, Actor? actor)> IsPlacementAllowed { get; }

    [LateInitialization] private partial IEvent<INeighborUpdateMessage> NeighborUpdate { get; set; }

    [LateInitialization] private partial IEvent<IPlacementCompletedMessage> PlacementCompleted { get; set; }

    /// <inheritdoc />
    public override void DefineEvents(IEventRegistry registry)
    {
        NeighborUpdate = registry.RegisterEvent<INeighborUpdateMessage>();
        PlacementCompleted = registry.RegisterEvent<IPlacementCompletedMessage>();
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        if (rotatable != null) return;

        bus.Subscribe<Block.IPlacementMessage>(OnPlacement);
        bus.Subscribe<Block.IDestructionMessage>(OnDestruction);
        bus.Subscribe<Block.IStateUpdateMessage>(OnStateUpdate);
        bus.Subscribe<Block.INeighborUpdateMessage>(OnNeighborUpdate);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        MaximumSize.Initialize(this);
    }

    /// <inheritdoc />
    protected override void OnValidate(IValidator validator)
    {
        Vector3i maxSize = MaximumSize.Get();

        if (maxSize.X <= 0 || maxSize.Y <= 0 || maxSize.Z <= 0) validator.ReportWarning("Composite block size must be greater than zero in every dimension");

        if (maxSize.X > Section.Size || maxSize.Y > Section.Size || maxSize.Z > Section.Size) validator.ReportWarning("Composite block size must not exceed section size in any dimension");

        maxSize = Vector3i.Clamp(maxSize, Vector3i.One, new Vector3i(Section.Size));

        if (maxSize == Vector3i.One) validator.ReportWarning("Composite block size is set to one, which is equivalent to a normal block");

        MaximumSize.Override(maxSize);

        ValidateForAllStatesOrError(validator,
            state =>
            {
                Vector3i size = GetSize(state);

                return size.X > 0 && size is {Y: > 0, Z: > 0}
                                  && size.X <= MaximumSize.Get().X
                                  && size.Y <= MaximumSize.Get().Y
                                  && size.Z <= MaximumSize.Get().Z;
            },
            "Composite block sizes must never exceed the maximum size");

        ValidateForAllStatesOrWarn(validator,
            state => state.IsReplaceable.Implies(GetSize(state) == Vector3i.One),
            "Only composite blocks of size one can be marked as replaceable");
    }

    /// <inheritdoc />
    public override void DefineState(IStateBuilder builder)
    {
        Part = builder.Define(nameof(Part)).Vector3i(MaximumSize.Get()).Attribute();
    }

    private Boolean GetPlacementAllowed(Boolean original, (World world, Vector3i position, Actor? actor) context)
    {
        if (rotatable != null) return true;

        (World world, Vector3i position, Actor? actor) = context;

        Vector3i size = GetSize(Subject.GetPlacementState(world, position, actor));

        for (var x = 0; x < size.X; x++)
        for (var y = 0; y < size.Y; y++)
        for (var z = 0; z < size.Z; z++)
        {
            State? block = world.GetBlock(position + (x, y, z));

            if (block?.IsReplaceable != true)
                return false;

            if (!IsPlacementAllowed.GetValue(original: true, (world, position + (x, y, z), (x, y, z), actor)))
                return false;
        }

        return true;
    }

    private Boolean GetIsValid(Boolean original, State state)
    {
        Vector3i currentSize = GetSize(state);
        Vector3i currentPart = GetPartPosition(state);

        return currentPart is {X: >= 0, Y: >= 0, Z: >= 0}
               && currentPart.X < currentSize.X && currentPart.Y < currentSize.Y && currentPart.Z < currentSize.Z;
    }

    private void OnPlacement(Block.IPlacementMessage message)
    {
        State state = message.PlacementState;
        Vector3i size = GetSize(state);

        for (var x = 0; x < size.X; x++)
        for (var y = 0; y < size.Y; y++)
        for (var z = 0; z < size.Z; z++)
        {
            Vector3i position = message.Position + (x, y, z);

            state = Subject.GetPlacementState(message.World, position, message.Actor);
            state.Set(Part, (x, y, z));

            message.World.SetBlock(state, position);

            PublishPlacementCompletedMessage(message.World, position, (x, y, z), message.Actor);
        }
    }

    private void PublishPlacementCompletedMessage(World world, Vector3i position, Vector3i part, Actor? actor)
    {
        if (!PlacementCompleted.HasSubscribers) return;

        PlacementCompletedMessage placementCompleted = IEventMessage<PlacementCompletedMessage>.Pool.Get();

        placementCompleted.World = world;
        placementCompleted.Position = position;
        placementCompleted.Part = part;
        placementCompleted.Actor = actor;

        PlacementCompleted.Publish(placementCompleted);

        IEventMessage<PlacementCompletedMessage>.Pool.Return(placementCompleted);
    }

    private void OnDestruction(Block.IDestructionMessage message)
    {
        Vector3i size = GetSize(message.State);
        Vector3i root = message.Position - GetPartPosition(message.State);

        for (var x = 0; x < size.X; x++)
        for (var y = 0; y < size.Y; y++)
        for (var z = 0; z < size.Z; z++)
        {
            Vector3i position = root + (x, y, z);
            message.World.SetDefaultBlock(position);
        }
    }

    private void OnStateUpdate(Block.IStateUpdateMessage message)
    {
        State oldState = message.OldState.Block;
        State newState = message.NewState.Block;

        if (oldState == newState) return;

        Vector3i oldSize = GetSize(oldState);
        Vector3i newSize = GetSize(newState);

        Vector3i currentPart = GetPartPosition(oldState);

        if (oldSize != newSize)
        {
            Vector3i position = message.Position - currentPart;

            if (!IsGrowthPossible(message.World, position, oldSize, newSize))
            {
                message.Undo();

                return;
            }

            ResizeComposite(message.World, position, oldSize, newSize, newState);
        }
        else if (message.OldState.Block != message.NewState.Block)
        {
            SetStateOnAllParts(message.World, newSize, message.Position - currentPart, currentPart, message.NewState.Block);
        }
    }

    private void OnNeighborUpdate(Block.INeighborUpdateMessage message)
    {
        Vector3i size = GetSize(message.State);

        Vector3i currentPart = GetPartPosition(message.State);
        Vector3i updatedPart = currentPart.Offset(message.Side);

        Boolean isPartOfComposite = updatedPart is {X: >= 0, Y: >= 0, Z: >= 0}
                                    && updatedPart.X < size.X && updatedPart.Y < size.Y && updatedPart.Z < size.Z;

        if (isPartOfComposite) return;

        PublishNeighborUpdateMessage(message.World, message.Position, currentPart, message.State, message.Side);
    }

    private void PublishNeighborUpdateMessage(World world, Vector3i position, Vector3i part, State state, Side side)
    {
        if (!NeighborUpdate.HasSubscribers) return;

        NeighborUpdateMessage neighborUpdate = IEventMessage<NeighborUpdateMessage>.Pool.Get();

        neighborUpdate.World = world;
        neighborUpdate.Position = position;
        neighborUpdate.Part = part;
        neighborUpdate.State = state;
        neighborUpdate.Side = side;

        NeighborUpdate.Publish(neighborUpdate);

        IEventMessage<NeighborUpdateMessage>.Pool.Return(neighborUpdate);
    }

    private void ResizeComposite(World world, Vector3i position, Vector3i oldSize, Vector3i newSize, State state)
    {
        Vector3i size = Vector3i.ComponentMax(oldSize, newSize);

        for (var x = 0; x < size.X; x++)
        for (var y = 0; y < size.Y; y++)
        for (var z = 0; z < size.Z; z++)
        {
            Vector3i current = position + (x, y, z);

            Boolean inOld = x < oldSize.X && y < oldSize.Y && z < oldSize.Z;
            Boolean inNew = x < newSize.X && y < newSize.Y && z < newSize.Z;

            Boolean kept = inOld && inNew;
            Boolean removed = inOld && !inNew;
            Boolean added = !inOld && inNew;

            if (kept || added)
            {
                state.Set(Part, (x, y, z));
                world.SetBlock(state, current);
            }
            else if (removed)
            {
                world.SetDefaultBlock(current);
            }
        }
    }

    private static Boolean IsGrowthPossible(World world, Vector3i position, Vector3i oldSize, Vector3i newSize)
    {
        Vector3i size = Vector3i.ComponentMax(oldSize, newSize);

        for (var x = 0; x < size.X; x++)
        for (var y = 0; y < size.Y; y++)
        for (var z = 0; z < size.Z; z++)
        {
            Vector3i current = position + (x, y, z);

            Boolean inOld = x < oldSize.X && y < oldSize.Y && z < oldSize.Z;
            Boolean inNew = x < newSize.X && y < newSize.Y && z < newSize.Z;

            if (inOld || !inNew) continue;

            State? block = world.GetBlock(current);

            if (block?.IsReplaceable != true)
                return false;
        }

        return true;
    }

    private void SetStateOnAllParts(World world, Vector3i size, Vector3i root, Vector3i exclude, State state)
    {
        for (var x = 0; x < size.X; x++)
        for (var y = 0; y < size.Y; y++)
        for (var z = 0; z < size.Z; z++)
        {
            if ((x, y, z) == exclude) continue;

            Vector3i current = root + (x, y, z);

            state.Set(Part, (x, y, z));

            world.SetBlock(state, current);
        }
    }

    /// <summary>
    ///     Get which part the block state is in the composite block.
    /// </summary>
    /// <param name="state">The state of the block to get the part position for.</param>
    /// <returns>The part position of the block in the composite block.</returns>
    public Vector3i GetPartPosition(State state)
    {
        return state.Get(Part);
    }

    private State SetPartPosition(State state, Vector3i part)
    {
        state.Set(Part, part);

        return state;
    }

    /// <summary>
    ///     Get the size of the composite in a given state.
    /// </summary>
    /// <param name="state">The state to get the size for.</param>
    /// <returns>The size of the composite in the given state.</returns>
    public Vector3i GetSize(State state)
    {
        return Size.GetValue(MaximumSize.Get(), state);
    }

    /// <summary>
    ///     Get the collider that encompasses all parts of the composite block for the provided state.
    /// </summary>
    /// <param name="state">The state for which to calculate the collider.</param>
    /// <param name="position">The world position of the queried part.</param>
    /// <returns>A collider covering all parts of the composite block.</returns>
    public BoxCollider GetFullCollider(State state, Vector3i position)
    {
        if (rotatable != null) return rotatable.GetFullCollider(state, position);

        CompositeColliderBuilder builder = new(this, state, SetPartPosition);

        return builder.Build(position);
    }

    /// <summary>
    ///     Sent when a neighboring position with a different block is updated.
    ///     This is essentially a filtered version of <see cref="Block.INeighborUpdateMessage" /> adapted to composite blocks.
    /// </summary>
    [GenerateRecord(typeof(IEventMessage<>))]
    public interface INeighborUpdateMessage
    {
        /// <summary>
        ///     The world in which the neighbor update occurs.
        /// </summary>
        World World { get; }

        /// <summary>
        ///     The position of the block.
        /// </summary>
        Vector3i Position { get; }

        /// <summary>
        ///     The part of the block that is affected.
        /// </summary>
        Vector3i Part { get; }

        /// <summary>
        ///     The state of this, unchanged, block at the position.
        /// </summary>
        State State { get; }

        /// <summary>
        ///     The side of the block where the change happened.
        /// </summary>
        Side Side { get; }
    }

    /// <summary>
    ///     Sent after the composite block was placed in the world successfully.
    /// </summary>
    [GenerateRecord(typeof(IEventMessage<>))]
    public interface IPlacementCompletedMessage
    {
        /// <summary>
        ///     The world in which the placement was completed.
        /// </summary>
        World World { get; }

        /// <summary>
        ///     The position at which the composite block was placed.
        /// </summary>
        Vector3i Position { get; }

        /// <summary>
        ///     The part of the composite block that was placed.
        /// </summary>
        Vector3i Part { get; }

        /// <summary>
        ///     The actor that placed the block.
        /// </summary>
        Actor? Actor { get; }
    }
}
