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
using VoxelGame.Core.Logic.Contents;
using VoxelGame.Core.Logic.Voxels.Behaviors;
using VoxelGame.Core.Logic.Voxels.Behaviors.Height;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Serialization;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Voxels;

/// <summary>
///     The basic unit of the game world - a block.
///     Blocks use the flyweight pattern, the world data only stores a state ID.
///     The state ID can be used to retrieve both the type of the block and its state.
/// </summary>
public abstract partial class Block : BehaviorContainer<Block, BlockBehavior>, IIdentifiable<CID>, IIdentifiable<UInt32>, IContent
{
    private const Int32 ScheduledDestroyOffset = 5;

    private Substance substance;

    private BoundingVolume? placementBoundingVolume;
    private Boolean receivesCollisions;
    private BoundingVolume[]? stateBoundingVolumes;

    /// <summary>
    ///     Create a new block.
    /// </summary>
    /// <param name="blockID">The unique integer ID of the block.</param>
    /// <param name="contentID">The content ID of the block. Must be unique.</param>
    /// <param name="name">The name of the block. Can be localized.</param>
    protected Block(UInt32 blockID, CID contentID, String name)
    {
        Name = name;
        BlockID = blockID;
        ContentID = contentID;

        BoundingVolume = Aspect<BoundingVolume, State>.New<Exclusive<BoundingVolume, State>>(nameof(BoundingVolume), this);
        PlacementState = Aspect<State, (World, Vector3i, Actor?)>.New<Chaining<State, (World, Vector3i, Actor?)>>(nameof(PlacementState), this);

        IsPlacementAllowed = Aspect<Boolean, (World, Vector3i, Actor?)>.New<ANDing<(World, Vector3i, Actor?)>>(nameof(IsPlacementAllowed), this);
        IsDestructionAllowed = Aspect<Boolean, (State, World, Vector3i, Actor?)>.New<ANDing<(State, World, Vector3i, Actor?)>>(nameof(IsDestructionAllowed), this);
    }

    /// <summary>
    ///     The states of the block.
    /// </summary>
    public StateSet States { get; private set; } = null!;

    [LateInitialization] private partial IEvent<IActorCollisionMessage> ActorCollision { get; set; }

    [LateInitialization] private partial IEvent<IActorInteractionMessage> ActorInteraction { get; set; }

    [LateInitialization] private partial IEvent<IPlacementMessage> Placement { get; set; }

    [LateInitialization] private partial IEvent<IPlacementCompletedMessage> PlacementCompleted { get; set; }

    [LateInitialization] private partial IEvent<IDestructionMessage> Destruction { get; set; }

    [LateInitialization] private partial IEvent<IDestructionCompletedMessage> DestructionCompleted { get; set; }

    [LateInitialization] private partial IEvent<IStateUpdateMessage> StateUpdate { get; set; }

    [LateInitialization] private partial IEvent<INeighborUpdateMessage> NeighborUpdate { get; set; }

    [LateInitialization] private partial IEvent<IRandomUpdateMessage> RandomUpdate { get; set; }

    [LateInitialization] private partial IEvent<IScheduledUpdateMessage> ScheduledUpdate { get; set; }

    [LateInitialization] private partial IEvent<IGeneratorUpdateMessage> GeneratorUpdate { get; set; }

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
    public UInt32 BlockID { get; }
    
    /// <summary>
    /// The named ID of the block.
    /// </summary>
    public CID ContentID { get; }

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
    public RID Identifier => ContentID.GetResourceID<Block>();

    /// <inheritdoc />
    public ResourceType Type => ResourceTypes.Block;

    /// <inheritdoc />
    CID IIdentifiable<CID>.ID => ContentID;

    /// <inheritdoc />
    UInt32 IIdentifiable<UInt32>.ID => BlockID;
    
    /// <inheritdoc />
    CID IContent.ID => ContentID;

