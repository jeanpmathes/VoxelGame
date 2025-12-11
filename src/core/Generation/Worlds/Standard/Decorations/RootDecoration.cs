// <copyright file="RootDecoration.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Generation.Worlds.Standard.Decorations;

/// <summary>
///     A clump of roots.
/// </summary>
public class RootDecoration : ShapeDecoration
{
    private const Int32 Diameter = 3;

    /// <summary>
    ///     Creates a new instance of the <see cref="RootDecoration" /> class.
    /// </summary>
    public RootDecoration(String name, Decorator decorator) : base(name, decorator, new Sphere {Radius = Diameter / 2.0}, Diameter) {}

    /// <inheritdoc />
    protected override void OnPlace(Vector3i position, IGrid grid, in PlacementContext placementContext)
    {
        if (grid.GetContent(position)?.Block.Block != Blocks.Instance.Environment.Soil)
            return;

        grid.SetContent(Content.CreateGenerated(Blocks.Instance.Environment.Roots), position);
    }
}
