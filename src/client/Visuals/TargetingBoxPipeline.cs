// <copyright file="BoxRenderer.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Runtime.InteropServices;
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;
using VoxelGame.Graphics.Definition;
using VoxelGame.Graphics.Objects;

namespace VoxelGame.Client.Visuals;

/// <summary>
///     A rendering pipeline for block targeting visualization based on the <see cref="BoxCollider" /> struct.
///     Create a <see cref="TargetingBoxEffect" /> to use this pipeline.
/// </summary>
public sealed partial class TargetingBoxPipeline : IDisposable
{
    private readonly VoxelGame.Graphics.Core.Client client;
    private readonly RasterPipeline pipeline;
    private readonly ShaderBuffer<Data> buffer;

    private Boolean dataDirty = true;

    private ColorS brightColor = ColorS.White;
    private ColorS darkColor = ColorS.Black;

    private TargetingBoxPipeline(VoxelGame.Graphics.Core.Client client, RasterPipeline pipeline, ShaderBuffer<Data> buffer)
    {
        this.client = client;
        this.pipeline = pipeline;
        this.buffer = buffer;
    }

    #region DISPOSABLE

    /// <inheritdoc />
    public void Dispose()
    {
        // Nothing to dispose, but all pipelines should implement IDisposable.
    }

    #endregion DISPOSABLE

    /// <summary>
    ///     Create a new <see cref="TargetingBoxPipeline" />.
    /// </summary>
    internal static TargetingBoxPipeline? Create(VoxelGame.Graphics.Core.Client client, PipelineFactory factory)
    {
        (RasterPipeline pipeline, ShaderBuffer<Data> buffer)? result
            = factory.LoadPipelineWithBuffer<Data>("Targeting", new ShaderPresets.SpatialEffect());

        return result is {pipeline: var rasterPipeline, buffer: var shaderBuffer}
            ? new TargetingBoxPipeline(client, rasterPipeline, shaderBuffer)
            : null;
    }

    /// <summary>
    ///     Create a new <see cref="TargetingBoxEffect" /> using this pipeline.
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
        dataDirty = true;
    }

    /// <summary>
    ///     Set the color to use on dark background.
    /// </summary>
    /// <param name="newColor">The new color.</param>
    public void SetBrightColor(ColorS newColor)
    {
        brightColor = newColor;
        dataDirty = true;
    }

    /// <summary>
    ///     Update the data used by the pipeline.
    /// </summary>
    public void UpdateData()
    {
        if (!dataDirty) return;

        buffer.Data = new Data(darkColor.ToColor4(), brightColor.ToColor4());

        dataDirty = false;
    }

    /// <summary>
    ///     Data used by the shader.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = ShaderBuffers.Pack)]
    [ValueSemantics]
    private readonly partial struct Data
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
    }
}
