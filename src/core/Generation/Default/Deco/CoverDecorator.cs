// <copyright file="CoverDecorator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Generation.Default.Deco;

/// <summary>
///     A decorator that selects only surface positions with a certain block.
/// </summary>
public class CoverDecorator : SurfaceDecorator
{
    private readonly Block block;
    private readonly Vector3i offset;

    /// <summary>
    ///     Creates a new instance of the <see cref="PlantableDecorator" /> class.
    /// </summary>
    public CoverDecorator(Block block)
    {
        offset = Vector3i.Zero;
        this.block = block;
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="PlantableDecorator" /> class.
    /// </summary>
    /// <param name="block">The block to check for.</param>
    /// <param name="offset">An offset to apply to the checked position.</param>
    /// <param name="width">The width of the surface column. See <see cref="SurfaceDecorator" />.</param>
    public CoverDecorator(Block block, Vector3i offset, int width) : base(width)
    {
        this.offset = offset;
        this.block = block;
    }

    /// <inheritdoc />
    public override bool CanPlace(Vector3i position, in Decoration.PlacementContext context, IReadOnlyGrid grid)
    {
        position += offset;

        if (!base.CanPlace(position, context, grid)) return false;

        Content below = grid.GetContent(position.Below()) ?? Content.Default;

        return below.Block.Block == block;
    }
}

