// <copyright file="IRenderer.cs" company="VoxelGame">
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
using VoxelGame.GUI.Graphics;
using VoxelGame.GUI.Texts;
using VoxelGame.GUI.Utilities;
using Brush = VoxelGame.GUI.Graphics.Brush;
using Font = VoxelGame.GUI.Texts.Font;

namespace VoxelGame.GUI.Rendering;

/// <summary>
///     The interface expected from a renderer for the GUI.
/// </summary>
public interface IRenderer
{
    /// <summary>
    ///     Begin a rendering pass. All rendering operations must be performed between <see cref="Begin" /> and
    ///     <see cref="End" />.
    /// </summary>
    public void Begin();

    /// <summary>
    ///     End a rendering pass. All rendering operations must be performed between <see cref="Begin" /> and
    ///     <see cref="End" />.
    /// </summary>
    public void End();

    /// <summary>
    ///     Push an offset that will be applied to all operations.
    ///     The offset is additive, meaning the previous offset will be considered.
    /// </summary>
    /// <param name="offset">The offset to push.</param>
    public void PushOffset(PointF offset);

    /// <summary>
    ///     Pop the last pushed offset. Performs no operation if no offset was previously pushed.
    /// </summary>
    public void PopOffset();

    /// <summary>
    ///     Push a clipping rectangle that will be applied to all operations.
    ///     The clipping rectangle is intersected with the previous clipping rectangle.
    ///     Note that clipping must be enabled via <see cref="BeginClip" /> for the clipping rectangle to take effect.
    /// </summary>
    /// <param name="rectangle">The clipping rectangle to push.</param>
    public void PushClip(RectangleF rectangle);

    /// <summary>
    ///     Pop the last pushed clipping rectangle. Performs no operation if no clipping rectangle was previously pushed.
    /// </summary>
    public void PopClip();

    /// <summary>
    ///     Begin clipping. All rendering operations after this call will be clipped to the current clipping rectangle if
    ///     clipping is enabled.
    /// </summary>
    public void BeginClip();

    /// <summary>
    ///     End clipping. All rendering operations after this call will not be clipped.
    /// </summary>
    public void EndClip();

    /// <summary>
    ///     Check if the current clipping rectangle is empty, meaning nothing would pass.
    /// </summary>
    /// <returns>True if the clipping rectangle is empty, false otherwise.</returns>
    public Boolean IsClipEmpty();

    /// <summary>
    ///     Push an opacity to the renderer, multiplying it with the current opacity.
    ///     The initial opacity is <c>1.0f</c>.
    /// </summary>
    /// <param name="opacity">The opacity to push, between <c>0.0f</c> and <c>1.0f</c>.</param>
    public void PushOpacity(Single opacity);

    /// <summary>
    ///     Pop the last pushed opacity. If there is no opacity on the stack, nothing happens.
    /// </summary>
    public void PopOpacity();

    /// <summary>
    ///     Create a formatted text object for the given text, font, and layout options.
    /// </summary>
    /// <param name="text">The text to format.</param>
    /// <param name="font">The font to use for formatting the text.</param>
    /// <param name="options">The layout options such as wrapping, alignment, trimming, and line height.</param>
    /// <returns>The formatted text object.</returns>
    IFormattedText CreateFormattedText(String text, Font font, TextOptions options);

    /// <summary>
    ///     Draw a filled rectangle.
    /// </summary>
    /// <param name="rectangle">The rectangle to draw.</param>
    /// <param name="corners">The radius of the corners to draw.</param>
    /// <param name="brush">The brush to use.</param>
    public void DrawFilledRectangle(RectangleF rectangle, RadiusF corners, Brush brush);

    /// <summary>
    ///     Draw a rectangle with non-rounded corners.
    /// </summary>
    /// <param name="rectangle">The rectangle to draw.</param>
    /// <param name="brush">The brush to use.</param>
    public void DrawFilledRectangle(RectangleF rectangle, Brush brush)
    {
        DrawFilledRectangle(rectangle, RadiusF.Zero, brush);
    }

    /// <summary>
    ///     Draw a rectangle outline with the specified line thickness.
    /// </summary>
    /// <param name="rectangle">The rectangle to draw.</param>
    /// <param name="width">The width of the line.</param>
    /// <param name="corners">The radius of the corners to draw.</param>
    /// <param name="stroke">The style of the line.</param>
    /// <param name="brush">The brush to use.</param>
    public void DrawLinedRectangle(RectangleF rectangle, WidthF width, RadiusF corners, StrokeStyle stroke, Brush brush);

    /// <summary>
    ///     Draw a solid rectangle line with a default border thickness of 1 unit and non-rounded corners.
    /// </summary>
    /// <param name="rectangle">The rectangle to draw.</param>
    /// <param name="brush">The brush to use.</param>
    public void DrawLinedRectangle(RectangleF rectangle, Brush brush)
    {
        DrawLinedRectangle(rectangle, WidthF.One, RadiusF.Zero, StrokeStyle.Solid, brush);
    }

    /// <summary>
    ///     Resize the renderer's internal buffers to the specified size.
    /// </summary>
    /// <param name="size">The new size.</param>
    public void Resize(Size size);

    /// <summary>
    ///     Scale the rendered results by the specified factor.
    ///     This affects all subsequent rendering operations and text measurements.
    /// </summary>
    /// <param name="newScale">The new scale factor.</param>
    public void Scale(Single newScale);
}
