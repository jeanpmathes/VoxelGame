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
using Texture = VoxelGame.Support.Objects.Texture;

namespace VoxelGame.UI.Platform;

/// <summary>
///     Base class for DirectX renderers.
/// </summary>
public sealed class DirectXRenderer : RendererBase
{
    private const int MaxVerts = 4096;

    private readonly Client client;

    private readonly Graphics graphics;
    private readonly RasterPipeline pipeline;

    private readonly Dictionary<string, int> preloadedTextures = new();

    private readonly Dictionary<string, string> preloadNameToPath = new();

    private readonly Dictionary<Tuple<string, Font>, TextRenderer> stringCache;
    private readonly StringFormat stringFormat;
    private readonly List<Texture> textures = new();
    private readonly ShaderBuffer<Vector2> uniformBuffer;

    private readonly Draw2D.Vertex[] vertices;
    private bool clipEnabled;

    private Bitmap? currentPixelColorSource;
    private string currentPixelColorSourceName = "";

    private int currentVertexCount;

    private bool dirtyTextures = true;

    private Draw2D? drawer;

    private int lastTextureIndex = -1;
    private bool textureEnabled;

    /// <summary>
    ///     Creates a new instance of <see cref="DirectXRenderer" />.
    /// </summary>
    internal DirectXRenderer(Client client, Action draw, GwenGuiSettings settings)
    {
        this.client = client;
        vertices = new Draw2D.Vertex[MaxVerts];

        stringCache = new Dictionary<Tuple<string, Font>, TextRenderer>();
        graphics = Graphics.FromImage(new Bitmap(width: 1024, height: 1024));
        stringFormat = new StringFormat(StringFormat.GenericTypographic);
        stringFormat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;

        (pipeline, uniformBuffer) = client.CreateRasterPipeline<Vector2>(
            PipelineDescription.Create(settings.ShaderFile, ShaderPreset.Draw2D),
            settings.ShaderLoadingErrorCallback);

        client.AddDraw2dPipeline(pipeline, CreateDrawCallback(draw));

        // The Draw2D pipeline requires at least one texture.
        using Bitmap sentinel = Texture.CreateFallback(resolution: 1);
        textures.Add(client.LoadTexture(sentinel));

        foreach (TexturePreload texturePreload in settings.TexturePreloads)
            try
            {
                using Bitmap bitmap = new(texturePreload.File.FullName);
                Texture texture = client.LoadTexture(bitmap);

                textures.Add(texture);
                preloadedTextures.Add(texturePreload.Name, textures.Count - 1);

                preloadNameToPath.Add(texturePreload.Name, texturePreload.File.FullName);
            }
#pragma warning disable S2221 // Not clear what could be thrown here.
            catch (Exception e)
#pragma warning restore S2221
            {
                settings.TexturePreloadErrorCallback(texturePreload, e);
            }
    }

    /// <summary>
    ///     Size of the text cache.
    /// </summary>
    public int TextCacheSize => stringCache.Count;

    /// <summary>
    ///     Number of draw calls for the last frame.
    /// </summary>
    public int DrawCallCount { get; private set; }

    /// <summary>
    ///     Gets the current vertex count.
    /// </summary>
    public int VertexCount { get; private set; }

    /// <summary>
    ///     Set the current draw color.
    /// </summary>
    public override Color DrawColor { get; set; }

    private Action<Draw2D> CreateDrawCallback(Action draw)
    {
        return draw2D =>
        {
            drawer = draw2D;

            if (dirtyTextures)
            {
                drawer.Value.InitializeTextures(textures);
                dirtyTextures = false;
            }

            draw();

            drawer = null;
        };
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        FlushTextCache();
        currentPixelColorSource?.Dispose();

        base.Dispose();
    }

    /// <inheritdoc />
    protected override void OnScaleChanged(float oldScale)
    {
        FlushTextCache();
    }

