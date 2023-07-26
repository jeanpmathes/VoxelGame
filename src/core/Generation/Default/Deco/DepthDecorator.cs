// <copyright file="DepthDecorator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Diagnostics;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Generation.Default.Deco;

/// <summary>
///     Selects positions between a minimum and maximum depth.
/// </summary>
public class DepthDecorator : Decorator
{
    private readonly int maxDepth;
    private readonly int minDepth;

    /// <summary>
    ///     Creates a new depth decorator.
    /// </summary>
    /// <param name="minDepth">The minimum depth.</param>
    /// <param name="maxDepth">The maximum depth, must be greater than the minimum depth.</param>
    public DepthDecorator(int minDepth, int maxDepth)
    {
        this.minDepth = minDepth;
        this.maxDepth = maxDepth;

        Debug.Assert(minDepth < maxDepth);
    }

    /// <inheritdoc />
    public override bool CanPlace(Vector3i position, in Decoration.PlacementContext context, IReadOnlyGrid grid)
    {
        return context.Depth >= minDepth && context.Depth <= maxDepth;
    }
}
