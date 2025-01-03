// <copyright file="DepthDecorator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Generation.Worlds.Default.Decorations;

/// <summary>
///     Selects positions between a minimum and maximum depth.
/// </summary>
public class DepthDecorator : Decorator
{
    private readonly Int32 maxDepth;
    private readonly Int32 minDepth;

    /// <summary>
    ///     Creates a new depth decorator.
    /// </summary>
    /// <param name="minDepth">The minimum depth.</param>
    /// <param name="maxDepth">The maximum depth, must be greater than the minimum depth.</param>
    public DepthDecorator(Int32 minDepth, Int32 maxDepth)
    {
        this.minDepth = minDepth;
        this.maxDepth = maxDepth;

        Debug.Assert(minDepth < maxDepth);
    }

    /// <inheritdoc />
    public override Boolean CanPlace(Vector3i position, in Decoration.PlacementContext context, IReadOnlyGrid grid)
    {
        return context.Depth >= minDepth && context.Depth <= maxDepth;
    }
}
