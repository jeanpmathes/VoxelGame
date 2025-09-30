// <copyright file="Block.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using VoxelGame.Annotations;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Actors.Components;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Logic.Definitions;
using VoxelGame.Core.Logic.Elements.Behaviors;
using VoxelGame.Core.Logic.Elements.Behaviors.Height;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Serialization;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Elements;

/// <summary>
///     The basic unit of the game world - a block.
///     Blocks use the flyweight pattern, the world data only stores a state ID.
///     The state ID can be used to retrieve both the type of the block and its state.
/// </summary>
public abstract partial class Block : BehaviorContainer<Block, BlockBehavior>, IIdentifiable<String>, IIdentifiable<UInt32>, IContent
{
    private const Int32 ScheduledDestroyOffset = 5;

    private Boolean isReplaceable;

    private BoundingVolume? placementBoundingVolume;
    private Boolean receivesCollisions;
    private BoundingVolume[]? stateBoundingVolumes;

    /// <summary>
    ///     Create a new block.
    /// </summary>
    /// <param name="id">The unique integer ID of the block.</param>
    /// <param name="namedID">The named ID of the block. A unique and unlocalized identifier.</param>
    /// <param name="name">The name of the block. Can be localized.</param>
    protected Block(UInt32 id, String namedID, String name)
    {
        Name = name;
        ID = id;
        NamedID = namedID;
        Identifier = RID.Named<Block>(namedID);

        BoundingVolume = Aspect<BoundingVolume, State>.New<Exclusive<BoundingVolume, State>>(nameof(BoundingVolume), this);
        PlacementState = Aspect<State, (World, Vector3i, Actor?)>.New<Chaining<State, (World, Vector3i, Actor?)>>(nameof(PlacementState), this);

        IsPlacementAllowed = Aspect<Boolean, (World, Vector3i, Actor?)>.New<ANDing<(World, Vector3i, Actor?)>>(nameof(IsPlacementAllowed), this);
        IsDestructionAllowed = Aspect<Boolean, (State, World, Vector3i, Actor?)>.New<ANDing<(State, World, Vector3i, Actor?)>>(nameof(IsDestructionAllowed), this);
    }

    /// <summary>
    ///     The states of the block.
    /// </summary>
    public StateSet States { get; private set; } = null!;

    [LateInitialization] private partial IEvent<ActorCollisionMessage> ActorCollision { get; set; }

    [LateInitialization] private partial IEvent<ActorInteractionMessage> ActorInteraction { get; set; }

    [LateInitialization] private partial IEvent<PlacementMessage> Placement { get; set; }

    [LateInitialization] private partial IEvent<PlacementCompletedMessage> PlacementCompleted { get; set; }

    [LateInitialization] private partial IEvent<DestructionMessage> Destruction { get; set; }

    [LateInitialization] private partial IEvent<DestructionCompletedMessage> DestructionCompleted { get; set; }

    [LateInitialization] private partial IEvent<StateUpdateMessage> StateUpdate { get; set; }

    [LateInitialization] private partial IEvent<NeighborUpdateMessage> NeighborUpdate { get; set; }

    [LateInitialization] private partial IEvent<RandomUpdateMessage> RandomUpdate { get; set; }

    [LateInitialization] private partial IEvent<ScheduledUpdateMessage> ScheduledUpdate { get; set; }

    [LateInitialization] private partial IEvent<GeneratorUpdateMessage> GeneratorUpdate { get; set; }

    /// <summary>
    ///     Whether the block is always full, meaning it occupies the entire voxel space it is in,
    ///     no matter the state of the block.
    /// </summary>
    protected virtual Boolean IsAlwaysFull => false;

    /// <summary>
    ///     Whether the block is opaque, meaning it does not allow light to pass through it.
    ///     If the block is not full, light can still pass around it.
    /// </summary>
    public Boolean IsOpaque { get; private set; }

    /// <summary>
    ///     Whether to mesh the face of the block when bordering a non-opaque block.
    /// </summary>
    public Boolean MeshFaceAtNonOpaques { get; private set; }

    /// <summary>
    ///     Whether the block has shade cast on it.
    /// </summary>
    public Boolean IsUnshaded { get; private set; }

    /// <summary>
    ///     Whether the block participates in the physics simulation, serving as an obstacle.
    /// </summary>
    public Boolean IsSolid { get; private set; }

