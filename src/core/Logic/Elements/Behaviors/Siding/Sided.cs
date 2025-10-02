// <copyright file="Sided.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Elements.Behaviors.Orienting;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Siding;

/// <summary>
///     Unifying behavior for all blocks that have one or more main or front sides that depend on the block state.
/// </summary>
public class Sided : BlockBehavior, IBehavior<Sided, BlockBehavior, Block>
{
    private Sided(Block subject) : base(subject)
    {
        subject.Require<Orientable>();
        
        Sides = Aspect<Sides, State>.New<Exclusive<Sides, State>>(nameof(Sides), this);
        SidedState = Aspect<State?, (State state, Sides side)>.New<Exclusive<State?, (State state, Sides side)>>(nameof(SidedState), this);
    }

    /// <summary>
    ///     Get the main or front sides of the block in a given state.
    /// </summary>
    public Aspect<Sides, State> Sides { get; }

    /// <summary>
    ///     Get a state set to the given sides, starting from a given other state.
    ///     May be <c>null</c> if the given side is not supported.
    /// </summary>
    public Aspect<State?, (State state, Sides side)> SidedState { get; }

    /// <inheritdoc />
    public static Sided Construct(Block input)
    {
        return new Sided(input);
    }

    /// <summary>
    ///     Get the current main or front sides of the block in the given state.
    /// </summary>
    /// <param name="state">The state to get the sides from.</param>
    /// <returns>The main or front sides of the block in the given state.</returns>
    public Sides GetSides(State state)
    {
        return Sides.GetValue(Elements.Sides.None, state);
    }

    /// <summary>
    ///     Set the main or front sides of the block in the given state.
    /// </summary>
    /// <param name="state">The state to set the sides in.</param>
    /// <param name="sides">The sides to set.</param>
    /// <returns>The state with the updated sides, or <c>null</c> if the sides are not supported.</returns>
    public State? SetSides(State state, Sides sides)
    {
        return SidedState.GetValue(original: null, (state, sides));
    }
}