    /// <summary>
    ///     Initialize the block with its states and internal values.
    ///     Must be called before the block behavior system is baked.
    /// </summary>
    /// <param name="offset">The number of already existing block states.</param>
    /// <param name="validator">The validator to use for validation.</param>
    /// <returns>The number of states this block has.</returns>
    internal UInt32 Initialize(UInt32 offset, Validator validator)
    {
        EnsureNotBaked();

        InitializeProperties();

        StateBuilder builder = new(validator);

        foreach (BlockBehavior behavior in Behaviors)
        {
            validator.SetScope(behavior);
            builder.Enclose(Reflections.GetLongName(behavior.GetType()), behavior.DefineState);
        }
        
        validator.SetScope(this);

        States = builder.Build(this, offset);

        return States.Count;
    }

    /// <inheritdoc />
    public sealed override void DefineEvents(IEventRegistry registry)
    {
        ActorCollision = registry.RegisterEvent<IActorCollisionMessage>();
        ActorInteraction = registry.RegisterEvent<IActorInteractionMessage>();

        Placement = registry.RegisterEvent<IPlacementMessage>(single: true);
        PlacementCompleted = registry.RegisterEvent<IPlacementCompletedMessage>();

        Destruction = registry.RegisterEvent<IDestructionMessage>(single: true);
        DestructionCompleted = registry.RegisterEvent<IDestructionCompletedMessage>();

        StateUpdate = registry.RegisterEvent<IStateUpdateMessage>();
        NeighborUpdate = registry.RegisterEvent<INeighborUpdateMessage>();
        RandomUpdate = registry.RegisterEvent<IRandomUpdateMessage>();
        ScheduledUpdate = registry.RegisterEvent<IScheduledUpdateMessage>();
        GeneratorUpdate = registry.RegisterEvent<IGeneratorUpdateMessage>(single: true);
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
        substance = properties.Substance.GetValue(Substance.Normal, this);
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

        OnValidate(validator);

        base.Validate(validator);
    }

    /// <summary>
    ///     Override this method to perform additional validation.
    /// </summary>
    /// <param name="validator"></param>
    protected abstract void OnValidate(IValidator validator);

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
    /// <param name="modelProvider">The model provider to use for the block.</param>
    /// <param name="visuals">The visual configuration to use for the block.</param>
    /// <param name="validator">The validator to use for validation.</param>
    public void Activate(ITextureIndexProvider textureIndexProvider, IModelProvider modelProvider, VisualConfiguration visuals, IValidator validator)
    {
        BuildBoundingVolumes();
        BuildMeshes(textureIndexProvider, modelProvider, visuals, validator);
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
            return height.GetHeight(state).IsFull;

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
            return height.GetHeight(state).IsFull || height.IsSideFull(side, state);

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
        return substance is Substance.Replaceable or Substance.Empty;
    }
    
    /// <summary>
    /// Get whether the block is fully empty, meaning it has no substance.
    /// This also implies that the block is replaceable in any state.
    /// </summary>
    public Boolean IsEmpty => substance == Substance.Empty;

