// <copyright file="TextSupport.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.ComponentModel;
using System.Drawing;
using Gwen.Net;
using Gwen.Net.Renderer;
using VoxelGame.Core.Utilities;
using Font = Gwen.Net.Font;
using FontStyle = System.Drawing.FontStyle;
using Point = System.Drawing.Point;

namespace VoxelGame.UI.Platform.Renderer;

/// <summary>
///     Supports implementation of text rendering.
/// </summary>
public sealed class TextSupport : IDisposable
{
    private readonly Graphics graphics;
    private readonly RendererBase renderer;

    /// <summary>
    ///     Creates a new instance of <see cref="TextSupport" />.
    /// </summary>
    public TextSupport(RendererBase renderer)
    {
        this.renderer = renderer;
        graphics = Graphics.FromImage(new Bitmap(width: 1024, height: 1024));
    }

    /// <summary>
    ///     Converts a value from a given unit to pixels.
    /// </summary>
    private float ConvertToPixels(float value, GraphicsUnit unit)
    {
        Throw.IfDisposed(disposed);

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
            default: throw new InvalidEnumArgumentException("Unknown unit " + unit);
        }

        return value;
    }

    /// <summary>
    ///     Load a font.
    /// </summary>
    public bool LoadFont(Font font)
    {
        Throw.IfDisposed(disposed);

        font.RealSize = (float) Math.Ceiling(font.Size * renderer.Scale);

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
        Throw.IfDisposed(disposed);

        if (font.RendererData is not System.Drawing.Font sysFont
            || Math.Abs(font.RealSize - font.Size * renderer.Scale) > 2)
        {
            FreeFont(font);
            LoadFont(font);
            sysFont = (System.Drawing.Font) font.RendererData!;
        }

        // From: http://csharphelper.com/blog/2014/08/get-font-metrics-in-c
        float emHeight = sysFont.FontFamily.GetEmHeight(sysFont.Style);
        float emHeightPixels = ConvertToPixels(sysFont.Size, sysFont.Unit);
        float designToPixels = emHeightPixels / emHeight;

        float ascentPixels = designToPixels * sysFont.FontFamily.GetCellAscent(sysFont.Style);
        float descentPixels = designToPixels * sysFont.FontFamily.GetCellDescent(sysFont.Style);
        float cellHeightPixels = ascentPixels + descentPixels;
        float internalLeadingPixels = cellHeightPixels - emHeightPixels;
        float lineSpacingPixels = designToPixels * sysFont.FontFamily.GetLineSpacing(sysFont.Style);
        float externalLeadingPixels = lineSpacingPixels - cellHeightPixels;

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
        Throw.IfDisposed(disposed);

        return graphics.MeasureString(
            "....",
            font); //Spaces are not being picked up, let's just use .'s.
    }

    /// <summary>
    ///     Measure the size of a string.
    /// </summary>
    public SizeF MeasureString(string text, System.Drawing.Font font, StringFormat format)
    {
        Throw.IfDisposed(disposed);

        return graphics.MeasureString(text, font, Point.Empty, format);
    }

    #region IDisposable Support

    private bool disposed;

    private void Dispose(bool disposing)
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

    #endregion IDisposable Support
}
