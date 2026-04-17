// <copyright file="IFormattedText.cs" company="VoxelGame">
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
using Brush = VoxelGame.GUI.Graphics.Brush;

namespace VoxelGame.GUI.Texts;

/// <summary>
///     Combines text content with formatting information.
/// </summary>
public interface IFormattedText : IDisposable
{
    /// <summary>
    ///     Measures the size of the formatted text given the available size constraints.
    /// </summary>
    /// <param name="availableSize">The available size for the formatted text.</param>
    /// <returns>The size required to render the formatted text within the given constraints.</returns>
    public SizeF Measure(SizeF availableSize);

    /// <summary>
    ///     Draws the formatted text in a given rectangle using the specified brush.
    ///     Note that this method is affected by the current state of the renderer, e.g. offset and clipping.
    /// </summary>
    /// <param name="rectangle">The rectangle in which to draw the text.</param>
    /// <param name="brush">The brush to use for drawing the text.</param>
    public void Draw(RectangleF rectangle, Brush brush);
}
