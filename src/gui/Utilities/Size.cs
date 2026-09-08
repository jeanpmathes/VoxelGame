// <copyright file="Size.cs" company="VoxelGame">
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
///     Represents the size of a rectangle in 2D space.
/// </summary>
/// <param name="Width">The width of the size.</param>
/// <param name="Height">The height of the size.</param>
[StructLayout(LayoutKind.Sequential)]
public readonly record struct Size(Single Width, Single Height)
{
    /// <summary>
    ///     Gets an empty size.
    /// </summary>
    public static readonly Size Empty = new(Width: 0, Height: 0);

    /// <summary>
    ///     Gets whether this size is empty.
    /// </summary>
    public Boolean IsEmpty => Width == 0 && Height == 0;

    /// <summary>
    ///     Add two sizes.
    /// </summary>
    public static Size operator +(Size left, Size right)
    {
        return new Size(left.Width + right.Width, left.Height + right.Height);
    }

    /// <inheritdoc cref="operator + (Size, Size)" />
    public static Size Add(Size left, Size right)
    {
        return left + right;
    }

    /// <summary>
    ///     Subtract one size from another.
    /// </summary>
    public static Size operator -(Size left, Size right)
    {
        return new Size(left.Width - right.Width, left.Height - right.Height);
    }

    /// <inheritdoc cref="operator - (Size, Size)" />
    public static Size Subtract(Size left, Size right)
    {
        return left - right;
    }

    /// <summary>
    ///     Multiply a size by a scalar.
    /// </summary>
    public static Size operator *(Size size, Single scalar)
    {
        return new Size(size.Width * scalar, size.Height * scalar);
    }

    /// <inheritdoc cref="operator * (Size, Single)" />
    public static Size Multiply(Size size, Single scalar)
    {
        return size * scalar;
    }

    /// <summary>
    ///     Divide a size by a scalar.
    /// </summary>
    public static Size operator /(Size size, Single scalar)
    {
        return new Size(size.Width / scalar, size.Height / scalar);
    }

    /// <inheritdoc cref="operator / (Size, Single)" />
    public static Size Divide(Size size, Single scalar)
    {
        return size / scalar;
    }

    /// <summary>
    ///     Get the component-wise maximum of two sizes.
    /// </summary>
    /// <param name="size1">The first size.</param>
    /// <param name="size2">The second size.</param>
    /// <returns>>The component-wise maximum of the two sizes.</returns>
    public static Size MaxComponents(Size size1, Size size2)
    {
        return new Size(Math.Max(size1.Width, size2.Width), Math.Max(size1.Height, size2.Height));
    }

    /// <summary>
    ///     Clamp size between min and max sizes, performing a component-wise clamp.
    /// </summary>
    /// <param name="size">The size to clamp.</param>
    /// <param name="minSize">The minimum size.</param>
    /// <param name="maxSize">The maximum size.</param>
    /// <returns>The clamped size.</returns>
    public static Size ClampComponents(Size size, Size minSize, Size maxSize)
    {
        return new Size(Math.Clamp(size.Width, minSize.Width, maxSize.Width), Math.Clamp(size.Height, minSize.Height, maxSize.Height));
    }

    /// <inheritdoc />
    public override String ToString()
    {
        return this == Empty
            ? "Size.Empty"
            : $"Size(Width: {Width}, Height: {Height})";
    }
}
