// <copyright file="CoverPreserving.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Attributes;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Nature;

/// <summary>
///     Marker behavior for blocks that can sit on covered soil without removing the cover.
/// </summary>
public class CoverPreserving : BlockBehavior, IBehavior<CoverPreserving, BlockBehavior, Block>
{
    private CoverPreserving(Block subject) : base(subject)
    {
        Preservation = Aspect<Boolean, State>.New<ANDing<State>>(nameof(Preservation), this);
    }

    /// <summary>
    ///     Aspect that determines whether the block preserves the cover for a given state.
    /// </summary>
    public Aspect<Boolean, State> Preservation { get; }

    /// <inheritdoc />
    public static CoverPreserving Construct(Block input)
    {
        return new CoverPreserving(input);
    }

    /// <summary>
    ///     Check whether the block preserves cover for the supplied state.
    /// </summary>
    /// <param name="state">The state to evaluate.</param>
    /// <returns><c>true</c> if the cover should remain, otherwise <c>false</c>.</returns>
    public Boolean IsPreserving(State state)
    {
        return Preservation.GetValue(original: true, state);
    }
}
