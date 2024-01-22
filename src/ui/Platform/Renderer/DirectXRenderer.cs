// <copyright file="DirectXRenderer.cs" company="Gwen.Net">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>Gwen.Net, jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using Gwen.Net;
using Gwen.Net.Renderer;
using OpenTK.Mathematics;
using VoxelGame.Support.Core;
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
public sealed class DirectXRenderer : RendererBase // todo: refactor to decrease type usage, maybe pull out TextureSupport and move more text stuff to TextSupport
{
    private readonly Dictionary<string, string> preloadNameToPath = new();

    private readonly RenderPipeline renderPipeline;

    private readonly TextStorage textStorage;
    private readonly TextSupport textSupport;

    private TextureList.Handle currentTexture;

    private bool textureDiscardAllowed;

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

        foreach (TexturePreload texturePreload in settings.TexturePreloads)
        {
            Exception? exception = renderPipeline.Textures.LoadTexture(texturePreload.File,
                textureDiscardAllowed,
                _ =>
                {
                    preloadNameToPath.Add(texturePreload.Name, texturePreload.File.FullName);
                });

            if (exception != null)
                settings.TexturePreloadErrorCallback(texturePreload, exception);
        }
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
    ///     Indicate that the loading phase is finished.
    ///     Textures that are loaded after this call can be freed, while texture created during loading are kept alive with the
    ///     client.
    /// </summary>
    internal void FinishLoading()
    {
        textureDiscardAllowed = true;
    }

    /// <inheritdoc />
    protected override void OnScaleChanged(float oldScale)
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
        float u1 = 0, float v1 = 0, float u2 = 1, float v2 = 1)
    {
        if (texture.RendererData == null)
        {
            DrawMissingImage(targetRect);

            return;
        }

        TextureList.Handle handle = GetRenderData(texture);
        targetRect = Translate(targetRect);

        bool differentTexture = currentTexture != handle;

        if (!currentTexture.IsValid || differentTexture) renderPipeline.PushCall(currentTexture);

        currentTexture = handle;

        renderPipeline.PushRect(targetRect, u1, v1, u2, v2);
    }

    /// <inheritdoc />
    public override bool LoadFont(Font font)
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
    public override Size MeasureText(Font font, string text)
    {
        if (font.RendererData is not System.Drawing.Font sysFont
            || Math.Abs(font.RealSize - font.Size * Scale) > 2)
        {
            FreeFont(font);
            LoadFont(font);
            sysFont = (System.Drawing.Font) font.RendererData;
        }

        if (textStorage.GetTexture(font, text) is {} texture) return new Size(texture.Width, texture.Height);

        Debug.Assert(sysFont != null);

        SizeF tabSize = textSupport.MeasureTab(sysFont);

        textStorage.StringFormat.SetTabStops(
            firstTabOffset: 0f,
            new[]
            {
                tabSize.Width
            });

        SizeF size = textSupport.MeasureString(text, sysFont, textStorage.StringFormat);

        return new Size(Util.Ceil(size.Width), Util.Ceil(size.Height));
    }

    /// <inheritdoc />
    public override void RenderText(Font font, Point position, string text)
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
    }

    /// <inheritdoc />
    public override void End()
    {
        renderPipeline.PushCall(currentTexture);
    }

    /// <inheritdoc />
    public override void LoadTexture(Texture texture, Action<Exception> errorCallback)
    {
        TextureList.Handle handle = TextureList.Handle.Invalid;

        if (preloadNameToPath.TryGetValue(texture.Name, out string? path)) handle = renderPipeline.Textures.GetTexture(path);

        if (!handle.IsValid) handle = renderPipeline.Textures.GetTexture(texture.Name);

        if (!handle.IsValid)
        {
            Exception? exception = renderPipeline.Textures.LoadTexture(new FileInfo(texture.Name),
                textureDiscardAllowed,
                loaded =>
                {
                    handle = loaded;
                });

            if (exception != null) errorCallback(exception);
        }

        if (handle.IsValid) SetTextureProperties(texture, handle);
        else SetFailedTextureProperties(texture);
    }

    /// <inheritdoc />
    public override void LoadTextureRaw(Texture texture, byte[] pixelData)
    {
        Span<byte> bytes = pixelData;
        Span<int> pixels = MemoryMarshal.Cast<byte, int>(bytes);

        Image image = new(pixels, Image.Format.BGRA, texture.Width, texture.Height);

        LoadTextureDirectly(texture, image);
    }

    /// <summary>
    ///     Load a texture directly from an image.
    /// </summary>
    /// <param name="t">The texture to load.</param>
    /// <param name="image">The image to load.</param>
    public void LoadTextureDirectly(Texture t, Image image)
    {
        TextureList.Handle loadedTexture = renderPipeline.Textures.LoadTexture(image, allowDiscard: true);
        SetTextureProperties(t, loadedTexture);
    }

    private void SetTextureProperties(Texture texture, TextureList.Handle loadedTexture)
    {
        renderPipeline.Textures.DiscardTexture(GetRenderData(texture));

        Support.Objects.Texture? entry = renderPipeline.Textures.GetEntry(loadedTexture);

        Debug.Assert(loadedTexture.IsValid);
        Debug.Assert(entry != null);

        texture.Width = entry.Width;
        texture.Height = entry.Height;
        texture.Failed = false;

        texture.RendererData = loadedTexture;
    }

    private void SetFailedTextureProperties(Texture texture)
    {
        renderPipeline.Textures.DiscardTexture(GetRenderData(texture));

        texture.RendererData = null;
        texture.Width = 0;
        texture.Height = 0;
        texture.Failed = true;
    }

    /// <inheritdoc />
    public override void FreeTexture(Texture texture)
    {
        renderPipeline.Textures.DiscardTexture(GetRenderData(texture));

        texture.RendererData = null;
        texture.Width = 0;
        texture.Height = 0;
        texture.Failed = false;
    }

    /// <inheritdoc />
    public override Color PixelColor(Texture texture, uint x, uint y, Color defaultColor)
    {
        if (texture.RendererData == null) return defaultColor;

        TextureList.Handle handle = GetRenderData(texture);

        return renderPipeline.Textures.GetPixel(handle, x, y);
    }

    /// <summary>
    ///     Call when the window is resized.
    /// </summary>
    /// <param name="size">New window size.</param>
    public void Resize(Vector2 size)
    {
        renderPipeline.Resize(size);
    }

    private static TextureList.Handle GetRenderData(Texture texture)
    {
        if (texture.RendererData == null) return TextureList.Handle.Invalid;

        var handle = (TextureList.Handle) texture.RendererData;
        Debug.Assert(handle.IsValid);

        return handle;
    }

    #region IDisposable Support

    /// <inheritdoc />
    public override void Dispose()
    {
        textSupport.Dispose();
        textStorage.Dispose();

        renderPipeline.Dispose();

        base.Dispose();
    }

    #endregion IDisposable Support
}
