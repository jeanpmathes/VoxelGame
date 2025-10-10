// <copyright file="Composite.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Annotations;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Logic.Voxels.Behaviors.Orienting;

namespace VoxelGame.Core.Logic.Voxels.Behaviors;

/// <summary>
///     A block that is a composite of multiple parts, occupying multiple block positions.
/// </summary>
public partial class Composite : BlockBehavior, IBehavior<Composite, BlockBehavior, Block>
{
    private Boolean isRotatable;

    private Composite(Block subject) : base(subject)
    {
        MaximumSizeInitializer = Aspect<Vector3i, Block>.New<Exclusive<Vector3i, Block>>(nameof(MaximumSizeInitializer), this);

        Size = Aspect<Vector3i, State>.New<Exclusive<Vector3i, State>>(nameof(Size), this);
        IsPlacementAllowed = Aspect<Boolean, (World, Vector3i, Vector3i, Actor?)>.New<ANDing<(World, Vector3i, Vector3i, Actor?)>>(nameof(IsPlacementAllowed), this);

        subject.IsPlacementAllowed.ContributeFunction(GetPlacementAllowed);

        subject.Require<Constraint>().IsValid.ContributeFunction(GetIsValid);

        subject.RequireIfPresent<LateralRotatableComposite, LateralRotatable>(rotatable => // todo: also ensure that oak door unaffected by the addition of this behavior
        {
            isRotatable = true;
            
            rotatable.PartState.ContributeFunction((original, context) => original.With(Part, context.part), exclusive: true);

            // While it would be possible to use an aspect for this, there are a few reasons not to:
            // - Using aspects for delegates with side effects goes against the core idea of aspects, even if the delegates will not be run when the aspect is evaluated.
            // - This behavior is the only one intended to set these delegates.
            rotatable.PublishPlacementCompleted = PublishPlacementCompletedMessage;
            rotatable.PublishNeighborUpdate = PublishNeighborUpdateMessage;
        });
    }

    [LateInitialization] private partial IAttribute<Vector3i> Part { get; set; }

    /// <summary>
    ///     Maximum size of the composite block in block positions.
    ///     Individual states can occupy a smaller size than this, but not larger.
    ///     If the block can be rotated, this size is interpreted as for the default orientation (north).
    ///     The size must be greater than zero in every dimension and not exceed the section size.
    /// </summary>
    public Vector3i MaximumSize { get; private set; } = Vector3i.One;

    /// <summary>
    ///     Aspect used to initialize the <see cref="MaximumSize" /> property.
    /// </summary>
    public Aspect<Vector3i, Block> MaximumSizeInitializer { get; }

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
    public static Composite Construct(Block input)
    {
        return new Composite(input);
    }

    /// <inheritdoc />
    public override void DefineEvents(IEventRegistry registry)
    {
        NeighborUpdate = registry.RegisterEvent<INeighborUpdateMessage>();
        PlacementCompleted = registry.RegisterEvent<IPlacementCompletedMessage>();
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        if (isRotatable) return;

        bus.Subscribe<Block.IPlacementMessage>(OnPlacement);
        bus.Subscribe<Block.IDestructionMessage>(OnDestruction);
        bus.Subscribe<Block.IStateUpdateMessage>(OnStateUpdate);
        bus.Subscribe<Block.INeighborUpdateMessage>(OnNeighborUpdate);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        MaximumSize = MaximumSizeInitializer.GetValue(Vector3i.One, Subject);

        properties.IsReplaceable.ContributeConstant(value: false, exclusive: true);
    }

    /// <inheritdoc />
    protected override void OnValidate(IValidator validator)
    {
        if (MaximumSize.X <= 0 || MaximumSize.Y <= 0 || MaximumSize.Z <= 0)
        {
            validator.ReportWarning("Composite block size must be greater than zero in every dimension");
        }

        if (MaximumSize.X > Section.Size || MaximumSize.Y > Section.Size || MaximumSize.Z > Section.Size)
        {
            validator.ReportWarning("Composite block size must not exceed section size in any dimension");
        }

        MaximumSize = Vector3i.Clamp(MaximumSize, Vector3i.One, new Vector3i(Section.Size));

        if (MaximumSize == Vector3i.One)
        {
            validator.ReportWarning("Composite block size is set to one, which is equivalent to a normal block");
        }
    }

    /// <inheritdoc />
    public override void DefineState(IStateBuilder builder)
    {
        Part = builder.Define(nameof(Part)).Vector3i(MaximumSize).Attribute();
    }

