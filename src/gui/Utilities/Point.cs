// <copyright file="Point.cs" company="VoxelGame">
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
using System.Runtime.InteropServices;

namespace VoxelGame.GUI.Utilities;

/// <summary>
///     Represents a point in 2D space.
/// </summary>
/// <param name="X">The x-coordinate of the point.</param>
/// <param name="Y">The y-coordinate of the point.</param>
[StructLayout(LayoutKind.Sequential)]
public readonly record struct Point(Single X, Single Y)
{
    /// <summary>
    ///     Get an empty point.
    /// </summary>
    public static readonly Point Empty = new(X: 0, Y: 0);

    /// <summary>
    ///     Whether this point is empty.
    /// </summary>
    public Boolean IsEmpty => X == 0 && Y == 0;

    /// <summary>
    ///     Add a size to a point.
    /// </summary>
    public static Point operator +(Point point, Size size)
    {
        return new Point(point.X + size.Width, point.Y + size.Height);
    }

    /// <inheritdoc cref="operator + (Point, Size)" />
    public static Point Add(Point point, Size size)
    {
        return point + size;
    }

    /// <summary>
    ///     Subtract a size from a point.
    /// </summary>
    public static Point operator -(Point point, Size size)
    {
        return new Point(point.X - size.Width, point.Y - size.Height);
    }

    /// <inheritdoc cref="operator - (Point, Size)" />
    public static Point Subtract(Point point, Size size)
    {
        return point - size;
    }

    /// <inheritdoc />
    public override String ToString()
    {
        return this == Empty
            ? "Point.Empty"
            : $"Point(X: {X}, Y: {Y})";
    }
}
