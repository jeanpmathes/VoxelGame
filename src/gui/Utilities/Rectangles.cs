// <copyright file="Rectangles.cs" company="VoxelGame">
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

using System.Drawing;

namespace VoxelGame.GUI.Utilities;

/// <summary>
///     Utility class for rectangle operations.
/// </summary>
public static class Rectangles
{
    /// <summary>
    ///     Clamp the size of the rectangle to the specified minimum and maximum sizes.
    /// </summary>
    /// <param name="rectangle">The rectangle to clamp.</param>
    /// <param name="minSize">The minimum size.</param>
    /// <param name="maxSize">The maximum size.</param>
    /// <returns>The clamped rectangle.</returns>
    public static RectangleF ClampSize(RectangleF rectangle, SizeF minSize, SizeF maxSize)
    {
        rectangle.Size = Sizes.Clamp(rectangle.Size, minSize, maxSize);
        return rectangle;
    }
}
