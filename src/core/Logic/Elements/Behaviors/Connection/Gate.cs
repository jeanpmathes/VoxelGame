// <copyright file="Gate.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Logic.Elements.Behaviors.Connection;

/// <summary>
/// Provides the functionality and collision for fence gates.
/// </summary>
public class Gate : BlockBehavior, IBehavior<Gate, BlockBehavior, Block>
{
    private readonly Connectable connectable;
    private readonly LateralRotatable rotatable;
    
    private IAttribute<Boolean> IsOpen => isOpen ?? throw Exceptions.NotInitialized(nameof(isOpen));
    private IAttribute<Boolean>? isOpen;

    private Gate(Block subject) : base(subject)
    {
        subject.Require<Modelled>().Selector.ContributeFunction(GetSelector);
        
        connectable = subject.Require<Connectable>();
        connectable.StrengthInitializer.ContributeConstant(Connectable.Strengths.Wide);
        connectable.IsConnectionAllowed.ContributeFunction(IsConnectionAllowed);
        
        rotatable = subject.Require<LateralRotatable>();
        
        subject.BoundingVolume.ContributeFunction(GetBoundingVolume);
        
        subject.IsPlacementAllowed.ContributeFunction(GetIsPlacementAllowed);
        subject.PlacementState.ContributeFunction(GetPlacementState);
    }