    /// <summary>
    ///     A trigger is a solid block on which <see cref="OnActorCollision" /> should be called in case of a collision.
    /// </summary>
    public Boolean IsCollider => receivesCollisions && IsSolid;

    /// <summary>
    ///     A trigger is a non-solid block on which <see cref="OnActorCollision" /> should be called if they would collide if
    ///     it were solid.
    /// </summary>
    public Boolean IsTrigger => receivesCollisions && !IsSolid;

    /// <summary>
    ///     Whether it is possible to interact with the block.
    /// </summary>
    public Boolean IsInteractable { get; private set; }

    /// <summary>
    ///     Gets the localized name of the block.
    /// </summary>
    public String Name { get; }

    /// <summary>
    ///     Gets the unique integer ID of the block.
    /// </summary>
    public UInt32 ID { get; }

    /// <summary>
    ///     Aspect to determine the actual placement state of a block.
    /// </summary>
    public Aspect<State, (World world, Vector3i position, Actor? actor)> PlacementState { get; }

    /// <summary>
    ///     Aspect to check whether the block can be placed at a given position in the world.
    /// </summary>
    public Aspect<Boolean, (World world, Vector3i position, Actor? actor)> IsPlacementAllowed { get; }

    /// <summary>
    ///     Aspect to check whether the block can be destroyed at a given position in the world.
    /// </summary>
    public Aspect<Boolean, (State state, World world, Vector3i position, Actor? actor)> IsDestructionAllowed { get; }

    /// <summary>
    ///     The state-dependent bounding volume of the block.
    ///     Relevant for physics and placement checks.
    /// </summary>
    public Aspect<BoundingVolume, State> BoundingVolume { get; }

    /// <summary>
    ///     Get the placement bounding volume of the block.
    ///     It is the bounding volume used for the placement state.
    /// </summary>
    public BoundingVolume PlacementBoundingVolume => placementBoundingVolume ?? Physics.BoundingVolume.Block;

    /// <summary>
    ///     Defines the type of meshing this block uses.
    /// </summary>
    public abstract Meshable Meshable { get; }

    /// <inheritdoc />
    public String NamedID { get; }

    /// <inheritdoc />
    public RID Identifier { get; }

    /// <inheritdoc />
    public ResourceType Type => ResourceTypes.Block;

    /// <inheritdoc />
    String IIdentifiable<String>.ID => NamedID;

    /// <inheritdoc />
    UInt32 IIdentifiable<UInt32>.ID => ID;

    /// <summary>
    ///     Initialize the block with its states and internal values.
    ///     Must be called before the block behavior system is baked.
    /// </summary>
    /// <param name="offset">The number of already existing block states.</param>
    /// <param name="validator">The validator to use for validation.</param>
    /// <returns>The number of states this block has.</returns>
    internal UInt32 Initialize(UInt32 offset, IValidator validator)
    {
        EnsureNotBaked();

        InitializeProperties();

        StateBuilder builder = new(validator);

        foreach (BlockBehavior behavior in Behaviors)
            builder.Enclose(Reflections.GetLongName(behavior.GetType()), behavior.DefineState);

        States = builder.Build(this, offset);

        return States.Count;
    }

    /// <inheritdoc />
    public sealed override void DefineEvents(IEventRegistry registry)
    {
        ActorCollision = registry.RegisterEvent<ActorCollisionMessage>();
        ActorInteraction = registry.RegisterEvent<ActorInteractionMessage>();

        Placement = registry.RegisterEvent<PlacementMessage>(single: true);
        PlacementCompleted = registry.RegisterEvent<PlacementCompletedMessage>();

        Destruction = registry.RegisterEvent<DestructionMessage>(single: true);
        DestructionCompleted = registry.RegisterEvent<DestructionCompletedMessage>();

        StateUpdate = registry.RegisterEvent<StateUpdateMessage>();
        NeighborUpdate = registry.RegisterEvent<NeighborUpdateMessage>();
        RandomUpdate = registry.RegisterEvent<RandomUpdateMessage>();
        ScheduledUpdate = registry.RegisterEvent<ScheduledUpdateMessage>();
        GeneratorUpdate = registry.RegisterEvent<GeneratorUpdateMessage>(single: true);
    }

