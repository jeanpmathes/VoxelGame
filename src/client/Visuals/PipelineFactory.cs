// <copyright file="PipelineFactory.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Graphics.Definition;
using VoxelGame.Graphics.Objects;

namespace VoxelGame.Client.Visuals;

/// <summary>
///     Creates raster pipelines.
/// </summary>
internal class PipelineFactory
{
    private readonly Application.Client client;
    private readonly ICollection<MissingResource> errors;

    /// <summary>
    ///     Create a new instance of the pipeline factory.
    /// </summary>
    /// <param name="client">The client which will use the pipelines.</param>
    /// <param name="errors">A collection to which errors are added.</param>
    internal PipelineFactory(Application.Client client, ICollection<MissingResource> errors)
    {
        this.client = client;
        this.errors = errors;
    }

    /// <summary>
    ///     Load a raster pipeline with a buffer.
    ///     Only valid to call during the loading phase.
    /// </summary>
    /// <param name="name">The name of the pipeline, which is also the name of the shader file.</param>
    /// <param name="preset">The preset to use.</param>
    /// <typeparam name="T">The type of the buffer.</typeparam>
    /// <returns>The pipeline and the buffer, if loading was successful.</returns>
    internal (RasterPipeline, ShaderBuffer<T>)? LoadPipelineWithBuffer<T>(String name, ShaderPresets.IPreset preset) where T : unmanaged, IEquatable<T>
    {
        FileInfo path = Engine.ShaderDirectory.GetFile($"{name}.hlsl");
        var ok = true;

        (RasterPipeline, ShaderBuffer<T>)? result = client.CreateRasterPipeline<T>(
            RasterPipelineDescription.Create(path, preset),
            error =>
            {
                ok = false;

                errors.Add(new MissingResource(ResourceTypes.Shader, RID.Path(path), ResourceIssue.FromMessage(Level.Error, error)));

                Debugger.Break();
            });

        return ok ? result : null;
    }

    /// <summary>
    ///     Load a raster pipeline.
    ///     Only valid to call during the loading phase.
    /// </summary>
    /// <param name="name">The name of the pipeline, which is also the name of the shader file.</param>
    /// <param name="preset">The preset to use.</param>
    /// <returns>The pipeline, if loading was successful.</returns>
    internal RasterPipeline? LoadPipeline(String name, ShaderPresets.IPreset preset)
    {
        FileInfo path = Engine.ShaderDirectory.GetFile($"{name}.hlsl");
        var ok = true;

        RasterPipeline? pipeline = client.CreateRasterPipeline(
            RasterPipelineDescription.Create(path, preset),
            error =>
            {
                ok = false;

                errors.Add(new MissingResource(ResourceTypes.Shader, RID.Path(path), ResourceIssue.FromMessage(Level.Error, error)));

                Debugger.Break();
            });

        return ok ? pipeline : null;
    }
}