    private Boolean GetPlacementAllowed(Boolean original, (World world, Vector3i position, Actor? actor) context)
    {
        if (isRotatable) return true;

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
        State state = Subject.GetPlacementState(message.World, message.Position, message.Actor);
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

        {
            placementCompleted.World = world;
            placementCompleted.Position = position;
            placementCompleted.Part = part;
            placementCompleted.Actor = actor;
        }

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
            ResizeComposite(message.World, message.Position - currentPart, oldSize, newSize, newState);
        else if (message.OldState.Block != message.NewState.Block)
            SetStateOnAllParts(message.World, newSize, message.Position - currentPart, currentPart, message.NewState.Block);
    }

    private void OnNeighborUpdate(Block.INeighborUpdateMessage message)
    {
        Vector3i size = GetSize(message.State);

        Vector3i currentPart = GetPartPosition(message.State);
        Vector3i updatedPart = message.Side.Offset(currentPart);

        Boolean isPartOfComposite = updatedPart is {X: >= 0, Y: >= 0, Z: >= 0}
                                    && updatedPart.X < size.X && updatedPart.Y < size.Y && updatedPart.Z < size.Z;

        if (isPartOfComposite) return;
        
        PublishNeighborUpdateMessage(message.World, message.Position, currentPart, message.State, message.Side);
    }

    private void PublishNeighborUpdateMessage(World world, Vector3i position, Vector3i part, State state, Side side)
    {
        if (!NeighborUpdate.HasSubscribers) return;
        
        NeighborUpdateMessage neighborUpdate = IEventMessage<NeighborUpdateMessage>.Pool.Get();

        {
            neighborUpdate.World = world;
            neighborUpdate.Position = position;
            neighborUpdate.Part = part;
            neighborUpdate.State = state;
            neighborUpdate.Side = side;
        }

        NeighborUpdate.Publish(neighborUpdate);
        
        IEventMessage<NeighborUpdateMessage>.Pool.Return(neighborUpdate);
    }

    private void ResizeComposite(World world, Vector3i position, Vector3i oldSize, Vector3i newSize, State state)
    {
        Vector3i size = Vector3i.ComponentMax(oldSize, newSize);

        // todo: check if growing larger in at least one dimension, if so check if new positions can be replaced
        // todo: if not, restore the old state and then destroy the composite block

        for (var x = 0; x < size.X; x++)
        for (var y = 0; y < size.Y; y++)
        for (var z = 0; z < size.Z; z++)
        {
            Vector3i current = position + (x, y, z);

            Boolean inOld = x < oldSize.X && y < oldSize.Y && z < oldSize.Z;
            Boolean inNew = x < newSize.X && y < newSize.Y && z < newSize.Z;

            if (inOld && inNew)
            {
                state.Set(Part, (x, y, z));
                world.SetBlock(state, current);
            }
            else if (inOld && !inNew)
            {
                world.SetDefaultBlock(current);
            }
            else if (!inOld && inNew)
            {
                state.Set(Part, (x, y, z));
                world.SetBlock(state, current);
            }
        }
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

    /// <summary>
    ///     Get the size of the composite in a given state.
    /// </summary>
    /// <param name="state">The state to get the size for.</param>
    /// <returns>The size of the composite in the given state.</returns>
    public Vector3i GetSize(State state)
    {
        return Size.GetValue(MaximumSize, state);
    }

    /// <summary>
    ///     Sent when a neighboring position with a different block is updated.
    ///     This is essentially a filtered version of <see cref="Block.INeighborUpdateMessage" /> adapted to composite blocks.
    /// </summary>
    [GenerateRecord(typeof(IEventMessage<>))]
    public interface INeighborUpdateMessage : IEventMessage
    {
        /// <summary>
        ///     The world in which the neighbor update occurs.
        /// </summary>
        public World World { get; }

        /// <summary>
        ///     The position of the block.
        /// </summary>
        public Vector3i Position { get; }

        /// <summary>
        ///     The part of the block that is affected.
        /// </summary>
        public Vector3i Part { get; }

        /// <summary>
        ///     The state of this, unchanged, block at the position.
        /// </summary>
        public State State { get; }

        /// <summary>
        ///     The side of the block where the change happened.
        /// </summary>
        public Side Side { get; }
    }

    /// <summary>
    ///     Sent after the composite block was placed in the world successfully.
    /// </summary>
    [GenerateRecord(typeof(IEventMessage<>))]
    public interface IPlacementCompletedMessage : IEventMessage
    {
        /// <summary>
        ///     The world in which the placement was completed.
        /// </summary>
        public World World { get; }

        /// <summary>
        ///     The position at which the composite block was placed.
        /// </summary>
        public Vector3i Position { get; }

        /// <summary>
        ///     The part of the composite block that was placed.
        /// </summary>
        public Vector3i Part { get; }

        /// <summary>
        ///     The actor that placed the block.
        /// </summary>
        public Actor? Actor { get; }
    }
}
