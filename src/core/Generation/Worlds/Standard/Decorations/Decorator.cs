// <copyright file="Decorator.cs" company="VoxelGame">
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
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Generation.Worlds.Standard.Decorations;

/// <summary>
///     Determines whether to place a decoration at a position.
/// </summary>
public abstract class Decorator
{
    /// <summary>
    ///     Pass a size hint to the decorator.
    /// </summary>
    /// <param name="extents">The extents of the decoration.</param>
    public virtual void SetSizeHint(Vector3i extents) {}

    /// <summary>
    ///     Check whether the decoration should be placed at the given position.
    /// </summary>
    /// <param name="position">The position of the decoration.</param>
    /// <param name="context">The placement context of the decoration.</param>
    /// <param name="grid">The grid in which the position is.</param>
    /// <returns>True if the decoration should be placed, false otherwise.</returns>
    public abstract Boolean CanPlace(Vector3i position, in Decoration.PlacementContext context, IReadOnlyGrid grid);
}