    /// <inheritdoc />
    public sealed override void SubscribeToEvents(IEventBus bus) {}

    private void InitializeProperties()
    {
        BlockProperties properties = new(this);

        foreach (BlockBehavior behavior in Behaviors)
            behavior.OnInitialize(properties);

        Initializing?.Invoke(this, new InitializingEventArgs(properties));

        IsOpaque = properties.IsOpaque.GetValue(original: true, this);
        MeshFaceAtNonOpaques = properties.MeshFaceAtNonOpaques.GetValue(original: false, this);
        IsSolid = properties.IsSolid.GetValue(original: true, this);
        isReplaceable = properties.IsReplaceable.GetValue(original: false, this);
        IsUnshaded = properties.IsUnshaded.GetValue(original: false, this);
    }

    /// <summary>
    ///     Invoked once on block initialization, before the block is baked.
    ///     Unsubscribe from this event after it was invoked.
    /// </summary>
    public event EventHandler<InitializingEventArgs>? Initializing;

    /// <inheritdoc />
    public sealed override void Validate(IValidator validator)
    {
        IMeshable? meshable = null;
        List<IMeshable> otherMeshables = [];

        foreach (BlockBehavior behavior in Behaviors)
        {
            if (behavior is not IMeshable m) continue;

            if (meshable == null)
                meshable = m;
            else
                otherMeshables.Add(meshable);
        }

        if (otherMeshables.Count > 0)
        {
            otherMeshables.Add(meshable!);

            validator.ReportWarning($"Multiple meshable behaviors found ({String.Join(", ", otherMeshables.Select(m => m.GetType()))}), which indicates a misconfiguration");
        }
        else if (meshable == null)
        {
            validator.ReportWarning("No meshable behavior found");
        }
        else if (Meshable != meshable.Type)
        {
            validator.ReportWarning($"Meshable type {meshable.Type.ToStringFast()} does not match the declared meshable type {Meshable.ToStringFast()}");
        }

        OnValidate();

        base.Validate(validator);
    }

    /// <summary>
    ///     Override this method to perform additional validation.
    /// </summary>
    protected abstract void OnValidate();

    /// <inheritdoc />
    protected override void OnBake()
    {
        receivesCollisions = ActorCollision.HasSubscribers;
        IsInteractable = ActorInteraction.HasSubscribers;
    }

    /// <summary>
    ///     Call this after baking of all blocks is done.
    ///     This completes the set-up of the block and its behaviors.
    /// </summary>
    /// <param name="textureIndexProvider">The texture index provider to use for the block.</param>
    /// <param name="blockModelProvider">The block model provider to use for the block.</param>
    /// <param name="visuals">The visual configuration to use for the block.</param>
    public void Activate(ITextureIndexProvider textureIndexProvider, IBlockModelProvider blockModelProvider, VisualConfiguration visuals)
    {
        BuildBoundingVolumes();
        BuildMeshes(textureIndexProvider, blockModelProvider, visuals);
    }

    /// <summary>
    ///     Get whether the block is solid and fully occupies the voxel space it is in, given a state.
    ///     This means that it is equivalent to a 1x1x1 collider.
    /// </summary>
    /// <param name="state">The state of the block.</param>
    /// <returns><c>true</c> if the block is solid and full in the given state, <c>false</c> otherwise.</returns>
    public Boolean IsFullySolid(State state)
    {
        return IsSolid && IsFull(state);
    }

    /// <summary>
    ///     Get whether the block is opaque and fully occupies the voxel space it is in.
    ///     This means no light can pass through any part of the voxel space the block is in.
    /// </summary>
    /// <param name="state">The state of the block.</param>
    /// <returns><c>true</c> if the block is opaque and full in the given state, <c>false</c> otherwise.</returns>
    public Boolean IsFullyOpaque(State state)
    {
        return IsOpaque && IsFull(state);
    }

    /// <summary>
    ///     Get whether the block fully occupies the voxel space it is in the given state.
    /// </summary>
    /// <param name="state">The state of the block.</param>
    /// <returns><c>true</c> if the block is full in the given state, <c>false</c> otherwise.</returns>
    public Boolean IsFull(State state)
    {
        if (IsAlwaysFull) return true;

        if (Get<PartialHeight>() is {} height)
            return height.GetHeight(state) == PartialHeight.MaximumHeight;

        return false;
    }

