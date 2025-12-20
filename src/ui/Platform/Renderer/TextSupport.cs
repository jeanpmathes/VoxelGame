// <copyright file="TextGraphics.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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
using Gwen.Net;
using Gwen.Net.Renderer;
using VoxelGame.Toolkit.Utilities;
using Font = Gwen.Net.Font;
using FontStyle = System.Drawing.FontStyle;
using Point = System.Drawing.Point;

namespace VoxelGame.UI.Platform.Renderer;

/// <summary>
///     Supports implementation of text rendering.
/// </summary>
public sealed class TextSupport : IDisposable
{
    private readonly System.Drawing.Graphics graphics;
    private readonly RendererBase renderer;

    /// <summary>
    ///     Creates a new instance of <see cref="TextSupport" />.
    /// </summary>
    public TextSupport(RendererBase renderer)
    {
        this.renderer = renderer;
        graphics = System.Drawing.Graphics.FromImage(new Bitmap(width: 1024, height: 1024));
    }

    /// <summary>
    ///     Converts a value from a given unit to pixels.
    /// </summary>
    private Single ConvertToPixels(Single value, GraphicsUnit unit)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        switch (unit)
        {
            case GraphicsUnit.Document:
                value *= graphics.DpiX / 300;

                break;
            case GraphicsUnit.Inch:
                value *= graphics.DpiX;

                break;
            case GraphicsUnit.Millimeter:
                value *= graphics.DpiX / 25.4F;

                break;
            case GraphicsUnit.Pixel: break;
            case GraphicsUnit.Point:
                value *= graphics.DpiX / 72;

                break;
            default: throw Exceptions.UnsupportedEnumValue(unit);
        }

        return value;
    }

    /// <summary>
    ///     Load a font.
    /// </summary>
    public Boolean LoadFont(Font font)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        font.RealSize = (Single) Math.Ceiling(font.Size * renderer.Scale);

        if (font.RendererData is System.Drawing.Font sysFont) sysFont.Dispose();

        var fontStyle = FontStyle.Regular;

        if (font.Bold) fontStyle |= FontStyle.Bold;

        if (font.Italic) fontStyle |= FontStyle.Italic;

        if (font.Underline) fontStyle |= FontStyle.Underline;

        if (font.Strikeout) fontStyle |= FontStyle.Strikeout;

        // Apparently this can't fail:
        // "If you attempt to use a font that is not supported, or the font is not installed on the machine that is running the application, the Microsoft Sans Serif font will be substituted."
        sysFont = new System.Drawing.Font(font.FaceName, font.RealSize, fontStyle);
        font.RendererData = sysFont;

        return true;
    }

    /// <summary>
    ///     Free a font.
    /// </summary>
    public static void FreeFont(Font font)
    {
        if (font.RendererData is System.Drawing.Font sysFont) sysFont.Dispose();

        font.RendererData = null;
    }

    /// <summary>
    ///     Get the metrics of a font.
    /// </summary>
    public FontMetrics GetFontMetrics(Font font)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        if (font.RendererData is not System.Drawing.Font sysFont
            || Math.Abs(font.RealSize - font.Size * renderer.Scale) > 2)
        {
            FreeFont(font);
            LoadFont(font);
            sysFont = (System.Drawing.Font) font.RendererData!;
        }

        // From: http://csharphelper.com/blog/2014/08/get-font-metrics-in-c
        Single emHeight = sysFont.FontFamily.GetEmHeight(sysFont.Style);
        Single emHeightPixels = ConvertToPixels(sysFont.Size, sysFont.Unit);
        Single designToPixels = emHeightPixels / emHeight;

        Single ascentPixels = designToPixels * sysFont.FontFamily.GetCellAscent(sysFont.Style);
        Single descentPixels = designToPixels * sysFont.FontFamily.GetCellDescent(sysFont.Style);
        Single cellHeightPixels = ascentPixels + descentPixels;
        Single internalLeadingPixels = cellHeightPixels - emHeightPixels;
        Single lineSpacingPixels = designToPixels * sysFont.FontFamily.GetLineSpacing(sysFont.Style);
        Single externalLeadingPixels = lineSpacingPixels - cellHeightPixels;

        FontMetrics fm = new(
            emHeightPixels,
            ascentPixels,
            descentPixels,
            cellHeightPixels,
            internalLeadingPixels,
            lineSpacingPixels,
            externalLeadingPixels
        );

        return fm;
    }

    /// <summary>
    ///     Measure the size of tab characters.
    /// </summary>
    public SizeF MeasureTab(System.Drawing.Font font)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        return graphics.MeasureString(
            "....",
            font); //Spaces are not being picked up, let's just use .'s.
    }

    /// <summary>
    ///     Measure the size of a string.
    /// </summary>
    public SizeF MeasureString(String text, System.Drawing.Font font, StringFormat format)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        return graphics.MeasureString(text, font, Point.Empty, format);
    }

    #region DISPOSABLE

    private Boolean disposed;

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing) graphics.Dispose();

        disposed = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~TextSupport()
    {
        Dispose(disposing: false);
    }

    #endregion DISPOSABLE
}
