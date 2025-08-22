// <copyright file="FourWayRotatable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Elements.Behaviors.Siding;
using VoxelGame.Core.Utilities;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Orienting;

/// <summary>
/// Allows a block to be rotated in the four cardinal directions.
/// </summary>
public class FourWayRotatable : BlockBehavior, IBehavior<FourWayRotatable, BlockBehavior, Block>
{
    private IAttribute<Utilities.Orientation> Orientation => orientation ?? throw Exceptions.NotInitialized(nameof(orientation));
    private IAttribute<Utilities.Orientation>? orientation;
    
    private FourWayRotatable(Block subject) : base(subject)
    {
        subject.Require<Rotatable>().Rotation.ContributeFunction(GetRotation);

        var siding = subject.Require<SingleSided>();
        siding.Side.ContributeFunction(GetSide);
        siding.SidedState.ContributeFunction(GetSidedState);
    }

    /// <inheritdoc />
    public static FourWayRotatable Construct(Block input)
    {
        return new FourWayRotatable(input);
    }

    /// <inheritdoc />
    public override void DefineState(IStateBuilder builder)
    {
        orientation = builder.Define(nameof(orientation)).Enum<Utilities.Orientation>()
            .Attribute(placementDefault: Utilities.Orientation.South, generationDefault: Utilities.Orientation.South);
    }
    
    private Side GetRotation(Side original, State state)
    {
        return GetOrientation(state).ToSide();
    }
    
    private Side GetSide(Side original, State state)
    {
        return GetOrientation(state).ToSide();
    }
    
    private State? GetSidedState(State? original, (State state, Side side) context)
    {
        (State state, Side side) = context;

        if (!side.IsLateral())
            return null;
        
        return SetOrientation(state, side.ToOrientation());
    }
    
    public Utilities.Orientation GetOrientation(State state) // todo: remove
    {
        return state.Get(Orientation);
    }

    public State SetOrientation(State state, Utilities.Orientation newOrientation) // todo: remove
    {
        return state.With(Orientation, newOrientation);
    }
}