    /// <summary>
    ///     Get whether the side of a block is full in the given state.
    /// </summary>
    /// <param name="side">The side to check.</param>
    /// <param name="state">The state of the block.</param>
    /// <returns><c>true</c> if the side is full in the given state, <c>false</c> otherwise.</returns>
    public Boolean IsSideFull(Side side, State state)
    {
        if (IsAlwaysFull) return true;

        if (Get<PartialHeight>() is {} height)
            return height.GetHeight(state) == PartialHeight.MaximumHeight || height.IsSideFull(side, state);

        return false;
    }

    /// <summary>
    ///     Get whether the block can be replaced by another block.
    ///     A replaceable block can be overwritten without having to destroy it first.
    /// </summary>
    /// <param name="state">The state of the block.</param>
    /// <returns><c>true</c> if the block can be replaced, <c>false</c> otherwise.</returns>
    public Boolean IsReplaceable(State state)
    {
        return isReplaceable;
    }

    /// <summary>
    ///     This method is called when an actor collides with this block.
    /// </summary>
    /// <param name="body">The body of the actor.</param>
    /// <param name="position">The block position.</param>
    public void OnActorCollision(Body body, Vector3i position)
    {
        State? potentialBlock = body.Subject.World.GetBlock(position);

        if (potentialBlock?.Block != this) return;

        ActorCollisionMessage message = new(this)
        {
            Body = body,
            Position = position,
            State = potentialBlock.Value
        };

        ActorCollision.Publish(message);
    }

    /// <summary>
    ///     Called when a block and an actor interact.
    ///     If <see cref="IsInteractable" /> is <c>false</c>, this method does nothing.
    /// </summary>
    /// <param name="actor">The actor that interacts with the block.</param>
    /// <param name="position">The block position.</param>
    public void OnActorInteract(Actor actor, Vector3i position)
    {
        if (!IsInteractable) return;

        State? potentialBlock = actor.World.GetBlock(position);

        if (potentialBlock?.Block != this) return;

        ActorInteractionMessage message = new(this)
        {
            Actor = actor,
            Position = position,
            State = potentialBlock.Value
        };

        ActorInteraction.Publish(message);
    }

    /// <summary>
    ///     Get whether the block can be placed at the given position in the world.
    /// </summary>
    /// <param name="world">The world in which to check placement.</param>
    /// <param name="position">The position at which to check placement.</param>
    /// <param name="actor">The actor that is attempting to place the block, if any.</param>
    /// <returns><c>true</c> if the block can be placed at the given position, <c>false</c> otherwise.</returns>
    public Boolean CanPlace(World world, Vector3i position, Actor? actor = null)
    {
        return IsPlacementAllowed.GetValue(original: true, (world, position, actor));
    }

    /// <summary>
    ///     Attempt to place the block in the world.
    /// </summary>
    /// <param name="world">The world in which to place the block.</param>
    /// <param name="position">The position at which to place the block.</param>
    /// <param name="actor"></param>
    /// <returns>True if placement was successful.</returns>
    public Boolean Place(World world, Vector3i position, Actor? actor = null)
    {
        Content? content = world.GetContent(position);

        if (content == null) return false;

        (State block, FluidInstance _) = content.Value;

        if (!block.IsReplaceable)
            return false;

        if (!CanPlace(world, position, actor))
            return false;

        if (Placement.HasSubscribers)
        {
            PlacementMessage placement = new(this)
            {
                World = world,
                Position = position,
                Actor = actor
            };

            Placement.Publish(placement);
        }
        else
        {
            world.SetBlock(GetPlacementState(world, position, actor), position);
        }

        PlacementCompleted.Publish(new PlacementCompletedMessage(this)
        {
            World = world,
            Position = position,
            Actor = actor
        });

        return true;
    }

    /// <summary>
    ///     Get the state of the block preferred to be used for placement.
    /// </summary>
    /// <param name="world">World in which placement would occur.</param>
    /// <param name="position">The position at which placement would occur.</param>
    /// <param name="actor">The actor that would place the block, if any.</param>
    /// <returns>The preferred state for placement.</returns>
    public State GetPlacementState(World world, Vector3i position, Actor? actor = null)
    {
        return PlacementState.GetValue(States.PlacementDefault, (world, position, actor));
    }

