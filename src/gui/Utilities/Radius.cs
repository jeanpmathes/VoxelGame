// <copyright file="Radius.cs" company="VoxelGame">
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

namespace VoxelGame.GUI.Utilities;

/// <summary>
///     A radius used to draw rounded corners for rectangles.
/// </summary>
public readonly record struct Radius(Single X, Single Y)
{
    /// <summary>
    ///     Create a new radius with uniform X and Y values.
    /// </summary>
    /// <param name="uniform">The uniform value to use for both X and Y.</param>
    public Radius(Single uniform) : this(uniform, uniform) {}

    /// <summary>
    ///     Get a uniformly zero radius, which corresponds to non-rounded corners.
    /// </summary>
    public static Radius Zero { get; } = new(0);

    /// <summary>
    ///     Convert the radius to a <see cref="Size" />.
    /// </summary>
    /// <returns>>A <see cref="Size" /> with the same X and Y values as the radius.</returns>
    public Size ToSize()
    {
        return new Size(X, Y);
    }

    /// <inheritdoc />
    public override String ToString()
    {
        return this == Zero
            ? "Radius.Zero"
            : $"Radius(X: {X}, Y: {Y})";
    }
}
