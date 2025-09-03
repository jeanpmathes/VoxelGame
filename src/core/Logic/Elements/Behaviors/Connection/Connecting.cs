// <copyright file="Connecting.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Utilities;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Connection;

/// <summary>
/// Connects to other connectable blocks along its lateral sides.
/// </summary>
public class Connecting : BlockBehavior, IBehavior<Connecting, BlockBehavior, Block>
{
    private readonly Connectable connectable;
    
    private IAttribute<Boolean> North => north ?? throw Exceptions.NotInitialized(nameof(north));
    private IAttribute<Boolean>? north;
    
    private IAttribute<Boolean> East => east ?? throw Exceptions.NotInitialized(nameof(east));
    private IAttribute<Boolean>? east;
    
    private IAttribute<Boolean> South => south ?? throw Exceptions.NotInitialized(nameof(south));
    private IAttribute<Boolean>? south;
    
    private IAttribute<Boolean> West => west ?? throw Exceptions.NotInitialized(nameof(west));
    private IAttribute<Boolean>? west;

    private Connecting(Block subject) : base(subject)
    {
        connectable = subject.Require<Connectable>();
        
        subject.PlacementState.ContributeFunction(GetPlacementState);
    }
    
    /// <inheritdoc/>
    public static Connecting Construct(Block input)
    {
        return new Connecting(input);
    }
    
    /// <inheritdoc/>
    public override void DefineState(IStateBuilder builder)
    {
        north = builder.Define(nameof(north)).Boolean().Attribute();
        east = builder.Define(nameof(east)).Boolean().Attribute();
        south = builder.Define(nameof(south)).Boolean().Attribute();
        west = builder.Define(nameof(west)).Boolean().Attribute();
    }

    /// <inheritdoc/>
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.NeighborUpdateMessage>(OnNeighborUpdate);
    }
    
    private State GetPlacementState(State original, (World world, Vector3i position, Actor? actor) context)
    {
        (World world, Vector3i position, Actor? _) = context;
        
        State state = original;
        
        foreach (Orientation orientation in Orientations.All)
        {
            Vector3i neighborPosition = orientation.Offset(position);

            if (CanConnectTo(world, neighborPosition, orientation.ToSide())) 
                state.Set(GetDirection(orientation), value: true);
        }

        return state;
    }
    
    private void OnNeighborUpdate(Block.NeighborUpdateMessage message)
    {
        if (!message.Side.IsLateral()) return;
        
        IAttribute<Boolean> side = GetDirection(message.Side.ToOrientation());
        Boolean canConnect = CanConnectTo(message.World, message.Side.Offset(message.Position), message.Side);
        
        if (message.State.Get(side) == canConnect) return;

        State newState = message.State;
        newState.Set(side, canConnect);
        message.World.SetBlock(new BlockInstance(newState), message.Position);
    }
    
    private Boolean CanConnectTo(World world, Vector3i position, Side side)
    {
        BlockInstance? other = world.GetBlock(position);

        return other?.Block.Get<Connectable>() is {} otherConnectable && otherConnectable.CanConnect(other.Value.State, side.Opposite(), connectable);
    }
    
    private IAttribute<Boolean> GetDirection(Orientation orientation)
    {
        return orientation switch
        {
            Orientation.North => North,
            Orientation.East => East,
            Orientation.South => South,
            Orientation.West => West,
            _ => throw Exceptions.NotInitialized(nameof(orientation))
        };
    }
    
    /// <summary>
    /// Get the connections of the block given its state.
    /// </summary>
    /// <param name="state">The state of the block.</param>
    /// <returns>The connections, <c>true</c> if connected, <c>false</c> if not for each of the lateral sides.</returns>
    public (Boolean north, Boolean east, Boolean south, Boolean west) GetConnections(State state)
    {
        return (state.Get(North), 
            state.Get(East), 
            state.Get(South), 
            state.Get(West));
    }
}
