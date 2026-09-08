// <copyright file="Rectangle.cs" company="VoxelGame">
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
///     Represents a rectangle in 2D space, which has both a position and a size.
/// </summary>
/// <param name="X">The x-coordinate of the top-left corner.</param>
/// <param name="Y">The y-coordinate of the top-left corner.</param>
/// <param name="Width">The width of the rectangle.</param>
/// <param name="Height">The height of the rectangle.</param>
[StructLayout(LayoutKind.Sequential)]
public readonly record struct Rectangle(Single X, Single Y, Single Width, Single Height)
{
    /// <summary>
    ///     Gets an empty rectangle.
    /// </summary>
    public static readonly Rectangle Empty = new(X: 0, Y: 0, Width: 0, Height: 0);

    /// <summary>
    ///     Create a rectangle from location and size.
    /// </summary>
    /// <param name="location">The top-left location.</param>
    /// <param name="size">The rectangle size.</param>
    public Rectangle(Point location, Size size) : this(location.X, location.Y, size.Width, size.Height) {}

    /// <summary>
    ///     Create a rectangle from coordinates and size.
    /// </summary>
    /// <param name="x">The top-left x-coordinate.</param>
    /// <param name="y">The top-left y-coordinate.</param>
    /// <param name="size">The rectangle size.</param>
    public Rectangle(Single x, Single y, Size size) : this(x, y, size.Width, size.Height) {}

    /// <summary>
    ///     Create a rectangle from location and dimensions.
    /// </summary>
    /// <param name="location">The top-left location.</param>
    /// <param name="width">The rectangle width.</param>
    /// <param name="height">The rectangle height.</param>
    public Rectangle(Point location, Single width, Single height) : this(location.X, location.Y, width, height) {}

    /// <summary>
    ///     Gets whether this rectangle is empty.
    /// </summary>
    public Boolean IsEmpty => Width <= 0 || Height <= 0;

    /// <summary>
    ///     Gets the top-left location.
    /// </summary>
    public Point Location
    {
        get => new(X, Y);

        init
        {
            X = value.X;
            Y = value.Y;
        }
    }

    /// <summary>
    ///     Gets the rectangle size.
    /// </summary>
    public Size Size
    {
        get => new(Width, Height);

        init
        {
            Width = value.Width;
            Height = value.Height;
        }
    }

    /// <summary>
    ///     Gets the x-coordinate of the top-left corner.
    /// </summary>
    public Single Left => X;

    /// <summary>
    ///     Gets the y-coordinate of the top-left corner.
    /// </summary>
    public Single Top => Y;

    /// <summary>
    ///     Gets the x-coordinate of the bottom-right corner.
    /// </summary>
    public Single Right => X + Width;

    /// <summary>
    ///     Gets the y-coordinate of the bottom-right corner.
    /// </summary>
    public Single Bottom => Y + Height;

    /// <summary>
    ///     Check whether this rectangle contains a point.
    /// </summary>
    /// <param name="point">The point to check.</param>
    /// <returns><see langword="true" /> when the point is contained, otherwise <see langword="false" />.</returns>
    public Boolean Contains(Point point)
    {
        return point.X >= Left && point.X < Right && point.Y >= Top && point.Y < Bottom;
    }

    /// <summary>
    ///     Check whether this rectangle fully contains another rectangle.
    /// </summary>
    /// <param name="rectangle">The rectangle to check.</param>
    /// <returns><see langword="true" /> when the rectangle is fully contained, otherwise <see langword="false" />.</returns>
    public Boolean Contains(Rectangle rectangle)
    {
        return rectangle.Left >= Left && rectangle.Right <= Right && rectangle.Top >= Top && rectangle.Bottom <= Bottom;
    }

    /// <summary>
    ///     Get the intersection of two rectangles.
    /// </summary>
    /// <param name="first">The first operand.</param>
    /// <param name="second">The second operand.</param>
    /// <returns>The intersecting rectangle or <see cref="Empty" /> if there is no overlap.</returns>
    public static Rectangle Intersect(Rectangle first, Rectangle second)
    {
        Single left = Math.Max(first.Left, second.Left);
        Single top = Math.Max(first.Top, second.Top);
        Single right = Math.Min(first.Right, second.Right);
        Single bottom = Math.Min(first.Bottom, second.Bottom);

        if (left <= right && top <= bottom)
        {
            return new Rectangle(left, top, right - left, bottom - top);
        }

        return Empty;
    }

    /// <summary>
    ///     Check whether this rectangle intersects another rectangle.
    /// </summary>
    /// <param name="rectangle">The rectangle to check.</param>
    /// <returns><see langword="true" /> when the rectangles intersect, otherwise <see langword="false" />.</returns>
    public Boolean Intersects(Rectangle rectangle)
    {
        return rectangle.Left < Right && rectangle.Right > Left && rectangle.Top < Bottom && rectangle.Bottom > Top;
    }

    /// <summary>
    ///     Clamp the size of the rectangle to the specified minimum and maximum sizes.
    /// </summary>
    /// <param name="rectangle">The rectangle to clamp.</param>
    /// <param name="minSize">The minimum size.</param>
    /// <param name="maxSize">The maximum size.</param>
    /// <returns>The clamped rectangle.</returns>
    public static Rectangle ClampSize(Rectangle rectangle, Size minSize, Size maxSize)
    {
        Size size = Size.ClampComponents(rectangle.Size, minSize, maxSize);

        return rectangle with {Width = size.Width, Height = size.Height};
    }

    /// <summary>
    ///     Gets the rectangle with the specified offset.
    /// </summary>
    /// <param name="offsetX">The offset in the x-direction.</param>
    /// <param name="offsetY">The offset in the y-direction.</param>
    /// <returns>The rectangle with the specified offset.</returns>
    public Rectangle Moved(Single offsetX, Single offsetY)
    {
        return this with {X = X + offsetX, Y = Y + offsetY};
    }

    /// <inheritdoc />
    public override String ToString()
    {
        return this == Empty
            ? "Rectangle.Empty"
            : $"Rectangle(X: {X}, Y: {Y}, Width: {Width}, Height: {Height})";
    }
}
