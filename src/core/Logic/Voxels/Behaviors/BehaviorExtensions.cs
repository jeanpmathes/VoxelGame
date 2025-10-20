// <copyright file="BehaviorExtensions.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels.Behaviors.Height;

namespace VoxelGame.Core.Logic.Voxels.Behaviors;

/// <summary>
///     Extensions that simplify working with behaviors.
/// </summary>
public static class BehaviorExtensions
{
    /// <summary>
    ///     Get the state with the given height applied, if the owning block supports stored height.
    /// </summary>
    /// <param name="state">The original state.</param>
    /// <param name="height">The desired height.</param>
    /// <returns>The state with the desired height if supported, otherwise the original state.</returns>
    public static State WithHeight(this State state, BlockHeight height)
    {
        return state.Block.Get<StoredHeight>() is {} storedHeight ? storedHeight.SetHeight(state, height) : state;
    }
}
