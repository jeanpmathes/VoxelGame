// <copyright file="StoredMultiSided.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Siding;

public class StoredMultiSided : BlockBehavior, IBehavior<StoredMultiSided, BlockBehavior, Block>
{
    private IAttribute<Sides> Sides => sides ?? throw Exceptions.NotInitialized(nameof(sides));
    private IAttribute<Sides>? sides;
    
    private StoredMultiSided(Block subject) : base(subject)
    {
        subject.Require<Sided>();
    }
    
    /// <inheritdoc/>
    public static StoredMultiSided Construct(Block input)
    {
        return new StoredMultiSided(input);
    }
    
    /// <inheritdoc/>
    public override void DefineState(IStateBuilder builder)
    {
        sides = builder.Define(nameof(sides)).Flags<Sides>().Attribute();
    }
    
    /// <summary>
    /// Get the current sides of the block in the given state.
    /// </summary>
    /// <param name="state">The state to get the sides from.</param>
    /// <returns>The sides of the block in the given state.</returns>
    public Sides GetSides(State state)
    {
        return state.Get(Sides);
    }
    
    /// <summary>
    /// Set the sides of the block in the given state.
    /// </summary>
    /// <param name="state">The state to set the sides in.</param>
    /// <param name="newSides">The sides to set.</param>
    /// <returns>The state with the updated sides.</returns>
    public State SetSides(State state, Sides newSides)
    {
        return state.With(Sides, newSides);
    }
}