    /// <summary>
    ///     This method is called when an actor collides with this block.
    /// </summary>
    /// <param name="body">The body of the actor.</param>
    /// <param name="position">The block position.</param>
    public void OnActorCollision(Body body, Vector3i position)
    {
        State? potentialBlock = body.Subject.World.GetBlock(position);

        if (potentialBlock?.Block != this) return;

        ActorCollisionMessage actorCollision = IEventMessage<ActorCollisionMessage>.Pool.Get();

        {
            actorCollision.Body = body;
            actorCollision.Position = position;
            actorCollision.State = potentialBlock.Value;
        }

        ActorCollision.Publish(actorCollision);
        
        IEventMessage<ActorCollisionMessage>.Pool.Return(actorCollision);
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

        ActorInteractionMessage actorInteraction = IEventMessage<ActorInteractionMessage>.Pool.Get();

        {
            actorInteraction.Actor = actor;
            actorInteraction.Position = position;
            actorInteraction.State = potentialBlock.Value;
        }

        ActorInteraction.Publish(actorInteraction);
        
        IEventMessage<ActorInteractionMessage>.Pool.Return(actorInteraction);
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

        State placementState = GetPlacementState(world, position, actor);
        
        if (Placement.HasSubscribers)
        {
            PlacementMessage placement = IEventMessage<PlacementMessage>.Pool.Get();

            {
                placement.World = world;
                placement.Position = position;
                placement.Actor = actor;
                placement.PlacementState = placementState;
            }

            Placement.Publish(placement);
            
            IEventMessage<PlacementMessage>.Pool.Return(placement);
        }
        else
        {
            world.SetBlock(placementState, position);
        }

        PlacementCompletedMessage placementCompleted = IEventMessage<PlacementCompletedMessage>.Pool.Get();

        {
            placementCompleted.World = world;
            placementCompleted.Position = position;
            placementCompleted.Actor = actor;
        }
        
        PlacementCompleted.Publish(placementCompleted);
        
        IEventMessage<PlacementCompletedMessage>.Pool.Return(placementCompleted);

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
            DestructionMessage destruction = IEventMessage<DestructionMessage>.Pool.Get();
            
            {
                destruction.World = world;
                destruction.Position = position;
                destruction.State = block;
                destruction.Actor = actor;
            }

            Destruction.Publish(destruction);
            
            IEventMessage<DestructionMessage>.Pool.Return(destruction);
        }
        else
        {
            world.SetDefaultBlock(position);
        }
        
        DestructionCompletedMessage destructionCompleted = IEventMessage<DestructionCompletedMessage>.Pool.Get();
        
        {
            destructionCompleted.World = world;
            destructionCompleted.Position = position;
            destructionCompleted.State = block;
            destructionCompleted.Actor = actor;
        }
        
        DestructionCompleted.Publish(destructionCompleted);
        
        IEventMessage<DestructionCompletedMessage>.Pool.Return(destructionCompleted);

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
        if (!StateUpdate.HasSubscribers) return;
        
        if (oldContent.Block.Block != this)
            return;
        
        StateUpdateMessage stateUpdate = IEventMessage<StateUpdateMessage>.Pool.Get();

        {
            stateUpdate.World = world;
            stateUpdate.Position = position;
            stateUpdate.OldState = oldContent;
            stateUpdate.NewState = newContent;
        }
        
        StateUpdate.Publish(stateUpdate);
        