    /// <summary>
    ///     Check whether the block can be destroyed at the given position in the world.
    /// </summary>
    /// <param name="world">The world in which to check destruction.</param>
    /// <param name="position">The position at which to check destruction.</param>
    /// <param name="actor">The actor that is attempting to destroy the block, if any.</param>
    /// <returns><c>true</c> if the block can be destroyed at the given position, <c>false</c> otherwise.</returns>
    public Boolean CanDestroy(World world, Vector3i position, Actor? actor = null)
    {
        State? potentialBlock = world.GetBlock(position);

        if (potentialBlock is not {} block) return false;
        if (block.Block != this) return false;

        return IsDestructionAllowed.GetValue(original: true, (block, world, position, actor));
    }

    /// <summary>
    ///     Attempt to destroy the block in the world.
    ///     Will always fail if there is a different block at the given position.
    /// </summary>
    /// <param name="world">The world in which to destroy the block.</param>
    /// <param name="position">The position at which to destroy to block.</param>
    /// <param name="actor">The actor destroying the block.</param>
    /// 1
    /// <returns>True if destruction was successful.</returns>
    public Boolean Destroy(World world, Vector3i position, Actor? actor = null)
    {
        if (!CanDestroy(world, position, actor))
            return false;

        State? potentialBlock = world.GetBlock(position);

        if (potentialBlock is not {} block) return false;

        if (Destruction.HasSubscribers)
        {
            DestructionMessage destruction = new(this)
            {
                World = world,
                Position = position,
                State = block,
                Actor = actor
            };

            Destruction.Publish(destruction);
        }
        else
        {
            world.SetDefaultBlock(position);
        }

        DestructionCompleted.Publish(new DestructionCompletedMessage(this)
        {
            World = world,
            Position = position,
            State = block,
            Actor = actor
        });

        return true;
    }

    /// <summary>
    ///     Schedules an update according to the given update offset.
    ///     Note that the system does not guarantee that the update will be executed exactly at the given offset, as chunks
    ///     could
    ///     be inactive.
    /// </summary>
    /// <param name="world">The world in which the block is.</param>
    /// <param name="position">The position of the block an update should be scheduled for.</param>
    /// <param name="updateOffset">The offset in cycles to when the block should be updated. Must be greater than 0.</param>
    public void ScheduleUpdate(World world, Vector3i position, UInt32 updateOffset)
    {
        Chunk? chunk = world.GetActiveChunk(position);
        chunk?.ScheduleBlockUpdate(new BlockUpdate(position, this, UpdateOperation.Update), updateOffset);
    }

    /// <summary>
    ///     Schedule the block for destruction to happen soon.
    ///     This should be used when destruction was caused by a block update,
    ///     as this destruction could then cause further block updates.
    /// </summary>
    /// <param name="world">The world in which to schedule destruction.</param>
    /// <param name="position">The position at which to schedule destruction.</param>
    public void ScheduleDestroy(World world, Vector3i position)
    {
        Chunk? chunk = world.GetActiveChunk(position);
        chunk?.ScheduleBlockUpdate(new BlockUpdate(position, this, UpdateOperation.Destroy), ScheduledDestroyOffset);
    }

    /// <inheritdoc cref="StateUpdate" />
    public void DoStateUpdate(World world, Vector3i position, Content oldContent, Content newContent)
    {
        if (oldContent.Block.Block != this)
            return;

        StateUpdateMessage message = new(this)
        {
            World = world,
            Position = position,
            OldState = oldContent,
                        NewState = newContent 
        };

        StateUpdate.Publish(message);
    }

    /// <inheritdoc cref="NeighborUpdate" />
    public void DoNeighborUpdate(World world, Vector3i position, State state, Side side)
    {
        NeighborUpdateMessage message = new(this)
        {
            World = world,
            Position = position,
            State = state,
            Side = side
        };

        NeighborUpdate.Publish(message);
    }

    /// <inheritdoc cref="RandomUpdate" />
    public void DoRandomUpdate(World world, Vector3i position, State state)
    {
        RandomUpdateMessage message = new(this)
        {
            World = world,
            Position = position,
            State = state
        };

        RandomUpdate.Publish(message);
    }

