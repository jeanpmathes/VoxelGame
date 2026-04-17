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
using System.Drawing;
using VoxelGame.GUI.Texts;
using Brush = VoxelGame.GUI.Graphics.Brush;
using Font = VoxelGame.GUI.Texts.Font;

namespace VoxelGame.Presentation.New.Platform.Graphics;

/// <summary>
///     Combines all information related to a formatted text for drawing.
/// </summary>
/// <param name="renderer">The renderer used to create and draw this text.</param>
/// <param name="text">The text content.</param>
/// <param name="font">The font used to draw this text.</param>
/// <param name="options">Further options to draw this text.</param>
public sealed class FormattedText(Renderer renderer, String text, Font font, TextOptions options) : IFormattedText
{
    /// <summary>
    ///     The text content.
    /// </summary>
    public String Text { get; } = text;

    /// <summary>
    ///     The font used to draw this text.
    /// </summary>
    public Font Font { get; } = font;

    /// <summary>
    ///     The string format created from the passed options.
    /// </summary>
    public StringFormat StringFormat { get; } = CreateStringFormat(options);

    /// <summary>
    ///     Measure the size this formatted text requires.
    /// </summary>
    /// <param name="availableSize">The available size.</param>
    /// <returns>The measured and required size.</returns>
    public SizeF Measure(SizeF availableSize)
    {
        return renderer.MeasureText(this, availableSize);
    }

    /// <summary>
    ///     Draw this formatted text.
    /// </summary>
    /// <param name="rectangle">The rectangle in which the text will be drawn, used for positioning and clipping.</param>
    /// <param name="brush">The brush to draw the text with.</param>
    public void Draw(RectangleF rectangle, Brush brush)
    {
        renderer.DrawText(this, rectangle, brush);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        StringFormat.Dispose();
    }

    private static StringFormat CreateStringFormat(TextOptions options)
    {
        StringFormat format = (StringFormat) StringFormat.GenericTypographic.Clone();

        if (options.Wrapping == TextWrapping.NoWrap)
            format.FormatFlags |= StringFormatFlags.NoWrap;

        format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;

        format.Alignment = options.Alignment switch
        {
            TextAlignment.Leading => StringAlignment.Near,
            TextAlignment.Center => StringAlignment.Center,
            TextAlignment.Trailing => StringAlignment.Far,
            _ => StringAlignment.Near
        };

        format.LineAlignment = StringAlignment.Near;

        format.Trimming = options.Trimming switch
        {
            TextTrimming.None => StringTrimming.None,
            TextTrimming.Character => StringTrimming.Character,
            TextTrimming.Word => StringTrimming.Word,
            TextTrimming.CharacterEllipsis => StringTrimming.EllipsisCharacter,
            TextTrimming.WordEllipsis => StringTrimming.EllipsisWord,
            TextTrimming.PathEllipsis => StringTrimming.EllipsisPath,
            _ => StringTrimming.None
        };

        return format;
    }
}
