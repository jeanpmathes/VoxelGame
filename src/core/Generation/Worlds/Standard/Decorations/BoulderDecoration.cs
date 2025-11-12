// <copyright file="BoulderDecoration.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Generation.Worlds.Standard.Decorations;

/// <summary>
///     Places boulders in the world.
/// </summary>
public class BoulderDecoration : ShapeDecoration
{
    private const Int32 Diameter = 5;
    
    /// <summary>
    ///     Creates a new instance of the <see cref="BoulderDecoration" /> class.
    /// </summary>
    public BoulderDecoration(String name, Decorator decorator) : base(name, decorator, new Sphere {Radius = Diameter / 2.0}, Diameter)
    {
    }

    /// <inheritdoc />
    protected override void OnPlace(Vector3i position, IGrid grid, in PlacementContext placementContext)
    {
        grid.SetContent(placementContext.Palette.GetStone(placementContext.StoneType), position);
    }
}
