// <copyright file="LateralRotatableComposite.cs" company="VoxelGame">
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
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Orienting;

/// <summary>
///     Provides rotated composite behavior for blocks that are both <see cref="Composite" /> and
///     <see cref="LateralRotatable" />.
/// </summary>
public partial class LateralRotatableComposite : BlockBehavior, IBehavior<LateralRotatableComposite, BlockBehavior, Block>
{
    private readonly Composite composite;
    private readonly LateralRotatable rotatable;

    private LateralRotatableComposite(Block subject) : base(subject)
    {
        composite = subject.Require<Composite>();
        rotatable = subject.Require<LateralRotatable>();

        PartState = Aspect<State, (State, Vector3i)>.New<Exclusive<State, (State, Vector3i)>>(nameof(PartState), this);

        subject.IsPlacementAllowed.ContributeFunction(GetPlacementAllowed);
    }

    /// <summary>
    ///     Provides states where the part position is set to the given part.
    /// </summary>
    public Aspect<State, (State state, Vector3i part)> PartState { get; }

    /// <summary>
    /// Is set by <see cref="Composite"/> and allows this behavior to publish placement completion events.
    /// </summary>
    [LateInitialization] 
    public partial Action<World, Vector3i, Vector3i, Actor?> PublishPlacementCompleted { private get; set; }

    /// <summary>
    /// Is set by <see cref="Composite"/> and allows this behavior to publish neighbor update events.
    /// </summary>
    [LateInitialization] 
    public partial Action<World, Vector3i, Vector3i, State, Side> PublishNeighborUpdate { private get; set; }

    /// <inheritdoc />
    public static LateralRotatableComposite Construct(Block input)
    {
        return new LateralRotatableComposite(input);
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.IPlacementMessage>(OnPlacement);
        bus.Subscribe<Block.IDestructionMessage>(OnDestruction);
        bus.Subscribe<Block.IStateUpdateMessage>(OnStateUpdate);
        bus.Subscribe<Block.INeighborUpdateMessage>(OnNeighborUpdate);
    }

    private Boolean GetPlacementAllowed(Boolean original, (World world, Vector3i position, Actor? actor) context)
    {
        (World world, Vector3i position, Actor? actor) = context;

        State state = Subject.GetPlacementState(world, position, actor);
        Vector3i size = composite.GetSize(state);

        Orientation orientation = rotatable.GetOrientation(state);

        for (var x = 0; x < size.X; x++)
        for (var y = 0; y < size.Y; y++)
        for (var z = 0; z < size.Z; z++)
        {
            Vector3i part = (x, y, z);
            Vector3i current = position + Rotate(part, orientation);

            State? block = world.GetBlock(current);

            if (block?.IsReplaceable != true)
                return false;

            if (!composite.IsPlacementAllowed.GetValue(original: true, (world, current, part, actor)))
                return false;
        }

        return true;
    }

    private void OnPlacement(Block.IPlacementMessage message)
    {
        State state = Subject.GetPlacementState(message.World, message.Position, message.Actor);
        Vector3i size = composite.GetSize(state);

        Orientation orientation = rotatable.GetOrientation(state);

        for (var x = 0; x < size.X; x++)
        for (var y = 0; y < size.Y; y++)
        for (var z = 0; z < size.Z; z++)
        {
            Vector3i part = (x, y, z);
            Vector3i position = message.Position + Rotate(part, orientation);

            state = Subject.GetPlacementState(message.World, position, message.Actor);
            state = SetPartPosition(state, part);

            message.World.SetBlock(state, position);
            
            PublishPlacementCompleted(message.World, position, part, message.Actor);
        }
    }

    private void OnDestruction(Block.IDestructionMessage message)
    {
        Vector3i size = composite.GetSize(message.State);
        Vector3i currentPart = composite.GetPartPosition(message.State);

        Orientation orientation = rotatable.GetOrientation(message.State);
        Vector3i root = message.Position - Rotate(currentPart, orientation);

        for (var x = 0; x < size.X; x++)
        for (var y = 0; y < size.Y; y++)
        for (var z = 0; z < size.Z; z++)
        {
            Vector3i part = (x, y, z);
            Vector3i position = root + Rotate(part, orientation);
            message.World.SetDefaultBlock(position);
        }
    }

