// <copyright file="PlantableDecorator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Generation.Default.Deco;

/// <summary>
///     A decorator that only selects surface blocks that are plantable.
/// </summary>
public class PlantableDecorator : SurfaceDecorator
{
    /// <summary>
    ///     Creates a new instance of the <see cref="PlantableDecorator" /> class.
    /// </summary>
    public PlantableDecorator() {}

    /// <summary>
    ///     Creates a new instance of the <see cref="PlantableDecorator" /> class.
    /// </summary>
    /// <param name="width">The width of the surface column. See <see cref="SurfaceDecorator" />.</param>
    public PlantableDecorator(int width) : base(width) {}

    /// <inheritdoc />
    public override bool CanPlace(Vector3i position, IReadOnlyGrid grid)
    {
        if (!base.CanPlace(position, grid)) return false;

        Content below = grid.GetContent(position.Below()) ?? Content.Default;

        return below.Block.Block is IPlantable;
    }
}