        IEventMessage<StateUpdateMessage>.Pool.Return(stateUpdate);
    }

    /// <inheritdoc cref="NeighborUpdate" />
    public void DoNeighborUpdate(World world, Vector3i position, State state, Side side)
    {
        if (!NeighborUpdate.HasSubscribers) return;
        
        NeighborUpdateMessage neighborUpdate = IEventMessage<NeighborUpdateMessage>.Pool.Get();

        {
            neighborUpdate.World = world;
            neighborUpdate.Position = position;
            neighborUpdate.State = state;
            neighborUpdate.Side = side;
        }
        
        NeighborUpdate.Publish(neighborUpdate);
        
        IEventMessage<NeighborUpdateMessage>.Pool.Return(neighborUpdate);
    }

    /// <inheritdoc cref="RandomUpdate" />
    public void DoRandomUpdate(World world, Vector3i position, State state)
    {
        if (!RandomUpdate.HasSubscribers) return;

        RandomUpdateMessage randomUpdate = IEventMessage<RandomUpdateMessage>.Pool.Get();

        {
            randomUpdate.World = world;
            randomUpdate.Position = position;
            randomUpdate.State = state;
        }

        RandomUpdate.Publish(randomUpdate);
            
        IEventMessage<RandomUpdateMessage>.Pool.Return(randomUpdate);
    }

    /// <inheritdoc cref="ScheduledUpdate" />
    public void DoScheduledUpdate(World world, Vector3i position, State state)
    {
        if (!ScheduledUpdate.HasSubscribers) return;

        ScheduledUpdateMessage scheduledUpdate = IEventMessage<ScheduledUpdateMessage>.Pool.Get();

        {
            scheduledUpdate.World = world;
            scheduledUpdate.Position = position;
            scheduledUpdate.State = state;
        }

        ScheduledUpdate.Publish(scheduledUpdate);
            
        IEventMessage<ScheduledUpdateMessage>.Pool.Return(scheduledUpdate);
    }

    /// <inheritdoc cref="GeneratorUpdate" />
    public Content DoGeneratorUpdate(Content content)
    {
        if (!GeneratorUpdate.HasSubscribers) return content;

        GeneratorUpdateMessage generatorUpdate = IEventMessage<GeneratorUpdateMessage>.Pool.Get();

        {
            generatorUpdate.Content = content;
        }

        GeneratorUpdate.Publish(generatorUpdate);
        
        content = generatorUpdate.Content;
        
        IEventMessage<GeneratorUpdateMessage>.Pool.Return(generatorUpdate);

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
    /// <param name="modelProvider">The model provider to use for the block.</param>
    /// <param name="visuals">The visual configuration to use for the block.</param>
    /// <param name="validator"></param>
    protected abstract void BuildMeshes(ITextureIndexProvider textureIndexProvider, IModelProvider modelProvider, VisualConfiguration visuals, IValidator validator);

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
        return ContentID.Identifier;
    }

    /// <summary>
    ///     Sent when an actor collides with this block.
    /// </summary>
    [GenerateRecord(typeof(IEventMessage<>))]
    public interface IActorCollisionMessage : IEventMessage
    {
        /// <summary>
        ///     The body of the actor that collided with the block.
        /// </summary>
        public Body Body { get; }

        /// <summary>
        ///     The position of the block in the world.
        /// </summary>
        public Vector3i Position { get; }

        /// <summary>
        ///     The state of the block at the position.
        /// </summary>
        public State State { get; }
    }

    /// <summary>
    ///     Sent when an actor interacts with this block.
    /// </summary>
    [GenerateRecord(typeof(IEventMessage<>))]
    public interface IActorInteractionMessage : IEventMessage
    {
        /// <summary>
        ///     The actor that interacted with the block.
        /// </summary>
        public Actor Actor { get; }

        /// <summary>
        ///     The position of the block in the world.
        /// </summary>
        public Vector3i Position { get; }

        /// <summary>
        ///     The state of the block at the position.
        /// </summary>
        public State State { get; }
    }

    /// <summary>
    ///     Sent when the block is actually placed in the world.
    /// </summary>
    [GenerateRecord(typeof(IEventMessage<>))]
    public interface IPlacementMessage : IEventMessage
    {
        /// <summary>
        ///     The world in which the placement occurs.
        /// </summary>
        public World World { get; }

        /// <summary>
        ///     The position at which the placement is requested.
        /// </summary>
        public Vector3i Position { get; }

        /// <summary>
        ///     The actor that performs placement.
        /// </summary>
        public Actor? Actor { get; }
        
        /// <summary>
        /// The state this block would be placed as if there were no event subscribers.
        /// </summary>
        public State PlacementState { get; }
    }

    /// <summary>
    ///     Sent after the block was placed in the world successfully.
    /// </summary>
    [GenerateRecord(typeof(IEventMessage<>))]
    public interface IPlacementCompletedMessage : IEventMessage
    {
        /// <summary>
        ///     The world in which the placement was completed.
        /// </summary>
        public World World { get; }

        /// <summary>
        ///     The position at which the block was placed.
        /// </summary>
        public Vector3i Position { get; }

        /// <summary>
        ///     The actor that placed the block.
        /// </summary>
        public Actor? Actor { get; }
    }

    /// <summary>
    ///     Sent when the block is actually destroyed in the world.
    /// </summary>
    [GenerateRecord(typeof(IEventMessage<>))]
    public interface IDestructionMessage : IEventMessage
    {
        /// <summary>
        ///     The world in which the destruction occurs.
        /// </summary>
        public World World { get; }

        /// <summary>
        ///     The position at which the destruction is requested.
        /// </summary>
        public Vector3i Position { get; }

        /// <summary>
        ///     The state of the block at the position.
        /// </summary>
        public State State { get; }

        /// <summary>
        ///     The actor that performs destruction.
        /// </summary>
        public Actor? Actor { get; set; }
    }

    /// <summary>
    ///     Sent after the block was destroyed in the world successfully.
    /// </summary>
    [GenerateRecord(typeof(IEventMessage<>))]
    public interface IDestructionCompletedMessage : IEventMessage
    {
        /// <summary>
        ///     The world in which the destruction was completed.
        /// </summary>
        public World World { get; }

        /// <summary>
        ///     The position at which the block was destroyed.
        /// </summary>
        public Vector3i Position { get; }

        /// <summary>
        ///     The state of the block that was destroyed.
        /// </summary>
        public State State { get; }

        /// <summary>
        ///     The actor that destroyed the block.
        /// </summary>
        public Actor? Actor { get; }
    }

    /// <summary>
    ///     Sent when the state of a block changes, including when the fluid at its position changes.
    /// </summary>
    [GenerateRecord(typeof(IEventMessage<>))]
    public interface IStateUpdateMessage : IEventMessage
    {
        /// <summary>
        ///     The world in which the content update occurs.
        /// </summary>
        public World World { get; }

        /// <summary>
        ///     The position of the block.
        /// </summary>
        public Vector3i Position { get; }

        /// <summary>
        ///     The old state of this block at the position.
        /// </summary>
        public Content OldState { get; }

        /// <summary>
        ///     The new state of this block at the position.
        /// </summary>
        public Content NewState { get; }
    }

    /// <summary>
    ///     Sent when a neighboring position is changed.
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
        ///     The state of this, unchanged, block at the position.
        /// </summary>
        public State State { get; }

        /// <summary>
        ///     The side of the block where the change happened.
        /// </summary>
        public Side Side { get; }
    }

    /// <summary>
    ///     Sent for random updates.
    /// </summary>
    [GenerateRecord(typeof(IEventMessage<>))]
    public interface IRandomUpdateMessage : IEventMessage
    {
        /// <summary>
        ///     The world in which the random update occurs.
        /// </summary>
        public World World { get; }

        /// <summary>
        ///     The position of the block.
        /// </summary>
        public Vector3i Position { get; }

        /// <summary>
        ///     The state of the block.
        /// </summary>
        public State State { get; }
    }

    /// <summary>
    ///     Sent for scheduled updates.
    /// </summary>
    [GenerateRecord(typeof(IEventMessage<>))]
    public interface IScheduledUpdateMessage : IEventMessage
    {
        /// <summary>
        ///     The world in which the scheduled update occurs.
        /// </summary>
        public World World { get; }

        /// <summary>
        ///     The position of the block.
        /// </summary>
        public Vector3i Position { get; }

        /// <summary>
        ///     The state of the block.
        /// </summary>
        public State State { get; }
    }

    /// <summary>
    ///     Sent after the chunk the block is in has been generated.
    /// </summary>
    [GenerateRecord(typeof(IEventMessage<>))]
    public interface IGeneratorUpdateMessage : IEventMessage
    {
        /// <summary>
        ///     The content that is generated, containing this block.
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

        private UInt32 target = target.BlockID;
        private UpdateOperation operation = operation;

        public void Update(World world)
        {
            State? potentialBlock = world.GetBlock((x, y, z));

            if (potentialBlock is not {} block) return;
            if (block.Block.BlockID != target) return;

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
