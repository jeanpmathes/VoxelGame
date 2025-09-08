// <copyright file="Door.cs" company="VoxelGame">
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
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Elements.Behaviors.Orienting;
using VoxelGame.Core.Logic.Elements.Behaviors.Visuals;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Miscellaneous;

/// <summary>
/// Can be opened and closed, allowing bodies to pass through.
/// </summary>
public class Door : BlockBehavior, IBehavior<Door, BlockBehavior, Block>
{
    private readonly LateralRotatable rotatable;
    
    private IAttribute<Boolean> IsOpen => isOpen ?? throw Exceptions.NotInitialized(nameof(isOpen));
    private IAttribute<Boolean>? isOpen;
    
    private IAttribute<Boolean> IsLeftSided => isLeftSided ?? throw Exceptions.NotInitialized(nameof(isLeftSided));
    private IAttribute<Boolean>? isLeftSided;
    
    private Door(Block subject) : base(subject)
    {
        subject.Require<Modelled>().Selector.ContributeFunction(GetSelector);
        subject.Require<Composite>().MaximumSizeInitializer.ContributeConstant((1, 2, 1));
        subject.Require<Grounded>();

        rotatable = subject.Require<LateralRotatable>();
        subject.Require<LateralRotatableModelled>().OrientationOverride.ContributeFunction(GetOrientationOverride);
        
        subject.BoundingVolume.ContributeFunction(GetBoundingVolume);
        subject.PlacementState.ContributeFunction(GetPlacementState);
    }

    private BoundingVolume GetBoundingVolume(BoundingVolume original, State state)
    {
        Orientation orientation = rotatable.GetOrientation(state);
        
        if (state.Get(IsOpen))
            orientation = state.Get(IsLeftSided) ? orientation.Rotate() : orientation.Rotate().Opposite();

        return orientation switch
        {
            Orientation.North => new BoundingVolume(
                new Vector3d(x: 0.5f, y: 0.5f, z: 0.9375f),
                new Vector3d(x: 0.5f, y: 0.5f, z: 0.0625f)),
            Orientation.East => new BoundingVolume(
                new Vector3d(x: 0.0625f, y: 0.5f, z: 0.5f),
                new Vector3d(x: 0.0625f, y: 0.5f, z: 0.5f)),
            Orientation.South => new BoundingVolume(
                new Vector3d(x: 0.5f, y: 0.5f, z: 0.0625f),
                new Vector3d(x: 0.5f, y: 0.5f, z: 0.0625f)),
            Orientation.West => new BoundingVolume(
                new Vector3d(x: 0.9375f, y: 0.5f, z: 0.5f),
                new Vector3d(x: 0.0625f, y: 0.5f, z: 0.5f)),
            _ => new BoundingVolume(new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f), new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f))
        };
    }

    /// <inheritdoc />
    public static Door Construct(Block input)
    {
        return new Door(input);
    }

    /// <inheritdoc />
    public override void DefineState(IStateBuilder builder)
    {
        isOpen = builder.Define(nameof(isOpen)).Boolean().Attribute();
        isLeftSided = builder.Define(nameof(isLeftSided)).Boolean().Attribute();
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    { 
        bus.Subscribe<Block.ActorInteractionMessage>(OnActorInteract);
    }

    private Selector GetSelector(Selector original, State state)
    {
        return original.WithLayer(state.Get(IsOpen) ? 1 : 0);
    }
    
    private Orientation GetOrientationOverride(Orientation original, State state)
    {
        if (!state.Get(IsOpen)) return original;
        
        return state.Get(IsLeftSided) ? original.Opposite() : original;
    }
    
    private State GetPlacementState(State original, (World world, Vector3i position, Actor? actor) context)
    {
        (World world, Vector3i position, Actor? actor) = context;

        Orientation orientation = actor?.Head?.Forward.ToOrientation() ?? Orientation.North;
        Side side = actor?.GetTargetedSide() ?? Side.Top;

        Boolean leftSided = GetLeftSided(world, position, side, orientation);

        return rotatable.SetOrientation(original.With(IsLeftSided, leftSided), orientation);
    }

    private Boolean GetLeftSided(World world, Vector3i position, Side side, Orientation orientation)
    {
        Boolean leftSided;

        if (side == Side.Top)
        {
            Vector3i neighborPosition = orientation.Rotate().Opposite().Offset(position);
            BlockInstance neighbor = world.GetBlock(neighborPosition) ?? BlockInstance.Default;
            
            leftSided = neighbor.Block != Subject || rotatable.GetOrientation(neighbor.State) != orientation;
        }
        else
        {
            leftSided = orientation.Rotate().Opposite().ToSide() != side;
        }
        
        return leftSided;
    }
    
    private void OnActorInteract(Block.ActorInteractionMessage message)
    {
        ToggleDoor(message.Actor.World, message.Position, message.State);
        
        // if (message.Actor.GetComponent<Body>() is {} body && body.Collider.Intersects(collider)) return;
        // todo: use composite and add a method to it to get the full collider for a given state, merging all parts
        
        Boolean leftSided = message.State.Get(IsLeftSided);
        Boolean wasOpen = message.State.Get(IsOpen);

        Orientation orientation = rotatable.GetOrientation(message.State);

        ToggleNeighbor((leftSided ? orientation : orientation.Opposite()).Rotate().Offset(message.Position));

        void ToggleNeighbor(Vector3i neighborPosition)
        {
            BlockInstance neighbor = message.Actor.World.GetBlock(neighborPosition) ?? BlockInstance.Default;

            if (neighbor.Block == Subject 
                && neighbor.State.Get(IsLeftSided) != leftSided
                && neighbor.State.Get(IsOpen) == wasOpen
                && rotatable.GetOrientation(neighbor.State) == orientation)
                neighbor.Block.Get<Door>()?.ToggleDoor(message.Actor.World, neighborPosition, neighbor.State);
        }
    }

    private void ToggleDoor(World world, Vector3i position, State state)
    {
        State newState = state.With(IsOpen, !state.Get(IsOpen));
        world.SetBlock(new BlockInstance(newState), position);
    }
}
