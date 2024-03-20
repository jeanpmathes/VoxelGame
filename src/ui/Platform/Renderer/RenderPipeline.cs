// <copyright file="RenderPipeline.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using System.IO;
using Gwen.Net;
using Gwen.Net.Renderer;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;
using VoxelGame.Support.Core;
using VoxelGame.Support.Definition;
using VoxelGame.Support.Graphics;
using VoxelGame.Support.Objects;
using Timer = VoxelGame.Core.Profiling.Timer;

namespace VoxelGame.UI.Platform.Renderer;

/// <summary>
///     Does the actual issuing of draw calls and managing of GPU resources.
/// </summary>
public sealed class RenderPipeline : IDisposable
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<RenderPipeline>();

    private readonly PooledList<DrawCall> drawCalls = new();
    private readonly RasterPipeline pipeline;
    private readonly Action preDraw;

    private readonly IDisposable disposable;

    private readonly RendererBase renderer;
    private readonly ShaderBuffer<Vector2> buffer;

    private readonly PooledList<Draw2D.Vertex> vertexBuffer = new();

    private uint currentVertexCount;
    private uint totalVertexCount;

    /// <summary>
    ///     Creates a new render pipeline.
    /// </summary>
    private RenderPipeline(Client client, RendererBase renderer, Action preDraw, (RasterPipeline, ShaderBuffer<Vector2>) raster)
    {
        this.renderer = renderer;
        this.preDraw = preDraw;

        (pipeline, buffer) = raster;

        disposable = client.AddDraw2dPipeline(pipeline, Draw2D.Foreground, Draw);

        Textures = new TextureList(client);
    }

    /// <summary>
    ///     Gets the texture list.
    /// </summary>
    public TextureList Textures { get; }

    /// <summary>
    ///     Whether CPU clipping is enabled.
    /// </summary>
    public bool IsClippingEnabled { get; set; }

    /// <summary>
    ///     Creates a new render pipeline.
    /// </summary>
    public static RenderPipeline? Create(
        Client client,
        RendererBase rendererBase,
        Action preDrawAction,
        FileInfo shader,
        Action<string> errorCallback)
    {
        (RasterPipeline pipeline, ShaderBuffer<Vector2>)? result = client.CreateRasterPipeline<Vector2>(
            RasterPipelineDescription.Create(shader, new ShaderPresets.Draw2D()),
            errorCallback);

        return result == null ? null : new RenderPipeline(client, rendererBase, preDrawAction, result.Value);
    }

    /// <summary>
    ///     Push a new rectangle to the vertex buffer.
    /// </summary>
    public void PushRect(Rectangle rect, float u1 = 0, float v1 = 0, float u2 = 1, float v2 = 1)
    {
        Throw.IfDisposed(disposed);

        if (IsClippingEnabled && PerformClip(ref rect, ref u1, ref v1, ref u2, ref v2)) return;

        float cR = renderer.DrawColor.R / 255f;
        float cG = renderer.DrawColor.G / 255f;
        float cB = renderer.DrawColor.B / 255f;
        float cA = renderer.DrawColor.A / 255f;
        Color4 color = new(cR, cG, cB, cA);

        PushVertex(rect.X, rect.Y, new Vector2(u1, v1), ref color);
        PushVertex(rect.X + rect.Width, rect.Y, new Vector2(u2, v1), ref color);
        PushVertex(rect.X + rect.Width, rect.Y + rect.Height, new Vector2(u2, v2), ref color);
        PushVertex(rect.X, rect.Y, new Vector2(u1, v1), ref color);
        PushVertex(rect.X + rect.Width, rect.Y + rect.Height, new Vector2(u2, v2), ref color);
        PushVertex(rect.X, rect.Y + rect.Height, new Vector2(u1, v2), ref color);
    }

    /// <summary>
    ///     CPU scissors test.
    /// </summary>
    private bool PerformClip(ref Rectangle rect, ref float u1, ref float v1, ref float u2, ref float v2)
    {
        if (rect.Y < renderer.ClipRegion.Y)
        {
            int oldHeight = rect.Height;
            int delta = renderer.ClipRegion.Y - rect.Y;
            rect.Y = renderer.ClipRegion.Y;
            rect.Height -= delta;

            if (rect.Height <= 0) return true;

            float dv = delta / (float) oldHeight;

            v1 += dv * (v2 - v1);
        }

        if (rect.Y + rect.Height > renderer.ClipRegion.Y + renderer.ClipRegion.Height)
        {
            int oldHeight = rect.Height;
            int delta = rect.Y + rect.Height - (renderer.ClipRegion.Y + renderer.ClipRegion.Height);

            rect.Height -= delta;

            if (rect.Height <= 0) return true;

            float dv = delta / (float) oldHeight;

            v2 -= dv * (v2 - v1);
        }

        if (rect.X < renderer.ClipRegion.X)
        {
            int oldWidth = rect.Width;
            int delta = renderer.ClipRegion.X - rect.X;
            rect.X = renderer.ClipRegion.X;
            rect.Width -= delta;

            if (rect.Width <= 0) return true;

            float du = delta / (float) oldWidth;

            u1 += du * (u2 - u1);
        }

        if (rect.X + rect.Width > renderer.ClipRegion.X + renderer.ClipRegion.Width)
        {
            int oldWidth = rect.Width;
            int delta = rect.X + rect.Width - (renderer.ClipRegion.X + renderer.ClipRegion.Width);

            rect.Width -= delta;

            if (rect.Width <= 0) return true;

            float du = delta / (float) oldWidth;

            u2 -= du * (u2 - u1);
        }

        return false;
    }

    /// <summary>
    ///     Push a new draw call, it will draw all previously pushed rectangles since the last call.
    /// </summary>
    public void PushCall(TextureList.Handle texture)
    {
        Throw.IfDisposed(disposed);

        if (currentVertexCount == 0) return;

        DrawCall call = SimpleObjectPool<DrawCall>.Shared.Get();
        call.FirstVertex = totalVertexCount;
        call.VertexCount = currentVertexCount;
        call.Texture = texture;

        drawCalls.Add(call);

        totalVertexCount += currentVertexCount;
        currentVertexCount = 0;
    }

    private void PushVertex(int x, int y, Vector2 uv, ref Color4 vertexColor)
    {
        vertexBuffer.Add(new Draw2D.Vertex
        {
            Position = new Vector2((short) x, (short) y),
            TextureCoordinate = uv,
            Color = vertexColor
        });

        currentVertexCount++;
    }

    private void Draw(Draw2D drawer)
    {
        Debug.Assert((drawCalls.Count > 0).Implies(vertexBuffer.Count > 0));

        using Timer? timer = logger.BeginTimedScoped("UI Draw");

        preDraw();
        Textures.UploadIfDirty(drawer);

        if (vertexBuffer.Count > 0)
            drawer.UploadBuffer(vertexBuffer.AsSpan());

        foreach (DrawCall drawCall in drawCalls)
        {
            bool texturedDraw = drawCall.Texture.IsValid;
            int index = drawCall.Texture.Index;

            drawer.DrawBuffer((drawCall.FirstVertex, drawCall.VertexCount), (uint) index, texturedDraw);

            SimpleObjectPool<DrawCall>.Shared.Return(drawCall);
        }

        Reset();
    }

    /// <summary>
    ///     Clear all previously pushed rectangles and draw calls.
    ///     Should not be called directly after submitting the draw calls,
    ///     as these will only be drawn before the next frame.
    /// </summary>
    public void Reset()
    {
        vertexBuffer.Clear();
        drawCalls.Clear();

        currentVertexCount = 0;
        totalVertexCount = 0;
    }

    /// <summary>
    ///     Call this when the window is resized.
    /// </summary>
    public void Resize(Vector2 size)
    {
        Throw.IfDisposed(disposed);

        buffer.Data = size;
    }

    private sealed class DrawCall
    {
        public uint FirstVertex { get; set; }
        public uint VertexCount { get; set; }
        public TextureList.Handle Texture { get; set; }
    }

    #region IDisposable Support

    private bool disposed;

    private void Dispose(bool disposing)
    {
        if (disposed) return;
        if (!disposing) return;

        Textures.Dispose();

        disposable.Dispose();

        drawCalls.Dispose();
        vertexBuffer.Dispose();

        disposed = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     The finalizer.
    /// </summary>
    ~RenderPipeline()
    {
        Dispose(disposing: false);
    }

    #endregion IDisposable Support
}
