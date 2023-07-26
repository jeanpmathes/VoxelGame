// <copyright file="SurfaceDecorator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Diagnostics;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Generation.Default.Deco;

/// <summary>
///     Selects surface positions for decoration.
///     This decorator selects column areas on the surface of the world.
/// </summary>
public class SurfaceDecorator : Decorator
{
    private readonly int width;
    private int height = 1;

    /// <summary>
    ///     Creates a new surface decorator.
    /// </summary>
    /// <param name="width">The width of the column to check. Must be odd and in the range [1, <see cref="Section.Size" />].</param>
    public SurfaceDecorator(int width = 1)
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
    public override bool CanPlace(Vector3i position, in Decoration.PlacementContext context, IReadOnlyGrid grid)
    {
        for (var y = 0; y < height; y++)
        for (int x = -width / 2; x <= width / 2; x++)
        for (int z = -width / 2; z <= width / 2; z++)
        {
            Content current = grid.GetContent(position + (x, y, z)) ?? Content.Default;

            if (!current.IsSettable) return false;
        }

        Content below = grid.GetContent(position.Below()) ?? Content.Default;

        return below.Block.IsSolidAndFull;
    }
}
