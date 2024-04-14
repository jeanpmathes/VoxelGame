// <copyright file="PlantableDecorator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
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
    private readonly Vector3i offset;

    /// <summary>
    ///     Creates a new instance of the <see cref="PlantableDecorator" /> class.
    /// </summary>
    public PlantableDecorator()
    {
        offset = Vector3i.Zero;
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="PlantableDecorator" /> class.
    /// </summary>
    /// <param name="offset">An offset to apply to the checked position.</param>
    /// <param name="width">The width of the surface column. See <see cref="SurfaceDecorator" />.</param>
    public PlantableDecorator(Vector3i offset, Int32 width) : base(width)
    {
        this.offset = offset;
    }

    /// <inheritdoc />
    public override Boolean CanPlace(Vector3i position, in Decoration.PlacementContext context, IReadOnlyGrid grid)
    {
        position += offset;

        if (!base.CanPlace(position, context, grid)) return false;

        Content below = grid.GetContent(position.Below()) ?? Content.Default;

        return below.Block.Block is IPlantable;
    }
}
