// <copyright file="EngineLoader.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Client.Visuals.Textures;
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
    private readonly List<MissingResource> errors = [];

    String? ICatalogEntry.Instance => null;

    /// <inheritdoc />
    public IEnumerable<IResource> Load(IResourceContext context)
    {
        return context.Require<Application.Client>(client =>
            context.Require<TextureBundle>(Resources.Textures.BlockID,
                blocks =>
                    context.Require<TextureBundle>(Resources.Textures.FluidID,
                        fluids =>
                            context.Require<VisualConfiguration>(visuals =>
                                Load(context, blocks, fluids, client, visuals)))));
    }

    private IEnumerable<IResource> Load(IResourceContext context, TextureBundle blocks, TextureBundle fluids, Application.Client client, VisualConfiguration visuals)
    {
        errors.Clear();

        (TextureArray, TextureArray) textureSlots = TextureBundle.GetTextureSlots(blocks, fluids);

        PipelineFactory factory = new(client, errors);

        RasterPipeline? postProcessingPipeline = factory.LoadPipeline("PostProcessing", new ShaderPresets.PostProcessing());
        var crosshairVFX = ScreenElementPipeline.Create(client, factory, (0.5f, 0.5f));
        var overlayVFX = OverlayPipeline.Create(client, factory, textureSlots);

        if (postProcessingPipeline == null || crosshairVFX == null || overlayVFX == null)
            return errors;

        ShaderBuffer<Engine.RaytracingData>? rtData = LoadRaytracingPipeline(client, visuals, textureSlots, context);

        if (rtData == null)
            return errors;

        var selectionBoxVFX = TargetingBoxPipeline.Create(client, factory);

        if (selectionBoxVFX == null)
            return errors;

        client.SetPostProcessingPipeline(postProcessingPipeline);

        return [new Engine(client, crosshairVFX, overlayVFX, selectionBoxVFX, rtData)];
    }

    private ShaderBuffer<Engine.RaytracingData>? LoadRaytracingPipeline(
        VoxelGame.Graphics.Core.Client client, VisualConfiguration visuals, (TextureArray, TextureArray) textureSlots, IResourceContext context)
    {
        PipelineBuilder builder = new();

        builder.AddShaderFile(Engine.ShaderDirectory.GetFile("RayGen.hlsl"), names: ["RayGen"]);
        builder.AddShaderFile(Engine.ShaderDirectory.GetFile("Miss.hlsl"), names: ["Miss"]);
        builder.AddShaderFile(Engine.ShaderDirectory.GetFile("Shadow.hlsl"), names: ["ShadowMiss"]);

        SectionRenderer.InitializeRequiredResources(Engine.ShaderDirectory, visuals, builder);

        builder.SetFirstTextureSlot(textureSlots.Item1);
        builder.SetSecondTextureSlot(textureSlots.Item2);

        builder.SetCustomDataBufferType<Engine.RaytracingData>();

        builder.SetSpoolCounts(mesh: 8192, effect: 4);

        ResourceIssue? error = builder.Build(client, context, out ShaderBuffer<Engine.RaytracingData>? buffer);

        if (error != null)
            errors.Add(new MissingResource(ResourceTypes.Engine, RID.Named<Engine>("Default"), error));

        return buffer;
    }
}
