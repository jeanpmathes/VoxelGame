// <copyright file="DirectXRenderer.cs" company="Gwen.Net">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>Gwen.Net, jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Gwen.Net;
using Gwen.Net.Renderer;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Support;
using VoxelGame.Support.Definition;
using VoxelGame.Support.Graphics;
using VoxelGame.Support.Objects;
using Color = Gwen.Net.Color;
using Font = Gwen.Net.Font;
using FontStyle = System.Drawing.FontStyle;
using Point = Gwen.Net.Point;
using Rectangle = Gwen.Net.Rectangle;
using Size = Gwen.Net.Size;
using Texture = Gwen.Net.Texture;

namespace VoxelGame.UI.Platform.Renderer;

/// <summary>
///     Base class for DirectX renderers.
/// </summary>
public sealed class DirectXRenderer : RendererBase
{
    private readonly PooledList<DrawCall> drawCalls = new();
    private readonly Graphics graphics;
    private readonly RasterPipeline pipeline;
    private readonly Dictionary<string, string> preloadNameToPath = new();

    private readonly TextCache textCache;

    private readonly TextureList textures;
    private readonly ShaderBuffer<Vector2> uniformBuffer;

    private readonly PooledList<Draw2D.Vertex> vertexBuffer = new();
    private bool clipEnabled;

    private Bitmap? currentPixelColorSource;
    private string currentPixelColorSourceName = "";

    private uint currentVertexCount;

    private TextureList.Handle lastTexture;

    private bool textureDiscardAllowed;
    private bool textureEnabled;

