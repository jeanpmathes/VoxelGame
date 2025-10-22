// <copyright file="StoredMultiSided.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Logic.Attributes;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Siding;

/// <summary>
///     Behavior for <see cref="Sided" /> blocks that can have multiple main or front sides at once, and store them in the
///     block state.
/// </summary>
public partial class StoredMultiSided : BlockBehavior, IBehavior<StoredMultiSided, BlockBehavior, Block>
{
    [Constructible]
    private StoredMultiSided(Block subject) : base(subject)
    {
        var sided = subject.Require<Sided>();
        sided.Sides.ContributeFunction(GetSides);
        sided.SidedState.ContributeFunction(GetSidedState);
    }

    [LateInitialization] private partial IAttribute<Sides> Sides { get; set; }

    /// <inheritdoc />
    public override void DefineState(IStateBuilder builder)
    {
        Sides = builder.Define(nameof(Sides)).Flags<Sides>().Attribute();
    }

    private Sides GetSides(Sides original, State state)
    {
        return GetSides(state);
    }

    private State? GetSidedState(State? original, (State state, Sides sides) context)
    {
        (State state, Sides newSides) = context;

        return SetSides(state, newSides);
    }

    /// <summary>
    ///     Get the current sides of the block in the given state.
    /// </summary>
    /// <param name="state">The state to get the sides from.</param>
    /// <returns>The sides of the block in the given state.</returns>
    public Sides GetSides(State state)
    {
        return state.Get(Sides);
    }

    /// <summary>
    ///     Set the sides of the block in the given state.
    /// </summary>
    /// <param name="state">The state to set the sides in.</param>
    /// <param name="newSides">The sides to set.</param>
    /// <returns>The state with the updated sides.</returns>
    public State SetSides(State state, Sides newSides)
    {
        return state.With(Sides, newSides);
    }
}
