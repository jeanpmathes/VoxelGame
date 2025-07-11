﻿// <copyright file="SurfaceDecorator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Generation.Worlds.Default.Decorations;

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

        return below.Block.IsSolidAndFull;
    }
}
