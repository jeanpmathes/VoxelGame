// <copyright file="ShapeDecoration.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
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
using System.Diagnostics;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Generation.Worlds.Standard.Decorations;

/// <summary>
///     Base class for decorations that are defined by a 3D shape.
///     It implements the iteration over a cubic area of size <see cref="Size" /> and calls
///     <see cref="OnPlace" /> for every voxel that lies inside the shape.
/// </summary>
public abstract class ShapeDecoration : Decoration
{
    private readonly Shape3D shape;

    /// <summary>
    ///     Creates a new shape based decoration.
    /// </summary>
    protected ShapeDecoration(String name, Decorator decorator, Shape3D shape, Int32 size) : base(name, decorator)
    {
        this.shape = shape;

        Debug.Assert(size is > 0 and <= Section.Size);

        Size = size;
    }

    /// <inheritdoc />
    public override Int32 Size { get; }

    /// <summary>
    ///     The extents (half size) of the cubic area.
    /// </summary>
    private Vector3i Extents => new(Size / 2);

    /// <inheritdoc />
    protected override void DoPlace(Vector3i position, IGrid grid, in PlacementContext placementContext)
    {
        Vector3i extents = Extents;
        Vector3i center = position - extents;

        for (var x = 0; x < Size; x++)
        for (var y = 0; y < Size; y++)
        for (var z = 0; z < Size; z++)
        {
            Vector3i offset = new(x, y, z);
            Vector3i current = center + offset;

            var relative = (Vector3d) (offset - extents);

            if (shape.Contains(relative)) OnPlace(current, grid, placementContext);
        }
    }

    /// <summary>
    ///     Called for every position that lies inside the shape. Implement this to perform the
    ///     actual placement (set content, check neighbors, etc.).
    /// </summary>
    /// <param name="position">The absolute position in the world.</param>
    /// <param name="grid">The grid to modify.</param>
    /// <param name="placementContext">The placement context.</param>
    protected abstract void OnPlace(Vector3i position, IGrid grid, in PlacementContext placementContext);
}
