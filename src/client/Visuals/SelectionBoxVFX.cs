﻿// <copyright file="BoxRenderer.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;
using VoxelGame.Graphics.Data;
using VoxelGame.Graphics.Definition;
using VoxelGame.Graphics.Objects;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Visuals;

#pragma warning disable S101 // Naming.

/// <summary>
///     A VFX that shows instances of the <see cref="BoxCollider" /> struct.
///     For this multiple boxes are drawn.
/// </summary>
public sealed class SelectionBoxVFX : VFX
{
    private readonly VoxelGame.Graphics.Core.Client client;
    private readonly RasterPipeline pipeline;
    private readonly ShaderBuffer<Data> buffer;
    private Effect? effect;
    private BoxCollider? currentBox;
    private ColorS darkColor = ColorS.Black;
    private ColorS brightColor = ColorS.White;

    private SelectionBoxVFX(VoxelGame.Graphics.Core.Client client, RasterPipeline pipeline, ShaderBuffer<Data> buffer)
    {
        this.client = client;
        this.pipeline = pipeline;
        this.buffer = buffer;
    }

    /// <summary>
    ///     Set whether the VFX is enabled.
    /// </summary>
    public override Boolean IsEnabled
    {
        get => effect?.IsEnabled ?? false;
        set
        {
            Debug.Assert(effect != null);
            effect.IsEnabled = value;
        }
    }

    /// <summary>
    ///     Create a new <see cref="SelectionBoxVFX" />.
    /// </summary>
    internal static SelectionBoxVFX? Create(VoxelGame.Graphics.Core.Client client, PipelineFactory factory)
    {
        (RasterPipeline pipeline, ShaderBuffer<Data> buffer)? result
            = factory.LoadPipelineWithBuffer<Data>("Selection", new ShaderPresets.SpatialEffect(Topology.Line));

        return result is {pipeline: var rasterPipeline, buffer: var shaderBuffer}
            ? new SelectionBoxVFX(client, rasterPipeline, shaderBuffer)
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

        #pragma warning disable S2952 // Object has to be disposed here as it is nullified afterwards.
        effect.Dispose();
        #pragma warning restore S2952

        effect = null;
    }

    /// <summary>
    ///     Set the color to use o bright background.
    /// </summary>
    /// <param name="newColor">The new color.</param>
    public void SetDarkColor(ColorS newColor)
    {
        darkColor = newColor;
    }

    /// <summary>
    ///     Set the color to use on dark background.
    /// </summary>
    /// <param name="newColor">The new color.</param>
    public void SetBrightColor(ColorS newColor)
    {
        brightColor = newColor;
    }

    /// <inheritdoc />
    protected override void OnLogicUpdate()
    {
        Debug.Assert(effect != null);

        buffer.Data = new Data(darkColor.ToColor4(), brightColor.ToColor4());
    }

    /// <summary>
    ///     Set the box collider to display.
    /// </summary>
    /// <param name="boxCollider">The box collider to display.</param>
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

        for (var i = 0; i < boundingVolume.ChildCount; i++) BuildMeshData(boundingVolume[i], vertices);
    }

    private static void BuildMeshDataForTopLevelBox(BoundingVolume boundingVolume, PooledList<EffectVertex> vertices)
    {
        (Single minX, Single minY, Single minZ) = (Vector3) boundingVolume.Min;
        (Single maxX, Single maxY, Single maxZ) = (Vector3) boundingVolume.Max;

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
#pragma warning disable S3242 // Concrete type used for performance.
    private static void AddLine(PooledList<EffectVertex> vertices, Vector3 a, Vector3 b)
    #pragma warning restore S3242
    {
        vertices.Add(new EffectVertex {Position = a, Data = 0});
        vertices.Add(new EffectVertex {Position = b, Data = 0});
    }

    /// <summary>
    ///     Data used by the shader.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    private readonly struct Data : IEquatable<Data>
    {
        /// <summary>
        ///     Create the shader data.
        /// </summary>
        public Data(Color4 darkColor, Color4 brightColor)
        {
            DarkColor = darkColor;
            BrightColor = brightColor;
        }

        /// <summary>
        ///     The color to use with bright background.
        /// </summary>
        [FieldOffset(0 * ShaderBuffers.FieldOffset)]
        public readonly Color4 DarkColor;

        /// <summary>
        ///     The color to use with dark background.
        /// </summary>
        [FieldOffset(1 * ShaderBuffers.FieldOffset)]
        public readonly Color4 BrightColor;

        /// <summary>
        ///     Check equality.
        /// </summary>
        public Boolean Equals(Data other)
        {
            return (DarkColor, BrightColor) == (other.DarkColor, other.BrightColor);
        }

        /// <inheritdoc />
        public override Boolean Equals(Object? obj)
        {
            return obj is Data other && Equals(other);
        }

        /// <inheritdoc />
        public override Int32 GetHashCode()
        {
            return HashCode.Combine(DarkColor, BrightColor);
        }

        /// <summary>
        ///     The equality operator.
        /// </summary>
        public static Boolean operator ==(Data left, Data right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     The inequality operator.
        /// </summary>
        public static Boolean operator !=(Data left, Data right)
        {
            return !left.Equals(right);
        }
    }

    #region DISPOSABLE

    private Boolean disposed;

    /// <inheritdoc />
    protected override void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing)
            effect?.Dispose();
        else Throw.ForMissedDispose(this);

        base.Dispose(disposing);

        disposed = true;
    }

    #endregion DISPOSABLE
}
