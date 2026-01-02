// <copyright file="PipelineFactory.cs" company="VoxelGame">
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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Graphics.Definition;
using VoxelGame.Graphics.Objects;
using VoxelGame.Toolkit.Utilities.Constants;

namespace VoxelGame.Client.Visuals;

/// <summary>
///     Creates raster pipelines.
/// </summary>
internal sealed class PipelineFactory
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
    internal (RasterPipeline, ShaderBuffer<T>)? LoadPipelineWithBuffer<T>(String name, ShaderPresets.IPreset preset) where T : unmanaged, IEquatable<T>, IDefault<T>
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