    /// <summary>
    ///     Creates a new instance of <see cref="DirectXRenderer" />.
    /// </summary>
    internal DirectXRenderer(Client client, GwenGuiSettings settings)
    {
        graphics = Graphics.FromImage(new Bitmap(width: 1024, height: 1024));

        textCache = new TextCache(this);

        (pipeline, uniformBuffer) = client.CreateRasterPipeline<Vector2>(
            PipelineDescription.Create(settings.ShaderFile, ShaderPreset.Draw2D),
            settings.ShaderLoadingErrorCallback);

        client.AddDraw2dPipeline(pipeline, DoDraw);

        textures = new TextureList(client);

        foreach (TexturePreload texturePreload in settings.TexturePreloads)
        {
            Exception? exception = textures.LoadTexture(texturePreload.File,
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
    ///     Size of the text cache.
    /// </summary>
    public int TextCacheSize => textCache.Size;

    /// <summary>
    ///     Number of draw calls for the last frame.
    /// </summary>
    public int DrawCallCount => drawCalls.Count;

    /// <summary>
    ///     Gets the current vertex count.
    /// </summary>
    private uint VertexCount { get; set; }

    /// <summary>
    ///     Set the current draw color.
    /// </summary>
    public override Color DrawColor { get; set; }

    /// <summary>
    ///     Indicate that the loading phase is finished.
    ///     Textures that are loaded after this call can be freed, while texture created during loading are kept alive with the
    ///     client.
    /// </summary>
    internal void FinishLoading()
    {
        textureDiscardAllowed = true;
    }

    private void DoDraw(Draw2D drawer)
    {
        textCache.Evict();
        textures.UploadIfDirty(drawer);

        drawer.UploadBuffer(vertexBuffer.AsSpan());

        foreach (DrawCall drawCall in drawCalls)
        {
            bool texturedDraw = drawCall.Texture.IsValid;
            int index = drawCall.Texture.Index;

            drawer.DrawBuffer((drawCall.FirstVertex, drawCall.VertexCount), (uint) index, texturedDraw);

            ObjectPool<DrawCall>.Shared.Return(drawCall);
        }

        vertexBuffer.Clear();
        drawCalls.Clear();
    }

    /// <inheritdoc />
    public override void Dispose()
    {
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
        if (textureEnabled)
        {
            Flush();
            textureEnabled = false;
        }

        rect = Translate(rect);

        DrawRect(rect);
    }

    /// <inheritdoc />
    public override void StartClip()
    {
        clipEnabled = true;
    }

    /// <inheritdoc />
    public override void EndClip()
    {
        clipEnabled = false;
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

        bool differentTexture = lastTexture != handle;

        if (!textureEnabled || differentTexture) Flush();

        if (!textureEnabled) textureEnabled = true;

        if (differentTexture)
            lastTexture = handle;

        DrawRect(targetRect, u1, v1, u2, v2);
    }

    /// <inheritdoc />
    public override bool LoadFont(Font font)
    {
        font.RealSize = (float) Math.Ceiling(font.Size * Scale);

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

    /// <inheritdoc />
    public override void FreeFont(Font font)
    {
        if (font.RendererData == null) return;

        if (font.RendererData is not System.Drawing.Font sysFont)
            throw new InvalidOperationException("Freeing empty font");

        sysFont.Dispose();
        font.RendererData = null;
    }

    /// <inheritdoc />
    public override FontMetrics GetFontMetrics(Font font)
    {
        if (font.RendererData is not System.Drawing.Font sysFont
            || Math.Abs(font.RealSize - font.Size * Scale) > 2)
        {
            FreeFont(font);
            LoadFont(font);
            sysFont = (System.Drawing.Font) font.RendererData;
        }

        // from: http://csharphelper.com/blog/2014/08/get-font-metrics-in-c
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

    private float ConvertToPixels(float value, GraphicsUnit unit)
    {
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

        SizeF tabSize = graphics.MeasureString(
            "....",
            sysFont); //Spaces are not being picked up, let's just use .'s.

        textCache.StringFormat.SetTabStops(
            firstTabOffset: 0f,
            new[]
            {
                tabSize.Width
            });

        SizeF size = graphics.MeasureString(text, sysFont, System.Drawing.Point.Empty, textCache.StringFormat);

        return new Size(Util.Ceil(size.Width), Util.Ceil(size.Height));
    }

    /// <inheritdoc />
    public override void RenderText(Font font, Point position, string text)
    {
        Flush();

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
        currentVertexCount = 0;
        VertexCount = 0;
        clipEnabled = false;
        textureEnabled = false;
        lastTexture = TextureList.Handle.Invalid;
    }

    /// <inheritdoc />
    public override void End()
    {
        Flush();
    }

    private void Flush()
    {
        if (currentVertexCount == 0) return;

        DrawCall call = ObjectPool<DrawCall>.Shared.Get();
        call.FirstVertex = VertexCount;
        call.VertexCount = currentVertexCount;
        call.Texture = lastTexture;

        drawCalls.Add(call);

        VertexCount += currentVertexCount;
        currentVertexCount = 0;
    }

    private void PushVertex(int x, int y, Vector2 uv, ref Vector4 vertexColor)
    {
        vertexBuffer.Add(new Draw2D.Vertex
        {
            Position = new Vector2((short) x, (short) y),
            TextureCoordinate = uv,
            Color = vertexColor
        });

        currentVertexCount++;
    }

    private void DrawRect(Rectangle rect, float u1 = 0, float v1 = 0, float u2 = 1, float v2 = 1)
    {
        if (clipEnabled && PerformClip(ref rect, ref u1, ref v1, ref u2, ref v2)) return;

        float cR = DrawColor.R / 255f;
        float cG = DrawColor.G / 255f;
        float cB = DrawColor.B / 255f;
        float cA = DrawColor.A / 255f;
        Vector4 vertexColor = new(cR, cG, cB, cA);

        PushVertex(rect.X, rect.Y, new Vector2(u1, v1), ref vertexColor);
        PushVertex(rect.X + rect.Width, rect.Y, new Vector2(u2, v1), ref vertexColor);
        PushVertex(rect.X + rect.Width, rect.Y + rect.Height, new Vector2(u2, v2), ref vertexColor);
        PushVertex(rect.X, rect.Y, new Vector2(u1, v1), ref vertexColor);
        PushVertex(rect.X + rect.Width, rect.Y + rect.Height, new Vector2(u2, v2), ref vertexColor);
        PushVertex(rect.X, rect.Y + rect.Height, new Vector2(u1, v2), ref vertexColor);
    }

    /// <summary>
    ///     CPU scissors test.
    /// </summary>
    private bool PerformClip(ref Rectangle rect, ref float u1, ref float v1, ref float u2, ref float v2)
    {
        if (rect.Y < ClipRegion.Y)
        {
            int oldHeight = rect.Height;
            int delta = ClipRegion.Y - rect.Y;
            rect.Y = ClipRegion.Y;
            rect.Height -= delta;

            if (rect.Height <= 0) return true;

            float dv = delta / (float) oldHeight;

            v1 += dv * (v2 - v1);
        }

        if (rect.Y + rect.Height > ClipRegion.Y + ClipRegion.Height)
        {
            int oldHeight = rect.Height;
            int delta = rect.Y + rect.Height - (ClipRegion.Y + ClipRegion.Height);

            rect.Height -= delta;

            if (rect.Height <= 0) return true;

            float dv = delta / (float) oldHeight;

            v2 -= dv * (v2 - v1);
        }

        if (rect.X < ClipRegion.X)
        {
            int oldWidth = rect.Width;
            int delta = ClipRegion.X - rect.X;
            rect.X = ClipRegion.X;
            rect.Width -= delta;

            if (rect.Width <= 0) return true;

            float du = delta / (float) oldWidth;

            u1 += du * (u2 - u1);
        }

        if (rect.X + rect.Width > ClipRegion.X + ClipRegion.Width)
        {
            int oldWidth = rect.Width;
            int delta = rect.X + rect.Width - (ClipRegion.X + ClipRegion.Width);

            rect.Width -= delta;

            if (rect.Width <= 0) return true;

            float du = delta / (float) oldWidth;

            u2 -= du * (u2 - u1);
        }

        return false;
    }

    /// <inheritdoc />
    public override void LoadTexture(Texture texture, Action<Exception> errorCallback)
    {
        TextureList.Handle handle = TextureList.Handle.Invalid;

        if (preloadNameToPath.TryGetValue(texture.Name, out string? path))
        {
            handle = textures.GetTexture(path);
        }

        if (!handle.IsValid) handle = textures.GetTexture(texture.Name);

        if (!handle.IsValid)
        {
            Exception? exception = textures.LoadTexture(new FileInfo(texture.Name),
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
        TextureList.Handle loadedTexture = textures.LoadTexture(bitmap, allowDiscard: true);
        SetTextureProperties(t, loadedTexture);
    }

    private void SetTextureProperties(Texture texture, TextureList.Handle loadedTexture)
    {
        textures.DiscardTexture(GetRenderData(texture));

        Support.Objects.Texture? entry = textures.GetEntry(loadedTexture);

        Debug.Assert(loadedTexture.IsValid);
        Debug.Assert(entry != null);

        texture.Width = entry.Width;
        texture.Height = entry.Height;

        texture.RendererData = loadedTexture;
    }

    private void SetFailedTextureProperties(Texture texture)
    {
        texture.Width = 0;
        texture.Height = 0;

        textures.DiscardTexture(GetRenderData(texture));
        texture.RendererData = null;
    }

    /// <inheritdoc />
    public override void FreeTexture(Texture texture)
    {
        if (texture.RendererData == null) return;

        textures.DiscardTexture(GetRenderData(texture));

        texture.RendererData = null;
        texture.Width = 0;
        texture.Height = 0;
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
        uniformBuffer.Data = size;
    }

    private static TextureList.Handle GetRenderData(Texture texture)
    {
        if (texture.RendererData == null) return TextureList.Handle.Invalid;

        var handle = (TextureList.Handle) texture.RendererData;
        Debug.Assert(handle.IsValid);

        return handle;
    }

    private sealed class DrawCall
    {
        public uint FirstVertex { get; set; }
        public uint VertexCount { get; set; }
        public TextureList.Handle Texture { get; set; }
    }
}
