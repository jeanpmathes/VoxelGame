// <copyright file="TextRenderer.cs" company="Gwen.Net">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>Gwen.Net, jeanpmathes</author>

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using Gwen.Net;
using Gwen.Net.Renderer;
using VoxelGame.UI.Platform.Renderers;
using Color = System.Drawing.Color;
using Font = System.Drawing.Font;
using Point = Gwen.Net.Point;

namespace VoxelGame.UI.Platform;

/// <summary>
///     Renders text to a texture.
/// </summary>
public sealed class TextRenderer : IDisposable
{
    private readonly Bitmap bitmap;
    private readonly Graphics graphics;
    private bool disposed;

    /// <summary>
    ///     Creates a new instance of <see cref="TextRenderer" />.
    /// </summary>
    public TextRenderer(int width, int height, RendererBase renderer)
    {
        Debug.Assert(width > 0);
        Debug.Assert(height > 0);

        bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        graphics = Graphics.FromImage(bitmap);
        graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        graphics.Clear(Color.Transparent);
        Texture = new Texture(renderer) {Width = width, Height = height};
    }

    /// <summary>
    ///     Gets the backing store.
    /// </summary>
    public Texture Texture { get; }

    /// <summary>
    ///     Disposes the instance.
    /// </summary>
    public void Dispose()
    {
        Dispose(manual: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Draws the specified string to the backing store.
    /// </summary>
    /// <param name="text">The <see cref="System.String" /> to draw.</param>
    /// <param name="font">The <see cref="System.Drawing.Font" /> that will be used.</param>
    /// <param name="brush">The <see cref="System.Drawing.Brush" /> that will be used.</param>
    /// <param name="point">
    ///     The location of the text on the backing store, in 2d pixel coordinates.
    ///     The origin (0, 0) lies at the top-left corner of the backing store.
    /// </param>
    /// <param name="format">The <see cref="StringFormat" /> that will be used.</param>
    public void DrawString(string text, Font font, Brush brush, Point point, StringFormat format)
    {
        graphics.DrawString(
            text,
            font,
            brush,
            new System.Drawing.Point(point.X, point.Y),
            format); // render text on the bitmap

        DirectXRendererBase.LoadTextureInternal(Texture, bitmap); // copy bitmap to gl texture
    }

    private void Dispose(bool manual)
    {
        if (disposed) return;

        if (manual)
        {
            bitmap.Dispose();
            graphics.Dispose();
            Texture.Dispose();
        }

        disposed = true;
    }

    ~TextRenderer()
    {
        Dispose(manual: false);
    }
}

