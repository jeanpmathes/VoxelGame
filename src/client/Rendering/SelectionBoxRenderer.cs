// <copyright file="BoxRenderer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;
using VoxelGame.Support.Data;
using VoxelGame.Support.Definition;
using VoxelGame.Support.Objects;

namespace VoxelGame.Client.Rendering;

/// <summary>
///     A renderer that renders instances of the <see cref="BoxCollider" /> struct.
///     For this multiple boxes are drawn.
/// </summary>
public sealed class SelectionBoxRenderer : Renderer
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<SelectionBoxRenderer>();

    private readonly Support.Core.Client client;
    private readonly RasterPipeline pipeline;
    private readonly ShaderBuffer<Data> buffer;

    private Effect? effect;

    private BoxCollider? currentBox;

    private Color darkColor = Color.Black;
    private Color brightColor = Color.White;

    private SelectionBoxRenderer(Support.Core.Client client, RasterPipeline pipeline, ShaderBuffer<Data> buffer)
    {
        this.client = client;
        this.pipeline = pipeline;
        this.buffer = buffer;
    }

    /// <summary>
    /// Set whether the renderer is enabled.
    /// </summary>
    public override bool IsEnabled
    {
        get => effect?.IsEnabled ?? false;
        set
        {
            Debug.Assert(effect != null);
            effect.IsEnabled = value;
        }
    }

    /// <summary>
    /// Create a new <see cref="SelectionBoxRenderer"/>.
    /// </summary>
    public static SelectionBoxRenderer? Create(Support.Core.Client client, Pipelines pipelines)
    {
        (RasterPipeline pipeline, ShaderBuffer<Data> buffer)? result
            = pipelines.LoadPipelineWithBuffer<Data>(client, "Selection", new ShaderPresets.SpatialEffect(Topology.Line));

        return result is {pipeline: var rasterPipeline, buffer: var shaderBuffer}
            ? new SelectionBoxRenderer(client, rasterPipeline, shaderBuffer)
            : null;
    }

    /// <inheritdoc />
    protected override void OnSetUp()
    {
        Debug.Assert(effect == null);

        effect = client.Space.CreateEffect(pipeline);
    }

    /// <inheritdoc />
    protected override void OnTearDown()
    {
        Debug.Assert(effect != null);

        effect.Return();
        effect = null;
    }

    /// <summary>
    ///     Set the color to use o bright background.
    /// </summary>
    /// <param name="newColor">The new color.</param>
    public void SetDarkColor(Color newColor)
    {
        darkColor = newColor;
    }

    /// <summary>
    ///     Set the color to use on dark background.
    /// </summary>
    /// <param name="newColor">The new color.</param>
    public void SetBrightColor(Color newColor)
    {
        brightColor = newColor;
    }

    /// <inheritdoc />
    protected override void OnUpdate()
    {
        Debug.Assert(effect != null);

        buffer.Data = new Data(darkColor.ToVector3(), brightColor.ToVector3());
    }

    /// <summary>
    ///     Set the box collider to render.
    /// </summary>
    /// <param name="boxCollider">The box collider to render.</param>
    public void SetBox(BoxCollider boxCollider)
    {
        if (effect == null) return;

        if (currentBox == boxCollider) return;

        currentBox = boxCollider;
        effect.Position = boxCollider.Position;

        using PooledList<EffectVertex> vertices = new();
        BuildMeshData(boxCollider.Volume, vertices);

        effect.SetNewVertices(vertices.AsSpan());
    }

    private static void BuildMeshData(BoundingVolume boundingVolume, PooledList<EffectVertex> vertices)
    {
        BuildMeshDataForTopLevelBox(boundingVolume, vertices);

        if (boundingVolume.ChildCount == 0) return;

        for (var i = 0; i < boundingVolume.ChildCount; i++)
        {
            BuildMeshData(boundingVolume[i], vertices);
        }
    }

    private static void BuildMeshDataForTopLevelBox(BoundingVolume boundingVolume, PooledList<EffectVertex> vertices)
    {
        (float minX, float minY, float minZ) = boundingVolume.Min.ToVector3();
        (float maxX, float maxY, float maxZ) = boundingVolume.Max.ToVector3();

        // The four bottom lines:
        AddLine(vertices, (minX, minY, minZ), (maxX, minY, minZ));
        AddLine(vertices, (minX, minY, maxZ), (maxX, minY, maxZ));
        AddLine(vertices, (minX, minY, minZ), (minX, minY, maxZ));
        AddLine(vertices, (maxX, minY, minZ), (maxX, minY, maxZ));

        // The four top lines:
        AddLine(vertices, (minX, maxY, minZ), (maxX, maxY, minZ));
        AddLine(vertices, (minX, maxY, maxZ), (maxX, maxY, maxZ));
        AddLine(vertices, (minX, maxY, minZ), (minX, maxY, maxZ));
        AddLine(vertices, (maxX, maxY, minZ), (maxX, maxY, maxZ));

        // The four vertical lines:
        AddLine(vertices, (minX, minY, minZ), (minX, maxY, minZ));
        AddLine(vertices, (maxX, minY, minZ), (maxX, maxY, minZ));
        AddLine(vertices, (minX, minY, maxZ), (minX, maxY, maxZ));
        AddLine(vertices, (maxX, minY, maxZ), (maxX, maxY, maxZ));
    }

    private static void AddLine(PooledList<EffectVertex> vertices, Vector3 a, Vector3 b)
    {
        vertices.Add(new EffectVertex {Position = a, Data = 0});
        vertices.Add(new EffectVertex {Position = b, Data = 0});
    }

    #region IDisposable Support

    /// <inheritdoc />
    protected override void OnDispose(bool disposing)
    {
        if (!disposing)
            logger.LogWarning(
                Events.LeakedNativeObject,
                "Renderer disposed by GC without freeing storage");
    }

    #endregion IDisposable Support

    /// <summary>
    ///     Data used by the shader.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    private readonly struct Data : IEquatable<Data>
    {
        /// <summary>
        ///     Create the shader data.
        /// </summary>
        public Data(Vector3 darkColor, Vector3 brightColor)
        {
            DarkColor = darkColor;
            BrightColor = brightColor;
        }

        /// <summary>
        ///     The color to use with bright background.
        /// </summary>
        [FieldOffset(0 * ShaderBuffers.FieldOffset)]
        public readonly Vector3 DarkColor;

        /// <summary>
        ///     The color to use with dark background.
        /// </summary>
        [FieldOffset(1 * ShaderBuffers.FieldOffset)]
        public readonly Vector3 BrightColor;

        /// <summary>
        ///     Check equality.
        /// </summary>
        public bool Equals(Data other)
        {
            return (DarkColor, BrightColor) == (other.DarkColor, other.BrightColor);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is Data other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(DarkColor, BrightColor);
        }

        /// <summary>
        ///     The equality operator.
        /// </summary>
        public static bool operator ==(Data left, Data right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     The inequality operator.
        /// </summary>
        public static bool operator !=(Data left, Data right)
        {
            return !left.Equals(right);
        }
    }
}
