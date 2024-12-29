// <copyright file="EngineLoader.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using VoxelGame.Client.Resources;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;
using VoxelGame.Graphics.Definition;
using VoxelGame.Graphics.Graphics;
using VoxelGame.Graphics.Graphics.Raytracing;
using VoxelGame.Graphics.Objects;

namespace VoxelGame.Client.Visuals;

/// <summary>
///     Loads the engine and the required resources defining the engine.
/// </summary>
public sealed class EngineLoader : IResourceLoader
{
    private readonly DirectoryInfo directory = FileSystem.GetResourceDirectory("Shaders");

    private readonly List<MissingResource> errors = [];

    String? ICatalogEntry.Instance => null;

    /// <inheritdoc />
    public IEnumerable<IResource> Load(IResourceContext context)
    {
        return context.Require<Application.Client>(client =>
            context.Require<TextureBundle>(Textures.BlockID,
                blocks =>
                    context.Require<TextureBundle>(Textures.FluidID,
                        fluids =>
                            context.Require<VisualConfiguration>(visuals =>
                            {
                                errors.Clear();

                                (TextureArray, TextureArray) textureSlots = TextureBundle.GetTextureSlots(blocks, fluids);

                                PipelineFactory factory = new(client, this);

                                RasterPipeline? postProcessingPipeline = factory.LoadPipeline("PostProcessing", new ShaderPresets.PostProcessing());
                                var crosshairVFX = ScreenElementVFX.Create(client, factory, (0.5f, 0.5f));
                                var overlayVFX = OverlayVFX.Create(client, factory, textureSlots);

                                if (postProcessingPipeline == null || crosshairVFX == null || overlayVFX == null)
                                    return errors;

                                ShaderBuffer<Engine.RaytracingData>? rtData = LoadRaytracingPipeline(client, visuals, textureSlots, context);

                                if (rtData == null)
                                    return errors;

                                var selectionBoxVFX = SelectionBoxVFX.Create(client, factory);

                                if (selectionBoxVFX == null)
                                    return errors;

                                client.SetPostProcessingPipeline(postProcessingPipeline);

                                return [new Engine(client, crosshairVFX, overlayVFX, selectionBoxVFX, rtData)];
                            }))));
    }

    private ShaderBuffer<Engine.RaytracingData>? LoadRaytracingPipeline(Application.Client client, VisualConfiguration visuals, (TextureArray, TextureArray) textureSlots, IResourceContext context)
    {
        PipelineBuilder builder = new();

        builder.AddShaderFile(directory.GetFile("RayGen.hlsl"), names: ["RayGen"]);
        builder.AddShaderFile(directory.GetFile("Miss.hlsl"), names: ["Miss"]);
        builder.AddShaderFile(directory.GetFile("Shadow.hlsl"), names: ["ShadowMiss"]);

        SectionVFX.InitializeRequiredResources(directory, visuals, builder);

        builder.SetFirstTextureSlot(textureSlots.Item1);
        builder.SetSecondTextureSlot(textureSlots.Item2);

        builder.SetCustomDataBufferType<Engine.RaytracingData>();

        builder.SetSpoolCounts(mesh: 8192, effect: 4);

        ResourceIssue? error = builder.Build(client, context, out ShaderBuffer<Engine.RaytracingData>? buffer);

        if (error != null)
            errors.Add(new MissingResource(ResourceTypes.Engine, RID.Named<Engine>("Default"), error));

        return buffer;
    }

    /// <summary>
    ///     Creates raster pipelines.
    /// </summary>
    public class PipelineFactory
    {
        private readonly VoxelGame.Graphics.Core.Client client;
        private readonly EngineLoader loader;

        /// <summary>
        ///     Create a new instance of the pipeline factory.
        /// </summary>
        /// <param name="client">The client which will use the pipelines.</param>
        /// <param name="loader">The loader that created this factory.</param>
        public PipelineFactory(VoxelGame.Graphics.Core.Client client, EngineLoader loader)
        {
            this.client = client;
            this.loader = loader;
        }

        /// <summary>
        ///     Load a raster pipeline with a buffer.
        ///     Only valid to call during the loading phase.
        /// </summary>
        /// <param name="name">The name of the pipeline, which is also the name of the shader file.</param>
        /// <param name="preset">The preset to use.</param>
        /// <typeparam name="T">The type of the buffer.</typeparam>
        /// <returns>The pipeline and the buffer, if loading was successful.</returns>
        public (RasterPipeline, ShaderBuffer<T>)? LoadPipelineWithBuffer<T>(String name, ShaderPresets.IPreset preset) where T : unmanaged, IEquatable<T>
        {
            FileInfo path = loader.directory.GetFile($"{name}.hlsl");
            var ok = true;

            (RasterPipeline, ShaderBuffer<T>)? result = client.CreateRasterPipeline<T>(
                RasterPipelineDescription.Create(path, preset),
                error =>
                {
                    ok = false;

                    loader.errors.Add(new MissingResource(ResourceTypes.Shader, RID.Path(path), ResourceIssue.FromMessage(Level.Error, error)));

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
        public RasterPipeline? LoadPipeline(String name, ShaderPresets.IPreset preset)
        {
            FileInfo path = loader.directory.GetFile($"{name}.hlsl");
            var ok = true;

            RasterPipeline? pipeline = client.CreateRasterPipeline(
                RasterPipelineDescription.Create(path, preset),
                error =>
                {
                    ok = false;

                    loader.errors.Add(new MissingResource(ResourceTypes.Shader, RID.Path(path), ResourceIssue.FromMessage(Level.Error, error)));

                    Debugger.Break();
                });

            return ok ? pipeline : null;
        }
    }
}
