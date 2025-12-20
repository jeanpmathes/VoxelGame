// <copyright file="BoulderDecoration.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
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
    public BoulderDecoration(String name, Decorator decorator) : base(name, decorator, new Sphere {Radius = Diameter / 2.0}, Diameter) {}

    /// <inheritdoc />
    protected override void OnPlace(Vector3i position, IGrid grid, in PlacementContext placementContext)
    {
        grid.SetContent(placementContext.Palette.GetStone(placementContext.StoneType), position);
    }
}
