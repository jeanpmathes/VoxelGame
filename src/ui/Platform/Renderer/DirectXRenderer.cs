// <copyright file="DirectXRenderer.cs" company="Gwen.Net">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>Gwen.Net, jeanpmathes</author>

using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using Gwen.Net;
using Gwen.Net.Renderer;
using OpenTK.Mathematics;
using VoxelGame.Graphics.Core;
using Color = Gwen.Net.Color;
using Font = Gwen.Net.Font;
using Image = VoxelGame.Core.Visuals.Image;
using Point = Gwen.Net.Point;
using Rectangle = Gwen.Net.Rectangle;
using Size = Gwen.Net.Size;

namespace VoxelGame.UI.Platform.Renderer;

/// <summary>
///     Class for the DirectX-based GUI renderer.
/// </summary>
public sealed class DirectXRenderer : RendererBase
{
    private readonly RenderPipeline renderPipeline;

    private readonly TextStorage textStorage;
    private readonly TextSupport textSupport;

    private readonly TextureSupport textureSupport;

    private TextureList.Handle currentTexture;

    /// <summary>
    ///     Creates a new instance of <see cref="DirectXRenderer" />.
    /// </summary>
    internal DirectXRenderer(Client client, GwenGuiSettings settings)
    {
        textSupport = new TextSupport(this);
        textStorage = new TextStorage(this);

        renderPipeline
            = RenderPipeline.Create(client, this, PreDraw, settings.ShaderFile, settings.ShaderLoadingErrorCallback)
              ?? throw new InvalidOperationException("Failed to create render pipeline.");

        textureSupport = new TextureSupport(renderPipeline.Textures, settings);
    }

    /// <summary>
    ///     Set the current draw color.
    /// </summary>
    public override Color DrawColor { get; set; }

    private void PreDraw()
    {
        textStorage.Update();
    }

    /// <summary>
    ///     Notifies the renderer that the loading phase is finished.
    /// </summary>
    internal void FinishLoading()
    {
        textureSupport.FinishLoading();
    }

    /// <inheritdoc />
    protected override void OnScaleChanged(Single oldScale)
    {
        textStorage.Flush();
    }

    /// <inheritdoc />
    public override void DrawFilledRect(Rectangle rect)
    {
        if (currentTexture.IsValid)
        {
            renderPipeline.PushCall(currentTexture);
            currentTexture = TextureList.Handle.Invalid;
        }

        rect = Translate(rect);

        renderPipeline.PushRect(rect);
    }

    /// <inheritdoc />
    public override void StartClip()
    {
        renderPipeline.IsClippingEnabled = true;
    }

    /// <inheritdoc />
    public override void EndClip()
    {
        renderPipeline.IsClippingEnabled = false;
    }

    /// <inheritdoc />
    public override void DrawTexturedRect(Texture texture, Rectangle targetRect,
        Single u1 = 0, Single v1 = 0, Single u2 = 1, Single v2 = 1)
    {
        if (texture.RendererData == null)
        {
            DrawMissingImage(targetRect);

            return;
        }

        TextureList.Handle handle = TextureSupport.GetTextureHandle(texture);
        targetRect = Translate(targetRect);

        Boolean differentTexture = currentTexture != handle;

        if (!currentTexture.IsValid || differentTexture) renderPipeline.PushCall(currentTexture);

        currentTexture = handle;

        renderPipeline.PushRect(targetRect, u1, v1, u2, v2);
    }

    /// <inheritdoc />
    public override Boolean LoadFont(Font font)
    {
        return textSupport.LoadFont(font);
    }

    /// <inheritdoc />
    public override void FreeFont(Font font)
    {
        TextSupport.FreeFont(font);
    }

    /// <inheritdoc />
    public override FontMetrics GetFontMetrics(Font font)
    {
        return textSupport.GetFontMetrics(font);
    }

    /// <inheritdoc />
    public override Size MeasureText(Font font, String text)
    {
        if (font.RendererData is not System.Drawing.Font sysFont
            || Math.Abs(font.RealSize - font.Size * Scale) > 2)
        {
            FreeFont(font);
            LoadFont(font);

            sysFont = (System.Drawing.Font) font.RendererData!;
        }

        if (textStorage.GetTexture(font, text) is {} texture) return new Size(texture.Width, texture.Height);

        Debug.Assert(sysFont != null);

        SizeF tabSize = textSupport.MeasureTab(sysFont);

        textStorage.StringFormat.SetTabStops(
            firstTabOffset: 0f,
            [
                tabSize.Width
            ]);

        SizeF size = textSupport.MeasureString(text, sysFont, textStorage.StringFormat);

        return new Size(Util.Ceil(size.Width), Util.Ceil(size.Height));
    }

    /// <inheritdoc />
    public override void RenderText(Font font, Point position, String text)
    {
        renderPipeline.PushCall(currentTexture);

        if (font.RendererData is not System.Drawing.Font || Math.Abs(font.RealSize - font.Size * Scale) > 2)
        {
            FreeFont(font);
            LoadFont(font);
        }

        Texture texture = textStorage.GetOrCreateTexture(font, text);

        DrawTexturedRect(
            texture,
            new Rectangle(position.X, position.Y, texture.Width, texture.Height));
    }

    /// <inheritdoc />
    public override void Begin()
    {
        renderPipeline.IsClippingEnabled = false;
        currentTexture = TextureList.Handle.Invalid;

        renderPipeline.Reset();
    }

    /// <inheritdoc />
    public override void End()
    {
        renderPipeline.PushCall(currentTexture);
    }

    /// <inheritdoc />
    public override void LoadTexture(Texture texture, Action<Exception> errorCallback)
    {
        textureSupport.LoadTexture(texture, errorCallback);
    }

    /// <inheritdoc />
    public override void LoadTextureRaw(Texture texture, Byte[] pixelData)
    {
        Span<Byte> bytes = pixelData;
        Span<Int32> pixels = MemoryMarshal.Cast<Byte, Int32>(bytes);

        Image image = new(pixels, Image.Format.BGRA, texture.Width, texture.Height);

        textureSupport.LoadTextureDirectly(texture, image);
    }

    /// <summary>
    ///     Load a texture directly from an image.
    /// </summary>
    /// <param name="texture">The texture to load.</param>
    /// <param name="image">The image to load.</param>
    public void LoadTextureDirectly(Texture texture, Image image)
    {
        textureSupport.LoadTextureDirectly(texture, image);
    }

    /// <inheritdoc />
    public override void FreeTexture(Texture texture)
    {
        textureSupport.FreeTexture(texture);
    }

    /// <inheritdoc />
    public override Color PixelColor(Texture texture, UInt32 x, UInt32 y, Color defaultColor)
    {
        return textureSupport.GetTexturePixel(texture, (x, y)) ?? defaultColor;
    }

    /// <summary>
    ///     Call when the window is resized.
    /// </summary>
    /// <param name="size">New window size.</param>
    public void Resize(Vector2 size)
    {
        renderPipeline.Resize(size);
    }

    #region DISPOSABLE

    /// <inheritdoc />
    public override void Dispose()
    {
        textSupport.Dispose();
        textStorage.Dispose();

        renderPipeline.Dispose();

        base.Dispose();
    }

    #endregion DISPOSABLE
}
