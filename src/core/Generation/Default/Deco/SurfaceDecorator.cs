// <copyright file="SurfaceDecorator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Generation.Default.Deco;

/// <summary>
///     Selects surface positions for decoration.
/// </summary>
public class SurfaceDecorator : Decorator
{
    private int height = 1;

    /// <inheritdoc />
    public override void SetSizeHint(Vector3i extents)
    {
        height = extents.Y;
    }

    /// <inheritdoc />
    public override bool CanPlace(Vector3i position, IReadOnlyGrid grid)
    {
        for (var y = 0; y < height; y++)
        {
            Content current = grid.GetContent(position + (0, y, 0)) ?? Content.Default;

            if (!current.Block.Block.IsReplaceable) return false;
        }

        Content below = grid.GetContent(position.Below()) ?? Content.Default;

        if (!below.Block.IsSolidAndFull) return false;

        return true;
    }
}
