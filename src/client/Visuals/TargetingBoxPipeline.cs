// <copyright file="BoxRenderer.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Runtime.InteropServices;
using OpenTK.Mathematics;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;
using VoxelGame.Graphics.Definition;
using VoxelGame.Graphics.Objects;

namespace VoxelGame.Client.Visuals;

/// <summary>
///     A rendering pipeline for block targeting visualization based on the <see cref="BoxCollider" /> struct.
///     Create a <see cref="TargetingBoxEffect"/> to use this pipeline.
/// </summary>
public sealed class TargetingBoxPipeline : IDisposable
{
    private readonly VoxelGame.Graphics.Core.Client client;
    private readonly RasterPipeline pipeline;
    private readonly ShaderBuffer<Data> buffer;
    
    private ColorS darkColor = ColorS.Black;
    private ColorS brightColor = ColorS.White;
    private Boolean colorsDirty = true;

    private TargetingBoxPipeline(VoxelGame.Graphics.Core.Client client, RasterPipeline pipeline, ShaderBuffer<Data> buffer)
    {
        this.client = client;
        this.pipeline = pipeline;
        this.buffer = buffer;
    }

    /// <summary>
    ///     Create a new <see cref="TargetingBoxPipeline" />.
    /// </summary>
    internal static TargetingBoxPipeline? Create(VoxelGame.Graphics.Core.Client client, PipelineFactory factory)
    {
        (RasterPipeline pipeline, ShaderBuffer<Data> buffer)? result
            = factory.LoadPipelineWithBuffer<Data>("Targeting", new ShaderPresets.SpatialEffect(Topology.Line));

        return result is {pipeline: var rasterPipeline, buffer: var shaderBuffer}
            ? new TargetingBoxPipeline(client, rasterPipeline, shaderBuffer)
            : null;
    }

    /// <summary>
    /// Create a new <see cref="TargetingBoxEffect"/> using this pipeline.
    /// </summary>
    /// <returns>The created effect.</returns>
    public TargetingBoxEffect CreateEffect()
    {
        return new TargetingBoxEffect(client.Space.CreateEffect(pipeline), this);
    }

    /// <summary>
    ///     Set the color to use o bright background.
    /// </summary>
    /// <param name="newColor">The new color.</param>
    public void SetDarkColor(ColorS newColor)
    {
        darkColor = newColor;
        colorsDirty = true;
    }

    /// <summary>
    ///     Set the color to use on dark background.
    /// </summary>
    /// <param name="newColor">The new color.</param>
    public void SetBrightColor(ColorS newColor)
    {
        brightColor = newColor;
        colorsDirty = true;
    }

    /// <summary>
    /// Update the data used by the pipeline.
    /// </summary>
    public void UpdateData()
    {
        if (!colorsDirty) return;
        
        buffer.Data = new Data(darkColor.ToColor4(), brightColor.ToColor4());
        
        colorsDirty = false;
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
    
    /// <inheritdoc />
    public void Dispose()
    {
        // Nothing to dispose, but all pipelines should implement IDisposable.
    }

    #endregion DISPOSABLE
}
