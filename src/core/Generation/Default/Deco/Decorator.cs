// <copyright file="Decorator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Generation.Default.Deco;

/// <summary>
///     Determines whether to place a decoration at a position.
/// </summary>
public abstract class Decorator
{
    /// <summary>
    ///     Pass a size hint to the decorator.
    /// </summary>
    /// <param name="extents">The extents of the decoration.</param>
    public virtual void SetSizeHint(Vector3i extents) {}

    /// <summary>
    ///     Check whether the decoration should be placed at the given position.
    /// </summary>
    /// <param name="position">The position of the decoration.</param>
    /// <param name="context">The placement context of the decoration.</param>
    /// <param name="grid">The grid in which the position is.</param>
    /// <returns>True if the decoration should be placed, false otherwise.</returns>
    public abstract bool CanPlace(Vector3i position, in Decoration.PlacementContext context, IReadOnlyGrid grid);
}

