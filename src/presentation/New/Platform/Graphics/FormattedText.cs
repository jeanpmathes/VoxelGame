// <copyright file="FormattedText.cs" company="VoxelGame">
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
using VoxelGame.Graphics.Objects.UserInterface;
using VoxelGame.GUI.Texts;
using VoxelGame.GUI.Utilities;
using Brush = VoxelGame.GUI.Drawing.Brushes.Brush;

namespace VoxelGame.Presentation.New.Platform.Graphics;

/// <summary>
///     Combines all information related to a formatted text for drawing.
/// </summary>
public sealed class FormattedText : IFormattedText
{
    private readonly Renderer renderer;
    private readonly Text text;

    private Size? lastAvailableSize;

    internal FormattedText(Renderer renderer, String content, TextOptions options)
    {
        this.renderer = renderer;

        text = renderer.CreateText(content, options);
    }

    /// <summary>
    ///     Measure the size this formatted text requires.
    /// </summary>
    /// <param name="availableSize">The available size.</param>
    /// <returns>The measured and required size.</returns>
    public Size Measure(Size availableSize)
    {
        availableSize = renderer.ApplyScale(availableSize);

        lastAvailableSize = availableSize;

        return renderer.ApplyInverseScale(text.Measure(availableSize));
    }

    /// <summary>
    ///     Draw this formatted text.
    /// </summary>
    /// <param name="rectangle">The rectangle in which the text will be drawn, used for positioning and clipping.</param>
    /// <param name="brush">The brush with which to draw the text.</param>
    public void Draw(Rectangle rectangle, Brush brush)
    {
        if (lastAvailableSize == null || lastAvailableSize.Value != rectangle.Size)
            Measure(rectangle.Size);

        renderer.DrawText(text, rectangle.Location, brush);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        text.Dispose();
    }
}
