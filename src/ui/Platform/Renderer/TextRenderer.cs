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
using VoxelGame.Toolkit.Utilities;
using Color = System.Drawing.Color;
using Font = System.Drawing.Font;
using Image = VoxelGame.Core.Visuals.Image;
using Point = Gwen.Net.Point;

namespace VoxelGame.UI.Platform.Renderer;

/// <summary>
///     Renders text to a texture.
/// </summary>
public sealed class TextRenderer : IDisposable
{
    private readonly DirectXRenderer renderer;

    private readonly Texture texture;

    /// <summary>
    ///     Creates a new instance of <see cref="TextRenderer" />.
    /// </summary>
    public TextRenderer(Int32 width, Int32 height, DirectXRenderer renderer)
    {
        Debug.Assert(width > 0);
        Debug.Assert(height > 0);

        this.renderer = renderer;

        texture = new Texture(renderer) {Width = width, Height = height};
    }

    /// <summary>
    ///     Gets the backing store.
    /// </summary>
    public Texture Texture => disposed ? throw new ObjectDisposedException(nameof(TextRenderer)) : texture;

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
    public void SetString(String text, Font font, Brush brush, Point point, StringFormat format)
    {
        Throw.IfDisposed(disposed);

        using Bitmap bitmap = new(Texture.Width, Texture.Height, PixelFormat.Format32bppArgb);

        using System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap);
        graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        graphics.Clear(Color.Transparent);

        graphics.DrawString(
            text,
            font,
            brush,
            new System.Drawing.Point(point.X, point.Y),
            format);

        renderer.LoadTextureDirectly(Texture, new Image(bitmap));
    }

    #region DISPOSABLE

    private Boolean disposed;

    /// <summary>
    ///     Disposes the instance.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing) Texture.Dispose();

        disposed = true;
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~TextRenderer()
    {
        Dispose(disposing: false);
    }

    #endregion DISPOSABLE
}