    /// <inheritdoc />
    public static Gate Construct(Block input)
    {
        return new Gate(input);
    }
    
    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.NeighborUpdateMessage>(OnNeighborUpdate);
        bus.Subscribe<Block.ActorInteractionMessage>(OnActorInteraction);
    }

    /// <inheritdoc />
    public override void DefineState(IStateBuilder builder)
    {
        isOpen = builder.Define(nameof(isOpen)).Boolean().Attribute();
    }
    
    private Selector GetSelector(Selector original, State state)
    {
        return original.WithLayer(state.Get(IsOpen) ? 1 : 0);
    }
    
    private Boolean IsConnectionAllowed(Boolean original, (Side side, State state) context)
    {
        (Side side, State state) = context;

        Orientation orientation = rotatable.GetOrientation(state);

        return orientation switch
        {
            Orientation.North => side is Side.Left or Side.Right,
            Orientation.East => side is Side.Front or Side.Back,
            Orientation.South => side is Side.Left or Side.Right,
            Orientation.West => side is Side.Front or Side.Back,
            _ => false
        };
    }
    
    private BoundingVolume GetBoundingVolume(BoundingVolume original, State state)
    {
        Orientation orientation = rotatable.GetOrientation(state);
        Boolean isClosed = !state.Get(IsOpen);

        return orientation switch
        {
            Orientation.North => NorthSouth(offset: 0.375f),
            Orientation.East => WestEast(offset: 0.625f),
            Orientation.South => NorthSouth(offset: 0.625f),
            Orientation.West => WestEast(offset: 0.375f),
            _ => NorthSouth(offset: 0.375f)
        };

        BoundingVolume NorthSouth(Single offset)
        {
            if (isClosed)
                return new BoundingVolume(
                    new Vector3d(x: 0.96875f, y: 0.71875f, z: 0.5f),
                    new Vector3d(x: 0.03125f, y: 0.15625f, z: 0.125f),
                    new BoundingVolume(
                        new Vector3d(x: 0.96875f, y: 0.28125f, z: 0.5f),
                        new Vector3d(x: 0.03125f, y: 0.15625f, z: 0.125f)),
                    new BoundingVolume(
                        new Vector3d(x: 0.03125f, y: 0.71875f, z: 0.5f),
                        new Vector3d(x: 0.03125f, y: 0.15625f, z: 0.125f)),
                    new BoundingVolume(
                        new Vector3d(x: 0.03125f, y: 0.28125f, z: 0.5f),
                        new Vector3d(x: 0.03125f, y: 0.15625f, z: 0.125f)),
                    // Moving parts.
                    new BoundingVolume(
                        new Vector3d(x: 0.75f, y: 0.71875f, z: 0.5f),
                        new Vector3d(x: 0.1875f, y: 0.09375f, z: 0.0625f)),
                    new BoundingVolume(
                        new Vector3d(x: 0.75f, y: 0.28125f, z: 0.5f),
                        new Vector3d(x: 0.1875f, y: 0.09375f, z: 0.0625f)),
                    new BoundingVolume(
                        new Vector3d(x: 0.25f, y: 0.71875f, z: 0.5f),
                        new Vector3d(x: 0.1875f, y: 0.09375f, z: 0.0625f)),
                    new BoundingVolume(
                        new Vector3d(x: 0.25f, y: 0.28125f, z: 0.5f),
                        new Vector3d(x: 0.1875f, y: 0.09375f, z: 0.0625f)));

            return new BoundingVolume(
                new Vector3d(x: 0.96875f, y: 0.71875f, z: 0.5f),
                new Vector3d(x: 0.03125f, y: 0.15625f, z: 0.125f),
                new BoundingVolume(
                    new Vector3d(x: 0.96875f, y: 0.28125f, z: 0.5f),
                    new Vector3d(x: 0.03125f, y: 0.15625f, z: 0.125f)),
                new BoundingVolume(
                    new Vector3d(x: 0.03125f, y: 0.71875f, z: 0.5f),
                    new Vector3d(x: 0.03125f, y: 0.15625f, z: 0.125f)),
                new BoundingVolume(
                    new Vector3d(x: 0.03125f, y: 0.28125f, z: 0.5f),
                    new Vector3d(x: 0.03125f, y: 0.15625f, z: 0.125f)),
                // Moving parts.
                new BoundingVolume(
                    new Vector3d(x: 0.875f, y: 0.71875f, offset),
                    new Vector3d(x: 0.0625f, y: 0.09375f, z: 0.1875f)),
                new BoundingVolume(
                    new Vector3d(x: 0.875f, y: 0.28125f, offset),
                    new Vector3d(x: 0.0625f, y: 0.09375f, z: 0.1875f)),
                new BoundingVolume(
                    new Vector3d(x: 0.125f, y: 0.71875f, offset),
                    new Vector3d(x: 0.0625f, y: 0.09375f, z: 0.1875f)),
                new BoundingVolume(
                    new Vector3d(x: 0.125f, y: 0.28125f, offset),
                    new Vector3d(x: 0.0625f, y: 0.09375f, z: 0.1875f)));
        }

        BoundingVolume WestEast(Single offset)
        {
            if (isClosed)
                return new BoundingVolume(
                    new Vector3d(x: 0.5f, y: 0.71875f, z: 0.96875f),
                    new Vector3d(x: 0.125f, y: 0.15625f, z: 0.03125f),
                    new BoundingVolume(
                        new Vector3d(x: 0.5f, y: 0.28125f, z: 0.96875f),
                        new Vector3d(x: 0.125f, y: 0.15625f, z: 0.03125f)),
                    new BoundingVolume(
                        new Vector3d(x: 0.5f, y: 0.71875f, z: 0.03125f),
                        new Vector3d(x: 0.125f, y: 0.15625f, z: 0.03125f)),
                    new BoundingVolume(
                        new Vector3d(x: 0.5f, y: 0.28125f, z: 0.03125f),
                        new Vector3d(x: 0.125f, y: 0.15625f, z: 0.03125f)),
                    // Moving parts.
                    new BoundingVolume(
                        new Vector3d(x: 0.5f, y: 0.71875f, z: 0.75f),
                        new Vector3d(x: 0.0625f, y: 0.09375f, z: 0.1875f)),
                    new BoundingVolume(
                        new Vector3d(x: 0.5f, y: 0.28125f, z: 0.75f),
                        new Vector3d(x: 0.0625f, y: 0.09375f, z: 0.1875f)),
                    new BoundingVolume(
                        new Vector3d(x: 0.5f, y: 0.71875f, z: 0.25f),
                        new Vector3d(x: 0.0625f, y: 0.09375f, z: 0.1875f)),
                    new BoundingVolume(
                        new Vector3d(x: 0.5f, y: 0.28125f, z: 0.25f),
                        new Vector3d(x: 0.0625f, y: 0.09375f, z: 0.1875f)));

            return new BoundingVolume(
                new Vector3d(x: 0.5f, y: 0.71875f, z: 0.96875f),
                new Vector3d(x: 0.125f, y: 0.15625f, z: 0.03125f),
                new BoundingVolume(
                    new Vector3d(x: 0.5f, y: 0.28125f, z: 0.96875f),
                    new Vector3d(x: 0.125f, y: 0.15625f, z: 0.03125f)),
                new BoundingVolume(
                    new Vector3d(x: 0.5f, y: 0.71875f, z: 0.03125f),
                    new Vector3d(x: 0.125f, y: 0.15625f, z: 0.03125f)),
                new BoundingVolume(
                    new Vector3d(x: 0.5f, y: 0.28125f, z: 0.03125f),
                    new Vector3d(x: 0.125f, y: 0.15625f, z: 0.03125f)),
                // Moving parts.
                new BoundingVolume(
                    new Vector3d(offset, y: 0.71875f, z: 0.875f),
                    new Vector3d(x: 0.1875f, y: 0.09375f, z: 0.0625f)),
                new BoundingVolume(
                    new Vector3d(offset, y: 0.28125f, z: 0.875f),
                    new Vector3d(x: 0.1875f, y: 0.09375f, z: 0.0625f)),
                new BoundingVolume(
                    new Vector3d(offset, y: 0.71875f, z: 0.125f),
                    new Vector3d(x: 0.1875f, y: 0.09375f, z: 0.0625f)),
                new BoundingVolume(
                    new Vector3d(offset, y: 0.28125f, z: 0.125f),
                    new Vector3d(x: 0.1875f, y: 0.09375f, z: 0.0625f)));
        }
    }
    
    private Boolean GetIsPlacementAllowed(Boolean original, (World world, Vector3i position, Actor? actor) context)
    {
        (World world, Vector3i position, Actor? _) = context;
        
        Boolean canConnectOnAxisX = CheckOrientation(world, position, Orientation.East) ||
                           CheckOrientation(world, position, Orientation.West);

        Boolean canConnectOnAxisZ = CheckOrientation(world, position, Orientation.South) ||
                                    CheckOrientation(world, position, Orientation.North);

        return canConnectOnAxisX || canConnectOnAxisZ;
    }
    
    private State GetPlacementState(State original, (World world, Vector3i position, Actor? actor) context)
    {
        (World world, Vector3i position, Actor? actor) = context;
        
        Orientation orientation = actor?.Head?.Forward.ToOrientation() ?? Orientation.North;

        Boolean connectX = CheckOrientation(world, position, Orientation.East) ||
                           CheckOrientation(world, position, Orientation.West);

        Boolean connectZ = CheckOrientation(world, position, Orientation.South) ||
                           CheckOrientation(world, position, Orientation.North);
        
        if (orientation.IsZ() && !connectX || orientation.IsX() && !connectZ) orientation = orientation.Rotate();

        return rotatable.SetOrientation(original, orientation);
    }
    
    private void OnNeighborUpdate(Block.NeighborUpdateMessage message)
    {
        Orientation orientation = rotatable.GetOrientation(message.State);

        if (orientation.Axis() != message.Side.Axis().Rotate()) return;

        Boolean valid =
            CheckOrientation(message.World, message.Position, message.Side.ToOrientation()) ||
            CheckOrientation(message.World, message.Position, message.Side.ToOrientation().Opposite());

        if (!valid) 
            Subject.ScheduleDestroy(message.World, message.Position);
    }
    
    private void OnActorInteraction(Block.ActorInteractionMessage message)
    {
        Orientation orientation = rotatable.GetOrientation(message.State);
        Boolean isClosed = !message.State.Get(IsOpen);
        
        var body = message.Actor.GetComponent<Body>();

        // Check if orientation has to be inverted.
        if (body != null && isClosed &&
            Vector2d.Dot(
                orientation.ToVector3().Xz,
                body.Transform.Position.Xz - new Vector2d(message.Position.X + 0.5, message.Position.Z + 0.5)) > 0)
            orientation = orientation.Opposite();

        Vector3d center = isClosed
            ? new Vector3d(x: 0.5, y: 0.5, z: 0.5) + -orientation.ToVector3() * 0.09375f
            : new Vector3d(x: 0.5, y: 0.5, z: 0.5);

        Double closedOffset = isClosed ? 0.09375 : 0;

        Vector3d extents = orientation is Orientation.North or Orientation.South
            ? new Vector3d(x: 0.5, y: 0.375, 0.125 + closedOffset)
            : new Vector3d(0.125 + closedOffset, y: 0.375, z: 0.5);

        BoundingVolume volume = new(center, extents);

        if (body != null && body.Collider.Intersects(volume.GetColliderAt(message.Position))) return;
        
        isClosed = !isClosed;
        
        message.Actor.World.SetBlock(new BlockInstance(rotatable.SetOrientation(message.State.With(IsOpen, !isClosed), orientation)), message.Position);
    }
    
    private static Boolean CheckOrientation(World world, Vector3i position, Orientation orientation)
    {
        BlockInstance? other = world.GetBlock(orientation.Offset(position));

        return other?.Block.Get<Connectable>() is {} connectable && connectable.CanConnect(other.Value.State, orientation.ToSide().Opposite(), connectable);
    }

}
