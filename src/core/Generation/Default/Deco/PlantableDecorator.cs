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
    /// <inheritdoc />
    public override bool CanPlace(Vector3i position, IReadOnlyGrid grid)
    {
        if (!base.CanPlace(position, grid)) return false;

        Content below = grid.GetContent(position.Below()) ?? Content.Default;

        return below.Block.Block is IPlantable;
    }
}