    /// <summary>
    ///     Flushes the text cache.
    /// </summary>
    private void FlushTextCache()
    {
        foreach (TextRenderer textRenderer in stringCache.Values) textRenderer.Dispose();
        stringCache.Clear();
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
    public override void DrawTexturedRect(Gwen.Net.Texture t, Rectangle targetRect, float u1 = 0, float v1 = 0, float u2 = 1,
        float v2 = 1)
    {
        if (null == t.RendererData)
        {
            DrawMissingImage(targetRect);

            return;
        }

        var data = (TextureRendererData) t.RendererData;
        targetRect = Translate(targetRect);

        bool differentTexture = data.textureIndex != lastTextureIndex;

        if (!textureEnabled || differentTexture) Flush();

        if (!textureEnabled) textureEnabled = true;

        if (differentTexture)
            lastTextureIndex = data.textureIndex;

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

        Tuple<string, Font> key = new(text, font);

        if (stringCache.ContainsKey(key))
        {
            Gwen.Net.Texture tex = stringCache[key].Texture;

            return new Size(tex.Width, tex.Height);
        }

        Debug.Assert(sysFont != null);

        SizeF tabSize = graphics.MeasureString(
            "....",
            sysFont); //Spaces are not being picked up, let's just use .'s.

        stringFormat.SetTabStops(
            firstTabOffset: 0f,
            new[]
            {
                tabSize.Width
            });

        SizeF size = graphics.MeasureString(text, sysFont, System.Drawing.Point.Empty, stringFormat);

        return new Size(Util.Ceil(size.Width), Util.Ceil(size.Height));
    }

    /// <inheritdoc />
    public override void RenderText(Font font, Point position, string text)
    {
        Flush();

        if (font.RendererData is not System.Drawing.Font sysFont
            || Math.Abs(font.RealSize - font.Size * Scale) > 2)
        {
            FreeFont(font);
            LoadFont(font);
            sysFont = (System.Drawing.Font) font.RendererData;
        }

        Tuple<string, Font> key = new(text, font);

        if (!stringCache.ContainsKey(key))
        {
            Size size = MeasureText(font, text);
            TextRenderer tr = new(size.Width, size.Height, this);
            tr.DrawString(text, sysFont, Brushes.White, Point.Zero, stringFormat); // renders string on the texture

            DrawTexturedRect(
                tr.Texture,
                new Rectangle(position.X, position.Y, tr.Texture.Width, tr.Texture.Height));

            stringCache[key] = tr;
        }
        else
        {
            TextRenderer tr = stringCache[key];

            DrawTexturedRect(
                tr.Texture,
                new Rectangle(position.X, position.Y, tr.Texture.Width, tr.Texture.Height));
        }
    }

    /// <inheritdoc />
    public override void Begin()
    {
        currentVertexCount = 0;
        VertexCount = 0;
        DrawCallCount = 0;
        clipEnabled = false;
        textureEnabled = false;
        lastTextureIndex = -1;
    }

    /// <inheritdoc />
    public override void End()
    {
        Flush();
    }

    private void Flush()
    {
        Debug.Assert(drawer.HasValue);

        if (currentVertexCount == 0) return;

        drawer.Value.DrawBuffer(vertices[..currentVertexCount], (uint) lastTextureIndex, textureEnabled);

        DrawCallCount++;
        VertexCount += currentVertexCount;
        currentVertexCount = 0;
    }

    private void PushVertex(int x, int y, Vector2 uv, ref Vector4 vertexColor)
    {
        vertices[currentVertexCount].Position = new Vector2((short) x, (short) y);
        vertices[currentVertexCount].TextureCoordinate = uv;
        vertices[currentVertexCount].Color = vertexColor;

        currentVertexCount++;
    }

    private void DrawRect(Rectangle rect, float u1 = 0, float v1 = 0, float u2 = 1, float v2 = 1)
    {
        if (currentVertexCount + 6 >= MaxVerts) Flush();

        if (clipEnabled && PerformClip(rect, ref u1, ref v1, ref u2, ref v2)) return;

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
    private bool PerformClip(Rectangle rect, ref float u1, ref float v1, ref float u2, ref float v2)
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
    public override void LoadTexture(Gwen.Net.Texture t, Action<Exception> errorCallback)
    {
        // todo: free previous texture if needed

        if (preloadedTextures.TryGetValue(t.Name, out int textureIndex))
        {
            Texture texture = textures[textureIndex];

            t.Width = texture.Width;
            t.Height = texture.Height;

            t.RendererData = new TextureRendererData
            {
                textureIndex = textureIndex
            };
        }
        else
        {
            t.Failed = true;
            // todo: load texture - allow post-init texture loading
        }
    }

    /// <inheritdoc />
    public override void LoadTextureRaw(Gwen.Net.Texture t, byte[] pixelData)
    {
        // todo: free previous texture if needed

        Bitmap bmp;

        try
        {
            unsafe
            {
                fixed (byte* ptr = &pixelData[0])
                {
                    bmp = new Bitmap(t.Width, t.Height, 4 * t.Width, PixelFormat.Format32bppArgb, (IntPtr) ptr);
                }
            }
        }
#pragma warning disable S2221 // Not clear what could be thrown here.
        catch (Exception)
#pragma warning restore S2221
        {
            t.Failed = true;

            return;
        }

        // todo: load texture - allow post-init texture loading

        bmp.Dispose();
    }

    /// <inheritdoc />
    public override void FreeTexture(Gwen.Net.Texture t)
    {
        if (t.RendererData == null) return;

        // todo: free texture if needed

        t.RendererData = null;
    }

    private FileInfo GetTextureFile(Gwen.Net.Texture texture)
    {
        return preloadNameToPath.TryGetValue(texture.Name, out string? path)
            ? new FileInfo(path)
            : new FileInfo(texture.Name);
    }

    /// <inheritdoc />
    public override Color PixelColor(Gwen.Net.Texture texture, uint x, uint y, Color defaultColor)
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

        return new Color(pixel.R, pixel.G, pixel.B, pixel.A);
    }

    /// <summary>
    ///     Call when the window is resized.
    /// </summary>
    /// <param name="size">New window size.</param>
    public void Resize(Vector2 size)
    {
        uniformBuffer.Data = size;
    }

    private sealed class TextureRendererData
    {
        public int textureIndex;
    }
}
