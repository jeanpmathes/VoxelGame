// <copyright file="Sizes.cs" company="VoxelGame">
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
///     Helps to work with sizes.
/// </summary>
public static class Sizes
{
    /// <summary>
    ///     Get the component-wise maximum of two sizes.
    /// </summary>
    /// <param name="size1">The first size.</param>
    /// <param name="size2">The second size.</param>
    /// <returns>>The component-wise maximum of the two sizes.</returns>
    public static SizeF Max(SizeF size1, SizeF size2)
    {
        return new SizeF(Math.Max(size1.Width, size2.Width), Math.Max(size1.Height, size2.Height));
    }

    /// <summary>
    ///     Clamp size between min and max sizes, performing a component-wise clamp.
    /// </summary>
    /// <param name="size">The size to clamp.</param>
    /// <param name="minSize">The minimum size.</param>
    /// <param name="maxSize">The maximum size.</param>
    /// <returns>The clamped size.</returns>
    public static SizeF Clamp(SizeF size, SizeF minSize, SizeF maxSize)
    {
        return new SizeF(Math.Clamp(size.Width, minSize.Width, maxSize.Width), Math.Clamp(size.Height, minSize.Height, maxSize.Height));
    }
}
