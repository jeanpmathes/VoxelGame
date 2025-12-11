// <copyright file="SurfaceDecorator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using OpenTK.Mathematics;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Generation.Worlds.Standard.Decorations;

/// <summary>
///     Selects surface positions for decoration.
///     This decorator selects column areas on the surface of the world.
/// </summary>
public class SurfaceDecorator : Decorator
{
    private readonly Int32 width;
    private Int32 height = 1;

    /// <summary>
    ///     Creates a new surface decorator.
    /// </summary>
    /// <param name="width">The width of the column to check. Must be odd and in the range [1, <see cref="Section.Size" />].</param>
    public SurfaceDecorator(Int32 width = 1)
    {
        Debug.Assert(width is > 0 and <= Section.Size);
        Debug.Assert(width % 2 != 0);

        this.width = width;
    }

    /// <inheritdoc />
    public override void SetSizeHint(Vector3i extents)
    {
        height = extents.Y;
    }

    /// <inheritdoc />
    public override Boolean CanPlace(Vector3i position, in Decoration.PlacementContext context, IReadOnlyGrid grid)
    {
        if (Math.Abs(context.Depth) > 3)
            return false;

        for (var y = 0; y < height; y++)
        for (Int32 x = -width / 2; x <= width / 2; x++)
        for (Int32 z = -width / 2; z <= width / 2; z++)
        {
            Content current = grid.GetContent(position + (x, y, z)) ?? Content.Default;

            if (!current.IsSettable) return false;
        }

        Content below = grid.GetContent(position.Below()) ?? Content.Default;

        return below.Block.IsFullySolid;
    }
}

/// <summary>
///     A specialization of <see cref="SurfaceDecorator" /> that only allows placement on surfaces that have a specific
///     behavior.
/// </summary>
/// <typeparam name="TBelow">The behavior that must be present on the block below the surface.</typeparam>
public class SurfaceDecorator<TBelow> : SurfaceDecorator where TBelow : BlockBehavior, IBehavior<TBelow, BlockBehavior, Block>
{
    private readonly Vector3i offset;

    /// <summary>
    ///     Creates a new instance of the <see cref="SurfaceDecorator{TBelow}" /> class.
    /// </summary>
    public SurfaceDecorator()
    {
        offset = Vector3i.Zero;
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="SurfaceDecorator{TBelow}" /> class.
    /// </summary>
    /// <param name="offset">An offset to apply to the checked position.</param>
    /// <param name="width">The width of the surface column. See <see cref="SurfaceDecorator" />.</param>
    public SurfaceDecorator(Vector3i offset, Int32 width) : base(width)
    {
        this.offset = offset;
    }

    /// <inheritdoc />
    public override Boolean CanPlace(Vector3i position, in Decoration.PlacementContext context, IReadOnlyGrid grid)
    {
        position += offset;

        if (!base.CanPlace(position, context, grid)) return false;

        return grid.GetContent(position.Below())?.Block.Block.Is<TBelow>() == true;
    }
}