    private void OnStateUpdate(Block.IStateUpdateMessage message)
    {
        State oldState = message.OldState.Block;
        State newState = message.NewState.Block;

        if (oldState == newState) return;

        Vector3i oldSize = composite.GetSize(oldState);
        Vector3i newSize = composite.GetSize(newState);

        Vector3i currentPart = composite.GetPartPosition(oldState);
        Orientation orientation = rotatable.GetOrientation(oldState);

        Vector3i root = message.Position - Rotate(currentPart, orientation);

        if (oldSize != newSize)
            ResizeComposite(message.World, root, oldSize, newSize, orientation, newState);
        else if (message.OldState.Block != message.NewState.Block)
            SetStateOnAllParts(message.World, newSize, root, currentPart, orientation, newState);
    }

    private void OnNeighborUpdate(Block.INeighborUpdateMessage message)
    {
        Vector3i size = composite.GetSize(message.State);
        Vector3i currentPart = composite.GetPartPosition(message.State);

        Orientation orientation = rotatable.GetOrientation(message.State);
        Vector3i currentOriented = Rotate(currentPart, orientation);
        Vector3i updatedOriented = message.Side.Offset(currentOriented);
        Vector3i updatedPart = Derotate(updatedOriented, orientation);

        Boolean isPartOfComposite = updatedPart is {X: >= 0, Y: >= 0, Z: >= 0}
                                    && updatedPart.X < size.X && updatedPart.Y < size.Y && updatedPart.Z < size.Z;
        
        if (isPartOfComposite) return;
        
        PublishNeighborUpdate(message.World, message.Position, currentPart, message.State, message.Side);
    }

    private void ResizeComposite(World world, Vector3i position, Vector3i oldSize, Vector3i newSize, Orientation orientation, State state)
    {
        Vector3i size = Vector3i.ComponentMax(oldSize, newSize);

        for (var x = 0; x < size.X; x++)
        for (var y = 0; y < size.Y; y++)
        for (var z = 0; z < size.Z; z++)
        {
            Vector3i part = (x, y, z);
            Boolean inOld = x < oldSize.X && y < oldSize.Y && z < oldSize.Z;
            Boolean inNew = x < newSize.X && y < newSize.Y && z < newSize.Z;

            if (inOld && inNew)
            {
                state = SetPartPosition(state, part);
                world.SetBlock(state, position + Rotate(part, orientation));
            }
            else if (inOld && !inNew)
            {
                world.SetDefaultBlock(position + Rotate(part, orientation));
            }
            else if (!inOld && inNew)
            {
                state = SetPartPosition(state, part);
                world.SetBlock(state, position + Rotate(part, orientation));
            }
        }
    }

    private void SetStateOnAllParts(World world, Vector3i size, Vector3i root, Vector3i exclude, Orientation orientation, State state)
    {
        for (var x = 0; x < size.X; x++)
        for (var y = 0; y < size.Y; y++)
        for (var z = 0; z < size.Z; z++)
        {
            Vector3i part = (x, y, z);

            if (part == exclude) continue;

            state = SetPartPosition(state, part);

            world.SetBlock(state, root + Rotate(part, orientation));
        }
    }

    private static Vector3i Rotate(Vector3i part, Orientation orientation)
    {
        return orientation switch
        {
            Orientation.North => part,
            Orientation.East => new Vector3i(-part.Z, part.Y, part.X),
            Orientation.South => new Vector3i(-part.X, part.Y, -part.Z),
            Orientation.West => new Vector3i(part.Z, part.Y, -part.X),
            _ => part
        };
    }

    private static Vector3i Derotate(Vector3i world, Orientation orientation)
    {
        Orientation inverse = orientation.Opposite();

        return Rotate(world, inverse);
    }

    private State SetPartPosition(State original, Vector3i part)
    {
        return PartState.GetValue(original, (original, part));
    }
}
