// <copyright file="Thickness.cs" company="VoxelGame">
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
///     Describes the thickness of a frame around a rectangle.
///     Uses include margins, padding, and borders.
/// </summary>
public readonly record struct Thickness
{
    /// <summary>
    ///     Create a thickness with the specified left, top, right, and bottom thicknesses.
    /// </summary>
    public Thickness(Single left, Single top, Single right, Single bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    /// <summary>
    ///     Create a thickness with the specified uniform thickness for all sides.
    /// </summary>
    public Thickness(Single uniform) : this(uniform, uniform, uniform, uniform) {}

    /// <summary>
    ///     Get a thickness with all sides set to zero.
    /// </summary>
    public static Thickness Zero { get; } = new(0);

    /// <summary>
    ///     Get a thickness with all sides set to one.
    /// </summary>
    public static Thickness One { get; } = new(1);

    /// <summary>
    ///     The left thickness.
    /// </summary>
    public Single Left { get; init; }

    /// <summary>
    ///     The top thickness.
    /// </summary>
    public Single Top { get; init; }

    /// <summary>
    ///     The right thickness.
    /// </summary>
    public Single Right { get; init; }

    /// <summary>
    ///     The bottom thickness.
    /// </summary>
    public Single Bottom { get; init; }

    /// <summary>
    ///     The total width of the thickness, which is the sum of the left and right thicknesses.
    /// </summary>
    public Single Width => Left + Right;

    /// <summary>
    ///     The total height of the thickness, which is the sum of the top and bottom thicknesses.
    /// </summary>
    public Single Height => Top + Bottom;

    /// <summary>
    ///     Add a thickness to a size, resulting in a new size that is increased by the thickness on all sides.
    /// </summary>
    public static Size operator +(Size size, Thickness thickness)
    {
        return new Size(size.Width + thickness.Left + thickness.Right, size.Height + thickness.Top + thickness.Bottom);
    }

    /// <summary>
    ///     Subtract a thickness from a size, resulting in a new size that is decreased by the thickness on all sides.
    /// </summary>
    public static Size operator -(Size size, Thickness thickness)
    {
        return new Size(size.Width - thickness.Left - thickness.Right, size.Height - thickness.Top - thickness.Bottom);
    }

    /// <summary>
    ///     Add a thickness to a rectangle, resulting in a new rectangle that is increased by the thickness on all sides.
    /// </summary>
    public static Rectangle operator +(Rectangle rectangle, Thickness thickness)
    {
        return new Rectangle(rectangle.X - thickness.Left, rectangle.Y - thickness.Top, rectangle.Width + thickness.Left + thickness.Right, rectangle.Height + thickness.Top + thickness.Bottom);
    }

    /// <summary>
    ///     Subtract a thickness from a rectangle, resulting in a new rectangle that is decreased by the thickness on all
    ///     sides.
    /// </summary>
    public static Rectangle operator -(Rectangle rectangle, Thickness thickness)
    {
        return new Rectangle(rectangle.X + thickness.Left, rectangle.Y + thickness.Top, rectangle.Width - thickness.Left - thickness.Right, rectangle.Height - thickness.Top - thickness.Bottom);
    }

    /// <inheritdoc />
    public override String ToString()
    {
        return this == Zero
            ? "Thickness.Zero"
            : $"Thickness(Left: {Left}, Top: {Top}, Right: {Right}, Bottom: {Bottom})";
    }
}
