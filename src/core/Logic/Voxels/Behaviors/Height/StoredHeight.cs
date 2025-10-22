// <copyright file="StoredHeight.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Attributes;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Height;

/// <summary>
///     Provides utilities to work with stored height attributes.
///     Behaviors that store height information can contribute to the aspect defined here.
/// </summary>
public partial class StoredHeight : BlockBehavior, IBehavior<StoredHeight, BlockBehavior, Block>
{
    [Constructible]
    private StoredHeight(Block subject) : base(subject)
    {
        HeightedState = Aspect<State, BlockHeight>.New<Exclusive<State, BlockHeight>>(nameof(HeightedState), this);
    }

    /// <summary>
    ///     Aspect used to retrieve the state for a given height.
    /// </summary>
    public Aspect<State, BlockHeight> HeightedState { get; }

    /// <summary>
    ///     Get the state with the given height applied.
    /// </summary>
    /// <param name="state">The original state.</param>
    /// <param name="height">The desired height.</param>
    /// <returns>The state with the desired height applied.</returns>
    public State SetHeight(State state, BlockHeight height)
    {
        return HeightedState.GetValue(state, height);
    }
}