    /// <inheritdoc cref="ScheduledUpdate" />
    public void DoScheduledUpdate(World world, Vector3i position, State state)
    {
        ScheduledUpdateMessage message = new(this)
        {
            World = world,
            Position = position,
            State = state
        };

        ScheduledUpdate.Publish(message);
    }

    /// <inheritdoc cref="GeneratorUpdate" />
    public Content DoGeneratorUpdate(Content content)
    {
        GeneratorUpdateMessage message = new(this)
        {
            Content = content
        };

        GeneratorUpdate.Publish(message);

        return content;
    }

    private void BuildBoundingVolumes()
    {
        stateBoundingVolumes = new BoundingVolume[States.Count];
        var uniformBoundingVolumes = true;

        placementBoundingVolume = BoundingVolume.GetValue(Physics.BoundingVolume.Block, States.PlacementDefault);

        foreach ((State state, Int32 index) in States.GetAllStatesWithIndex())
        {
            if (!Constraint.IsStateValid(state))
            {
                stateBoundingVolumes[index] = Physics.BoundingVolume.Block;

                continue;
            }

            BoundingVolume boundingVolume = BoundingVolume.GetValue(Physics.BoundingVolume.Block, state);
            stateBoundingVolumes[index] = boundingVolume;

            uniformBoundingVolumes &= placementBoundingVolume.Equals(boundingVolume);
        }

        if (uniformBoundingVolumes)
            stateBoundingVolumes = null;
    }

    /// <summary>
    ///     Get the bounding volume of the block for the given state.
    /// </summary>
    /// <param name="state">The state of the block.</param>
    /// <returns>The bounding volume of the block in the given state.</returns>
    public BoundingVolume GetBoundingVolume(State state)
    {
        return stateBoundingVolumes != null
            ? stateBoundingVolumes[state.Index]
            : placementBoundingVolume ?? throw Exceptions.InvalidOperation("Bounding volumes are not initialized.");
    }

    /// <summary>
    ///     Returns the collider for a given position.
    ///     If the position does not contain this block, the placement collider is returned.
    /// </summary>
    /// <param name="world">The world in which the block is.</param>
    /// <param name="position">The position of the block.</param>
    /// <returns>The bounding volume.</returns>
    public BoxCollider GetCollider(World world, Vector3i position)
    {
        State? potentialBlock = world.GetBlock(position);

        State state = potentialBlock?.Block == this ? potentialBlock.Value : GetPlacementState(world, position);

        return GetBoundingVolume(state).GetColliderAt(position);
    }

    /// <summary>
    ///     Build all meshes for this block.
    /// </summary>
    /// <param name="textureIndexProvider">The texture index provider to use for the block.</param>
    /// <param name="blockModelProvider">The block model provider to use for the block.</param>
    /// <param name="visuals">The visual configuration to use for the block.</param>
    protected abstract void BuildMeshes(ITextureIndexProvider textureIndexProvider, IBlockModelProvider blockModelProvider, VisualConfiguration visuals);

    /// <summary>
    ///     Mesh this block and add the created mesh data to the context.
    /// </summary>
    /// <param name="position">The position at which the block is meshed, in section-local coordinates.</param>
    /// <param name="state">The state of the block to mesh.</param>
    /// <param name="context">The current meshing context.</param>
    public abstract void Mesh(Vector3i position, State state, MeshingContext context);

    /// <inheritdoc />
    public override String ToString()
    {
        return NamedID;
    }

    /// <summary>
    ///     Sent when an actor collides with this block.
    /// </summary>
    /// <param name="Sender">The block that sent the message.</param>
    public record ActorCollisionMessage(Object Sender) : IEventMessage
    {
        /// <summary>
        ///     The body of the actor that collided with the block.
        /// </summary>
        public Body Body { get; set; } = null!;

        /// <summary>
        ///     The position of the block in the world.
        /// </summary>
        public Vector3i Position { get; set; }

        /// <summary>
        ///     The state of the block at the position.
        /// </summary>
        public State State { get; set; }
    }

    /// <summary>
    ///     Sent when an actor interacts with this block.
    /// </summary>
    /// <param name="Sender">The block that sent the message.</param>
    public record ActorInteractionMessage(Object Sender) : IEventMessage
    {
        /// <summary>
        ///     The actor that interacted with the block.
        /// </summary>
        public Actor Actor { get; set; } = null!;

