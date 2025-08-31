// <copyright file="LateralRotatable.cs" company="VoxelGame">
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
public class LateralRotatable : BlockBehavior, IBehavior<LateralRotatable, BlockBehavior, Block>
{
    private IAttribute<Orientation> Orientation => orientation ?? throw Exceptions.NotInitialized(nameof(orientation));
    private IAttribute<Orientation>? orientation;
    
    private LateralRotatable(Block subject) : base(subject)
    {
        subject.Require<Rotatable>().Rotation.ContributeFunction(GetRotation);

        var siding = subject.Require<SingleSided>();
        siding.Side.ContributeFunction(GetSide);
        siding.SidedState.ContributeFunction(GetSidedState);
    }

    /// <inheritdoc />
    public static LateralRotatable Construct(Block input)
    {
        return new LateralRotatable(input);
    }

    /// <inheritdoc />
    public override void DefineState(IStateBuilder builder)
    {
        orientation = builder.Define(nameof(orientation)).Enum<Orientation>()
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
    
    public Orientation GetOrientation(State state) // todo: remove
    {
        return state.Get(Orientation);
    }

    public State SetOrientation(State state, Orientation newOrientation) // todo: remove
    {
        return state.With(Orientation, newOrientation);
    }
}
