// <copyright file="RadiusF.cs" company="VoxelGame">
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
using System.Drawing;

namespace VoxelGame.GUI.Utilities;

/// <summary>
///     A radius used to draw rounded corners for rectangles.
/// </summary>
public readonly record struct RadiusF
{
    /// <summary>
    ///     Create a new radius with the specified X and Y values.
    /// </summary>
    /// <param name="x">The x value to use.</param>
    /// <param name="y">The y value to use.</param>
    public RadiusF(Single x, Single y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    ///     Create a new radius with uniform X and Y values.
    /// </summary>
    /// <param name="uniform">The uniform value to use for both X and Y.</param>
    public RadiusF(Single uniform) : this(uniform, uniform) {}

    /// <summary>
    ///     Get a uniformly zero radius, which corresponds to non-rounded corners.
    /// </summary>
    public static RadiusF Zero { get; } = new(0);

    /// <summary>
    ///     The radius of the corners on the X axis.
    /// </summary>
    public Single X { get; }

    /// <summary>
    ///     The radius of the corner on the Y axis.
    /// </summary>
    public Single Y { get; }

    /// <summary>
    ///     Convert the radius to a <see cref="SizeF" />.
    /// </summary>
    /// <returns>>A <see cref="SizeF" /> with the same X and Y values as the radius.</returns>
    public SizeF ToSizeF()
    {
        return new SizeF(X, Y);
    }

    /// <inheritdoc />
    public override String ToString()
    {
        return this == Zero
            ? "RadiusF.Zero"
            : $"RadiusF(X: {X}, Y: {Y})";
    }
}