        /// <summary>
        ///     The position of the block in the world.
        /// </summary>
        public Vector3i Position { get; set; }

        /// <summary>
        ///     The state of the block at the position.
        /// </summary>
        public State State { get; set; }
    }

    /// <summary>
    ///     Sent when the block is actually placed in the world.
    /// </summary>
    /// <param name="Sender">The block that sent the message.</param>
    public record PlacementMessage(Object Sender) : IEventMessage
    {
        /// <summary>
        ///     The world in which the placement occurs.
        /// </summary>
        public World World { get; set; } = null!;

        /// <summary>
        ///     The position at which the placement is requested.
        /// </summary>
        public Vector3i Position { get; set; }

        /// <summary>
        ///     The actor that performs placement.
        /// </summary>
        public Actor? Actor { get; set; }
    }

    /// <summary>
    ///     Sent after the block was placed in the world successfully.
    /// </summary>
    public record PlacementCompletedMessage(Object Sender) : IEventMessage
    {
        /// <summary>
        ///     The world in which the placement was completed.
        /// </summary>
        public World World { get; set; } = null!;

        /// <summary>
        ///     The position at which the block was placed.
        /// </summary>
        public Vector3i Position { get; set; }

        /// <summary>
        ///     The actor that placed the block.
        /// </summary>
        public Actor? Actor { get; set; }
    }

    /// <summary>
    ///     Sent when the block is actually destroyed in the world.
    /// </summary>
    /// <param name="Sender">The block that sent the message.</param>
    public record DestructionMessage(Object Sender) : IEventMessage
    {
        /// <summary>
        ///     The world in which the destruction occurs.
        /// </summary>
        public World World { get; set; } = null!;

        /// <summary>
        ///     The position at which the destruction is requested.
        /// </summary>
        public Vector3i Position { get; set; }

        /// <summary>
        ///     The state of the block at the position.
        /// </summary>
        public State State { get; set; }

        /// <summary>
        ///     The actor that performs destruction.
        /// </summary>
        public Actor? Actor { get; set; }
    }

    /// <summary>
    ///     Sent after the block was destroyed in the world successfully.
    /// </summary>
    public record DestructionCompletedMessage(Object Sender) : IEventMessage
    {
        /// <summary>
        ///     The world in which the destruction was completed.
        /// </summary>
        public World World { get; set; } = null!;

        /// <summary>
        ///     The position at which the block was destroyed.
        /// </summary>
        public Vector3i Position { get; set; }

        /// <summary>
        ///     The state of the block that was destroyed.
        /// </summary>
        public State State { get; set; }

        /// <summary>
        ///     The actor that destroyed the block.
        /// </summary>
        public Actor? Actor { get; set; }

        // todo: find a way to prevent setters being uses outside of this class, maybe interfaces could be used (code generator?)
        // todo: also go through interfaces that have setters that should be used (e.g. Combustible, Plantable) and use Methods there instead, if not better an aspect
    }

    /// <summary>
    ///     Sent when the state of a block changes, including when the fluid at its position changes.
    /// </summary>
    /// <param name="Sender">The block that sent the message.</param>
    public record StateUpdateMessage(Object Sender) : IEventMessage
    {
        /// <summary>
        ///     The world in which the content update occurs.
        /// </summary>
        public World World { get; set; } = null!;

        /// <summary>
        ///     The position of the block.
        /// </summary>
        public Vector3i Position { get; set; }

        /// <summary>
        ///     The old state of this block at the position.
        /// </summary>
        public Content OldState { get; set; }

        /// <summary>
        ///     The new state of this block at the position.
        /// </summary>
        public Content NewState { get; set; }
    }

    /// <summary>
    ///     Sent when a neighboring position is changed.
    /// </summary>
    /// <param name="Sender">The block that sent the message.</param>
    public record NeighborUpdateMessage(Object Sender) : IEventMessage
    {
        /// <summary>
        ///     The world in which the neighbor update occurs.
        /// </summary>
        public World World { get; set; } = null!;

        /// <summary>
        ///     The position of the block.
        /// </summary>
        public Vector3i Position { get; set; }

        /// <summary>
        ///     The state of this, unchanged, block at the position.
        /// </summary>
        public State State { get; set; }

