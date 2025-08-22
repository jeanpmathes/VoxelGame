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
    
    private State GetPlacementState(State original, (World world, Vector3i position, Actor? actor) context)
    {
        (World world, Vector3i position, Actor? _) = context;
        
        State state = original;
        
        foreach (Utilities.Orientation orientation in Orientations.All)
        {
            Vector3i neighborPosition = orientation.Offset(position);

            if (world.GetBlock(neighborPosition)?.Block.Get<Connectable>() is {} otherConnectable &&
                Connectable.CanConnect(connectable.Strength, otherConnectable.Strength)) 
                state.Set(GetDirection(orientation), value: true);
        }

        return state;
    }
    
    private IAttribute<Boolean> GetDirection(Utilities.Orientation orientation)
    {
        return orientation switch
        {
            Utilities.Orientation.North => North,
            Utilities.Orientation.East => East,
            Utilities.Orientation.South => South,
            Utilities.Orientation.West => West,
            _ => throw Exceptions.NotInitialized(nameof(orientation))
        };
    }
}
