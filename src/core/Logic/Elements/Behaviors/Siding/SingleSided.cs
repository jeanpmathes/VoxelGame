// <copyright file="SingleSided.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Attributes;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Siding;

/// <summary>
/// Unifying behavior for <see cref="Sided"/> blocks that have only one main or front side.
/// </summary>
public class SingleSided : BlockBehavior, IBehavior<SingleSided, BlockBehavior, Block>
{
    /// <summary>
    /// Get the main or front side of the block in a given state.
    /// </summary>
    public Aspect<Side, State> Side { get; }
    
    /// <summary>
    /// Get a state set to the given side, starting from a given other state.
    /// May be <c>null</c> if the given side is not supported.
    /// </summary>
    public Aspect<State?, (State state, Side side)> SidedState { get; }
    
    private SingleSided(Block subject) : base(subject)
    {
        Side = Aspect<Side, State>.New<Exclusive<Side, State>>(nameof(Side), this);
        SidedState = Aspect<State?, (State state, Side side)>.New<Exclusive<State?, (State state, Side side)>>(nameof(SidedState), this);
        
        // todo: require Sided behavior
    }
    
    /// <inheritdoc />
    public static SingleSided Construct(Block input)
    {
        return new SingleSided(input);
    }
    
    public Side GetSide(State state)// todo: remove this i think
    {
        return Side.GetValue(Elements.Side.Front, state);
    }
    
    public State? SetSide(State state, Side side) // todo: remove this i think or rename or so
    { 
        return SidedState.GetValue(state, (state, side));
    }
}
