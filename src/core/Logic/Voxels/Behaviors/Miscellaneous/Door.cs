// <copyright file="Door.cs" company="VoxelGame">
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
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;
using VoxelGame.Core.Logic.Voxels.Behaviors.Orienting;
using VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Miscellaneous;

/// <summary>
///     Can be opened and closed, allowing bodies to pass through.
/// </summary>
public partial class Door : BlockBehavior, IBehavior<Door, BlockBehavior, Block>
{
    private readonly Composite composite;
    private readonly LateralRotatable rotatable;

    [Constructible]
    private Door(Block subject) : base(subject)
    {
        subject.Require<Modelled>().Selector.ContributeFunction(GetSelector);
        composite = subject.Require<Composite>();
        composite.MaximumSize.Initializer.ContributeConstant((1, 2, 1));
        subject.Require<Grounded>();

        subject.Require<Fillable>();

        rotatable = subject.Require<LateralRotatable>();
        subject.Require<RotatableModelled>().RotationOverride.ContributeFunction(GetRotationOverride);

        subject.BoundingVolume.ContributeFunction(GetBoundingVolume);
        subject.PlacementState.ContributeFunction(GetPlacementState);
    }

    [LateInitialization] private partial IAttribute<Boolean> IsOpen { get; set; }

    [LateInitialization] private partial IAttribute<Boolean> IsLeftSided { get; set; }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.IActorInteractionMessage>(OnActorInteract);
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
    public override void DefineState(IStateBuilder builder)
    {
        IsOpen = builder.Define(nameof(IsOpen)).Boolean().Attribute();
        IsLeftSided = builder.Define(nameof(IsLeftSided)).Boolean().Attribute();
    }

    private Selector GetSelector(Selector original, State state)
    {
        return original.WithLayer(state.Get(IsOpen) ? 1 : 0);
    }

    private (Axis axis, Int32 turns) GetRotationOverride((Axis axis, Int32 turns) original, State state)
    {
        if (original.axis != Axis.Y) return original;

        Int32 turns = original.turns + 2; // Ugly fix because the model is not oriented correctly.
        
        if (state.Get(IsOpen) && state.Get(IsLeftSided))
        {
            turns += 2;
        }

        return (Axis.Y, turns);
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
            Vector3i neighborPosition = position.Offset(orientation.Rotate().Opposite());
            State neighbor = world.GetBlock(neighborPosition) ?? Content.DefaultState;

            leftSided = neighbor.Block != Subject || rotatable.GetOrientation(neighbor) != orientation;
        }
        else
        {
            leftSided = orientation.Rotate().Opposite().ToSide() != side;
        }

        return leftSided;
    }

    private void OnActorInteract(Block.IActorInteractionMessage message)
    {
        Boolean leftSided = message.State.Get(IsLeftSided);
        Boolean wasOpen = message.State.Get(IsOpen);

        var body = message.Actor.GetComponent<Body>();
        
        if (body != null && body.Collider.Intersects(composite.GetFullCollider(message.State.With(IsOpen, !wasOpen), message.Position)))
            return;

        ToggleDoor(message.Actor.World, message.Position, message.State);

        Orientation orientation = rotatable.GetOrientation(message.State);

        ToggleNeighbor(message.Position.Offset((leftSided ? orientation : orientation.Opposite()).Rotate()));

        void ToggleNeighbor(Vector3i neighborPosition)
        {
            State neighbor = message.Actor.World.GetBlock(neighborPosition) ?? Content.DefaultState;
            
            if (neighbor.Block == Subject
                && neighbor.Get(IsLeftSided) != leftSided
                && neighbor.Get(IsOpen) == wasOpen
                && rotatable.GetOrientation(neighbor) == orientation
                && body?.Collider.Intersects(composite.GetFullCollider(neighbor.With(IsOpen, !wasOpen), neighborPosition)) != true)
                neighbor.Block.Get<Door>()?.ToggleDoor(message.Actor.World, neighborPosition, neighbor);
        }
    }

    private void ToggleDoor(World world, Vector3i position, State state)
    {
        State newState = state.With(IsOpen, !state.Get(IsOpen));
        world.SetBlock(newState, position);
    }
}
