// <copyright file="DirectXRenderer.cs" company="Gwen.Net">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>Gwen.Net, jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Gwen.Net;
using Gwen.Net.Renderer;
using OpenTK.Mathematics;
using VoxelGame.Support;
using Color = Gwen.Net.Color;
using Font = Gwen.Net.Font;
using Point = Gwen.Net.Point;
using Rectangle = Gwen.Net.Rectangle;
using Size = Gwen.Net.Size;

namespace VoxelGame.UI.Platform.Renderer;

/// <summary>
///     Class for DirectX GUI renderer.
/// </summary>
public sealed class DirectXRenderer : RendererBase
{
    private readonly Dictionary<string, string> preloadNameToPath = new();

    private readonly RenderPipeline renderPipeline;

    private readonly TextCache textCache;
    private readonly TextSupport textSupport;

    private Bitmap? currentPixelColorSource;
    private string currentPixelColorSourceName = "";

    private TextureList.Handle currentTexture;

    private bool textureDiscardAllowed;

    /// <summary>
    ///     Creates a new instance of <see cref="DirectXRenderer" />.
    /// </summary>
    internal DirectXRenderer(Client client, GwenGuiSettings settings)
    {
        textSupport = new TextSupport(this);
        textCache = new TextCache(this);

        renderPipeline = new RenderPipeline(client, this, PreDraw, settings.ShaderFile, settings.ShaderLoadingErrorCallback);

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
        textCache.Evict();
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
    public override void Dispose()
    {
        textSupport.Dispose();
        textCache.Dispose();
        currentPixelColorSource?.Dispose();

        base.Dispose();
    }

    /// <inheritdoc />
    protected override void OnScaleChanged(float oldScale)
    {
        textCache.Flush();
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
    public override void DrawTexturedRect(Texture texture, Rectangle targetRect, float u1 = 0, float v1 = 0, float u2 = 1,
        float v2 = 1)
    {
        if (null == texture.RendererData)
        {
            DrawMissingImage(targetRect);

            return;
        }

        var handle = (TextureList.Handle) texture.RendererData;
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

        if (textCache.GetTexture(font, text) is {} texture)
        {
            return new Size(texture.Width, texture.Height);
        }

        Debug.Assert(sysFont != null);

        SizeF tabSize = textSupport.MeasureTab(sysFont);

        textCache.StringFormat.SetTabStops(
            firstTabOffset: 0f,
            new[]
            {
                tabSize.Width
            });

        SizeF size = textSupport.MeasureString(text, sysFont, textCache.StringFormat);

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

        Texture texture = textCache.GetOrCreateTexture(font, text);

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

        if (preloadNameToPath.TryGetValue(texture.Name, out string? path))
        {
            handle = renderPipeline.Textures.GetTexture(path);
        }

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

        if (handle.IsValid)
        {
            SetTextureProperties(texture, handle);
        }
        else
        {
            SetFailedTextureProperties(texture);
        }
    }

    /// <inheritdoc />
    public override void LoadTextureRaw(Texture texture, byte[] pixelData)
    {
        Bitmap bitmap;

        try
        {
            unsafe
            {
                fixed (byte* ptr = &pixelData[0])
                {
                    bitmap = new Bitmap(texture.Width, texture.Height, 4 * texture.Width, PixelFormat.Format32bppArgb, (IntPtr) ptr);
                }
            }
        }
#pragma warning disable S2221 // Not clear what could be thrown here.
        catch (Exception)
#pragma warning restore S2221
        {
            SetFailedTextureProperties(texture);

            return;
        }

        LoadTextureDirectly(texture, bitmap);

        bitmap.Dispose();
    }

    /// <summary>
    ///     Load a texture directly from a bitmap.
    /// </summary>
    /// <param name="t">The texture to load.</param>
    /// <param name="bitmap">The bitmap to load.</param>
    public void LoadTextureDirectly(Texture t, Bitmap bitmap)
    {
        TextureList.Handle loadedTexture = renderPipeline.Textures.LoadTexture(bitmap, allowDiscard: true);
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

    private FileInfo GetTextureFile(Texture texture)
    {
        return preloadNameToPath.TryGetValue(texture.Name, out string? path)
            ? new FileInfo(path)
            : new FileInfo(texture.Name);
    }

    /// <inheritdoc />
    public override Color PixelColor(Texture texture, uint x, uint y, Color defaultColor)
    {
        if (texture.RendererData == null) return defaultColor;

        if (texture.Name != currentPixelColorSourceName)
        {
#pragma warning disable S2952 // Reference is overriden and thus disposed.
            currentPixelColorSource?.Dispose();
#pragma warning restore S2952

            currentPixelColorSource = new Bitmap(GetTextureFile(texture).FullName);
            currentPixelColorSourceName = texture.Name;
        }

        System.Drawing.Color pixel = currentPixelColorSource!.GetPixel((int) x, (int) y);

        return new Color(pixel.A, pixel.R, pixel.G, pixel.B);
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
}
