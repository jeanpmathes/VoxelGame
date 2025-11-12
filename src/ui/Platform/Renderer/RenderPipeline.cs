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
using VoxelGame.Graphics.Core;
using VoxelGame.Graphics.Definition;
using VoxelGame.Graphics.Graphics;
using VoxelGame.Graphics.Objects;
using VoxelGame.Logging;
using VoxelGame.Toolkit.Utilities;
using Timer = VoxelGame.Core.Profiling.Timer;

namespace VoxelGame.UI.Platform.Renderer;

/// <summary>
///     Does the actual issuing of draw calls and managing of GPU resources.
/// </summary>
public sealed class RenderPipeline : IDisposable
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<RenderPipeline>();
    private readonly ShaderBuffer<Vector2> buffer;

    private readonly IDisposable disposable;

    private readonly PooledList<DrawCall> drawCalls = new();
    private readonly RasterPipeline pipeline;
    private readonly Action preDraw;

    private readonly RendererBase renderer;

    private readonly PooledList<Draw2D.Vertex> vertexBuffer = new();

    private UInt32 currentVertexCount;
    private UInt32 totalVertexCount;

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
    public Boolean IsClippingEnabled { get; set; }

    /// <summary>
    ///     Creates a new render pipeline.
    /// </summary>
    public static RenderPipeline? Create(
        Client client,
        RendererBase rendererBase,
        Action preDrawAction,
        FileInfo shader,
        Action<String> errorCallback)
    {
        (RasterPipeline pipeline, ShaderBuffer<Vector2>)? result = client.CreateRasterPipeline<Vector2>(
            RasterPipelineDescription.Create(shader, new ShaderPresets.Draw2D()),
            errorCallback);

        return result == null ? null : new RenderPipeline(client, rendererBase, preDrawAction, result.Value);
    }

    /// <summary>
    ///     Push a new rectangle to the vertex buffer.
    /// </summary>
    public void PushRect(Rectangle rect, Single u1 = 0, Single v1 = 0, Single u2 = 1, Single v2 = 1)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        if (IsClippingEnabled && PerformClip(ref rect, ref u1, ref v1, ref u2, ref v2)) return;

        Single cR = renderer.DrawColor.R / 255f;
        Single cG = renderer.DrawColor.G / 255f;
        Single cB = renderer.DrawColor.B / 255f;
        Single cA = renderer.DrawColor.A / 255f;
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
    private Boolean PerformClip(ref Rectangle rect, ref Single u1, ref Single v1, ref Single u2, ref Single v2)
    {
        if (rect.Y < renderer.ClipRegion.Y)
        {
            Int32 oldHeight = rect.Height;
            Int32 delta = renderer.ClipRegion.Y - rect.Y;
            rect.Y = renderer.ClipRegion.Y;
            rect.Height -= delta;

            if (rect.Height <= 0) return true;

            Single dv = delta / (Single) oldHeight;

            v1 += dv * (v2 - v1);
        }

        if (rect.Y + rect.Height > renderer.ClipRegion.Y + renderer.ClipRegion.Height)
        {
            Int32 oldHeight = rect.Height;
            Int32 delta = rect.Y + rect.Height - (renderer.ClipRegion.Y + renderer.ClipRegion.Height);

            rect.Height -= delta;

            if (rect.Height <= 0) return true;

            Single dv = delta / (Single) oldHeight;

            v2 -= dv * (v2 - v1);
        }

        if (rect.X < renderer.ClipRegion.X)
        {
            Int32 oldWidth = rect.Width;
            Int32 delta = renderer.ClipRegion.X - rect.X;
            rect.X = renderer.ClipRegion.X;
            rect.Width -= delta;

            if (rect.Width <= 0) return true;

            Single du = delta / (Single) oldWidth;

            u1 += du * (u2 - u1);
        }

        if (rect.X + rect.Width > renderer.ClipRegion.X + renderer.ClipRegion.Width)
        {
            Int32 oldWidth = rect.Width;
            Int32 delta = rect.X + rect.Width - (renderer.ClipRegion.X + renderer.ClipRegion.Width);

            rect.Width -= delta;

            if (rect.Width <= 0) return true;

            Single du = delta / (Single) oldWidth;

            u2 -= du * (u2 - u1);
        }

        return false;
    }

    /// <summary>
    ///     Push a new draw call, it will draw all previously pushed rectangles since the last call.
    /// </summary>
    public void PushCall(TextureList.Handle texture)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        if (currentVertexCount == 0) return;

        DrawCall call = SimpleObjectPool<DrawCall>.Shared.Get();
        call.FirstVertex = totalVertexCount;
        call.VertexCount = currentVertexCount;
        call.Texture = texture;

        drawCalls.Add(call);

        totalVertexCount += currentVertexCount;
        currentVertexCount = 0;
    }

    private void PushVertex(Int32 x, Int32 y, Vector2 uv, ref Color4 vertexColor)
    {
        vertexBuffer.Add(new Draw2D.Vertex
        {
            Position = new Vector2((Int16) x, (Int16) y),
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
            Boolean texturedDraw = drawCall.Texture.IsValid;
            Int32 index = drawCall.Texture.Index;

            drawer.DrawBuffer((drawCall.FirstVertex, drawCall.VertexCount), (UInt32) index, texturedDraw);

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
        ExceptionTools.ThrowIfDisposed(disposed);

        buffer.Data = size;
    }

    private sealed class DrawCall
    {
        public UInt32 FirstVertex { get; set; }
        public UInt32 VertexCount { get; set; }
        public TextureList.Handle Texture { get; set; }
    }

    #region DISPOSABLE

    private Boolean disposed;

    private void Dispose(Boolean disposing)
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

    #endregion DISPOSABLE
}