        /// <summary>
        ///     The side of the block where the change happened.
        /// </summary>
        public Side Side { get; set; }
    }

    /// <summary>
    ///     Sent for random updates.
    /// </summary>
    /// <param name="Sender">The block that sent the message.</param>
    public record RandomUpdateMessage(Object Sender) : IEventMessage
    {
        /// <summary>
        ///     The world in which the random update occurs.
        /// </summary>
        public World World { get; set; } = null!;

        /// <summary>
        ///     The position of the block.
        /// </summary>
        public Vector3i Position { get; set; }

        /// <summary>
        ///     The state of the block.
        /// </summary>
        public State State { get; set; }
    }

    /// <summary>
    ///     Sent for scheduled updates.
    /// </summary>
    /// <param name="Sender">The block that sent the message.</param>
    public record ScheduledUpdateMessage(Object Sender) : IEventMessage
    {
        /// <summary>
        ///     The world in which the scheduled update occurs.
        /// </summary>
        public World World { get; set; } = null!;

        /// <summary>
        ///     The position of the block.
        /// </summary>
        public Vector3i Position { get; set; }

        /// <summary>
        ///     The state of the block.
        /// </summary>
        public State State { get; set; }
    }

    /// <summary>
    ///     Sent after the chunk the block is in has been generated.
    /// </summary>
    /// <param name="Sender">The block that sent the message.</param>
    public record GeneratorUpdateMessage(Object Sender) : IEventMessage
    {
        /// <summary>
        ///     The content that is generated, containing this block.
        ///     Subscribers can modify this content.
        /// </summary>
        public Content Content { get; set; }
    }

    /// <summary>
    ///     Called when the block is being initialized.
    /// </summary>
    public class InitializingEventArgs(BlockProperties properties) : EventArgs
    {
        /// <summary>
        ///     The block properties that are being initialized.
        ///     Can be contributed to by the subscribers.
        /// </summary>
        public BlockProperties Properties { get; } = properties;
    }

    internal enum UpdateOperation
    {
        Update,
        Destroy
    }

    internal struct BlockUpdate(Vector3i position, Block target, UpdateOperation operation) : IUpdateable, IEquatable<BlockUpdate>
    {
        private Int32 x = position.X;
        private Int32 y = position.Y;
        private Int32 z = position.Z;

        private UInt32 target = target.ID;
        private UpdateOperation operation = operation;

        public void Update(World world)
        {
            State? potentialBlock = world.GetBlock((x, y, z));

            if (potentialBlock is not {} block) return;
            if (block.Block.ID != target) return;

            switch (operation)
            {
                case UpdateOperation.Update:
                    block.Block.DoScheduledUpdate(world, (x, y, z), block);

                    break;

                case UpdateOperation.Destroy:
                    block.Block.Destroy(world, (x, y, z));

                    break;

                default: throw Exceptions.UnsupportedEnumValue(operation);
            }
        }

        public void Serialize(Serializer serializer)
        {
            serializer.Serialize(ref x);
            serializer.Serialize(ref y);
            serializer.Serialize(ref z);
            serializer.Serialize(ref target);
            serializer.Serialize(ref operation);
        }

        public Boolean Equals(BlockUpdate other)
        {
            return (x, y, z, target, operation) == (other.x, other.y, other.z, other.target, other.operation);
        }

        public override Boolean Equals(Object? obj)
        {
            return obj is BlockUpdate other && Equals(other);
        }

#pragma warning disable S2328
        public override Int32 GetHashCode()
        {
            return HashCode.Combine(x, y, z, target, (Int32) operation);
        }
#pragma warning restore S2328

        public static Boolean operator ==(BlockUpdate left, BlockUpdate right)
        {
            return left.Equals(right);
        }

        public static Boolean operator !=(BlockUpdate left, BlockUpdate right)
        {
            return !(left == right);
        }
    }

    #region DISPOSABLE

    private Boolean disposed;

    /// <summary>
    ///     Override to dispose resources.
    /// </summary>
    /// <param name="disposing">Whether to dispose managed resources.</param>
    protected virtual void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing)
        {
            // Nothing to dispose.
        }
        else
        {
            Throw.ForMissedDispose(this);
        }

        disposed = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~Block()
    {
        Dispose(disposing: false);
    }

    #endregion DISPOSABLE
}
