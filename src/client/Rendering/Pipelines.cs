﻿// <copyright file="Shaders.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;
using VoxelGame.Support.Definition;
using VoxelGame.Support.Graphics;
using VoxelGame.Support.Graphics.Objects;
using VoxelGame.Support.Graphics.Raytracing;
using VoxelGame.Support.Graphics.Utility;
using VoxelGame.Support.Objects;

namespace VoxelGame.Client.Rendering;

/// <summary>
///     A utility class for loading, compiling and managing graphics pipelines used by the game.
/// </summary>
public sealed class Pipelines // todo: delete all GLSL shaders
{
    private const string SectionFragmentShader = "section";

    private const string TimeUniform = "time"; // todo: delete these constants, all the uniform stuff and the setter methods
    private const string NearPlaneUniform = "nearPlane";
    private const string FarPlaneUniform = "farPlane";

    private readonly DirectoryInfo directory;

    private readonly ISet<Shader> farPlaneSet = new HashSet<Shader>();

    private readonly ShaderLoader loader;
    private readonly LoadingContext loadingContext;
    private readonly ISet<Shader> nearPlaneSet = new HashSet<Shader>();

    private readonly ISet<Shader> timedSet = new HashSet<Shader>();

    private bool loaded;

    private RasterPipeline postProcessingPipeline = null!;

    private ShaderBuffer<RaytracingData>? raytracingDataBuffer;

    private Pipelines(DirectoryInfo directory, LoadingContext loadingContext)
    {
        this.directory = directory;
        this.loadingContext = loadingContext;

        loader = new ShaderLoader(
            directory,
            loadingContext,
            (timedSet, TimeUniform),
            (nearPlaneSet, NearPlaneUniform),
            (farPlaneSet, FarPlaneUniform));
    }

    /// <summary>
    ///     Get the selection effect.
    /// </summary>
    public RasterPipeline SelectionEffect { get; private set; } = null!;

    /// <summary>
    ///     Get the raytracing data buffer.
    /// </summary>
    public ShaderBuffer<RaytracingData> RaytracingDataBuffer => raytracingDataBuffer!;

    /// <summary>
    ///     The shader used for simple blocks.
    /// </summary>
    public Shader SimpleSection { get; private set; } = null!;

    /// <summary>
    ///     The shader used for complex blocks.
    /// </summary>
    public Shader ComplexSection { get; private set; } = null!;

    /// <summary>
    ///     The shader used for varying height blocks.
    /// </summary>
    public Shader VaryingHeightSection { get; private set; } = null!;

    /// <summary>
    ///     The shader used for cross plant blocks.
    /// </summary>
    public Shader CrossPlantSection { get; private set; } = null!;

    /// <summary>
    ///     The shader used for crop plant blocks.
    /// </summary>
    public Shader CropPlantSection { get; private set; } = null!;

    /// <summary>
    ///     The shader used for opaque fluids.
    /// </summary>
    public Shader OpaqueFluidSection { get; private set; } = null!;

    /// <summary>
    ///     The shader used for the accumulate pass for transparent fluids.
    /// </summary>
    public Shader TransparentFluidSectionAccumulate { get; private set; } = null!;

    /// <summary>
    ///     The shader used for the draw pass for transparent fluids.
    /// </summary>
    public Shader TransparentFluidSectionDraw { get; private set; } = null!;

    /// <summary>
    ///     The shader used for block/fluid texture overlays.
    /// </summary>
    public Shader Overlay { get; private set; } = null!;

    /// <summary>
    ///     The shader used for the selection box.
    /// </summary>
    public Shader Selection { get; private set; } = null!;

    /// <summary>
    ///     The shader used for simply screen elements.
    /// </summary>
    public Shader ScreenElement { get; private set; } = null!;

    /// <summary>
    ///     The basic raytracing material for opaque section parts.
    /// </summary>
    public Material BasicOpaqueSectionMaterial { get; private set; } = null!;

    /// <summary>
    ///     The basic raytracing material for transparent section parts.
    /// </summary>
    public Material BasicTransparentSectionMaterial { get; private set; } = null!;

    /// <summary>
    ///     The raytracing material used for foliage.
    /// </summary>
    public Material FoliageSectionMaterial { get; private set; } = null!;

    /// <summary>
    ///     The raytracing material used for opaque fluids.
    /// </summary>
    public Material FluidSectionMaterial { get; private set; } = null!;

    /// <summary>
    ///     Load all pipelines required for the game from a given directory.
    /// </summary>
    /// <param name="directory">The directory containing all necessary shader code.</param>
    /// <param name="client">The client to use.</param>
    /// <param name="textureSlots">The textures for the two texture slots.</param>
    /// <param name="visuals">Information on how the visuals are configured, meaning graphics settings.</param>
    /// <param name="loadingContext">The loader to use.</param>
    /// <returns>An object representing all loaded pipelines.</returns>
    internal static Pipelines Load(
        DirectoryInfo directory,
        Support.Core.Client client,
        (TextureArray, TextureArray) textureSlots,
        VisualConfiguration visuals,
        LoadingContext loadingContext)
    {
        Pipelines pipelines = new(directory, loadingContext);

        using (loadingContext.BeginStep(Events.RenderPipelineSetup, "Shader Setup"))
        {
            pipelines.LoadAll(client, textureSlots, visuals);
        }

        Graphics.Initialize(pipelines.loaded ? pipelines : null);

        return pipelines;
    }

    internal void Delete()
    {
        // todo: think about deleting (some cleanup like removing from draw2d and similar is necessary)
        // todo: maybe some more cleanup would be nice

        return;

        SimpleSection.Delete();
        ComplexSection.Delete();
        VaryingHeightSection.Delete();
        CrossPlantSection.Delete();
        CropPlantSection.Delete();
        OpaqueFluidSection.Delete();
        TransparentFluidSectionAccumulate.Delete();
        TransparentFluidSectionDraw.Delete();

        Overlay.Delete();
        Selection.Delete();
        ScreenElement.Delete();

        loaded = false;
    }

    private void LoadAll(Support.Core.Client client, (TextureArray, TextureArray) textureSlots, VisualConfiguration visuals)
    {
        loaded = true;

        LoadBasicRasterPipelines(client);
        LoadRaytracingPipeline(client, textureSlots, visuals);
        LoadEffectRasterPipelines(client);

        if (!loaded) return;

        client.SetPostProcessingPipeline(postProcessingPipeline);

        return; // todo: remove this, and maybe the code below

        Shader Check(Shader? shader)
        {
            loaded &= shader != null;

            return shader!;
        }

        SimpleSection = Check(loader.Load(nameof(SimpleSection), "simple_section", SectionFragmentShader));
        ComplexSection = Check(loader.Load(nameof(ComplexSection), "complex_section", SectionFragmentShader));
        VaryingHeightSection = Check(loader.Load(nameof(VaryingHeightSection), "varying_height_section", SectionFragmentShader));
        CrossPlantSection = Check(loader.Load(nameof(CrossPlantSection), "cross_plant_section", SectionFragmentShader));
        CropPlantSection = Check(loader.Load(nameof(CropPlantSection), "crop_plant_section", SectionFragmentShader));
        OpaqueFluidSection = Check(loader.Load(nameof(OpaqueFluidSection), "fluid_section", "opaque_fluid_section"));
        TransparentFluidSectionAccumulate = Check(loader.Load(nameof(TransparentFluidSectionAccumulate), "fluid_section", "transparent_fluid_section_accumulate"));
        TransparentFluidSectionDraw = Check(loader.Load(nameof(TransparentFluidSectionDraw), "fullscreen", "transparent_fluid_section_draw"));

        Overlay = Check(loader.Load(nameof(Overlay), "overlay", "overlay"));
        Selection = Check(loader.Load(nameof(Selection), "selection", "selection"));
        ScreenElement = Check(loader.Load(nameof(ScreenElement), "screen_element", "screen_element"));

        UpdateOrthographicProjection();
    }

    private void LoadBasicRasterPipelines(Support.Core.Client client)
    {
        postProcessingPipeline = LoadPipeline(client, "Post", ShaderPreset.PostProcessing);
    }

    private void LoadEffectRasterPipelines(Support.Core.Client client)
    {
        (RasterPipeline pipeline, ShaderBuffer<BoxRenderer.Data> buffer) = LoadPipelineWithBuffer<BoxRenderer.Data>(client, "Selection", ShaderPreset.SpatialEffect, Topology.Line);
        SelectionEffect = pipeline;
        if (loaded) buffer.Modify((ref BoxRenderer.Data data) => data.Color = (0.1f, 0.1f, 0.1f));
    }

    private (RasterPipeline, ShaderBuffer<T>) LoadPipelineWithBuffer<T>(Support.Core.Client client, string name, ShaderPreset preset, Topology topology = Topology.Triangle) where T : unmanaged, IEquatable<T>
    {
        FileInfo path = directory.GetFile($"{name}.hlsl");

        (RasterPipeline, ShaderBuffer<T>) result = client.CreateRasterPipeline<T>(
            RasterPipelineDescription.Create(path, preset, topology),
            error =>
            {
                loadingContext.ReportFailure(Events.RenderPipelineError, nameof(RasterPipeline), path, error);
                loaded = false;
            });

        if (loaded) loadingContext.ReportSuccess(Events.RenderPipelineSetup, nameof(RasterPipeline), path);

        return result;
    }

    private RasterPipeline LoadPipeline(Support.Core.Client client, string name, ShaderPreset preset, Topology topology = Topology.Triangle)
    {
        FileInfo path = directory.GetFile($"{name}.hlsl");

        RasterPipeline pipeline = client.CreateRasterPipeline(
            RasterPipelineDescription.Create(path, preset, topology),
            error =>
            {
                loadingContext.ReportFailure(Events.RenderPipelineError, nameof(RasterPipeline), path, error);
                loaded = false;
            });

        if (loaded) loadingContext.ReportSuccess(Events.RenderPipelineSetup, nameof(RasterPipeline), path);

        return pipeline;
    }

    private void LoadRaytracingPipeline(Support.Core.Client client, (TextureArray, TextureArray) textureSlots, VisualConfiguration visuals)
    {
        PipelineBuilder builder = new();

        PipelineBuilder.HitGroup basicOpaqueSectionHitGroup = new("BasicOpaqueSectionClosestHit");
        PipelineBuilder.HitGroup basicOpaqueShadowHitGroup = new("BasicOpaqueShadowClosestHit");

        PipelineBuilder.HitGroup basicTransparentSectionHitGroup = new("BasicTransparentSectionClosestHit", "BasicTransparentSectionAnyHit");
        PipelineBuilder.HitGroup basicTransparentShadowHitGroup = new("BasicTransparentShadowClosestHit", "BasicTransparentShadowAnyHit");

        PipelineBuilder.HitGroup foliageSectionHitGroup = new("FoliageSectionClosestHit", "FoliageSectionAnyHit");
        PipelineBuilder.HitGroup foliageShadowHitGroup = new("FoliageShadowClosestHit", "FoliageShadowAnyHit");

        PipelineBuilder.HitGroup fluidSectionHitGroup = new("FluidSectionClosestHit");
        PipelineBuilder.HitGroup fluidShadowHitGroup = new("FluidShadowClosestHit");

        builder.AddShaderFile(directory.GetFile("RayGen.hlsl"), names: new[] {"RayGen"});
        builder.AddShaderFile(directory.GetFile("Miss.hlsl"), names: new[] {"Miss"});
        builder.AddShaderFile(directory.GetFile("BasicOpaque.hlsl"), new[] {basicOpaqueSectionHitGroup, basicOpaqueShadowHitGroup});
        builder.AddShaderFile(directory.GetFile("BasicTransparent.hlsl"), new[] {basicTransparentSectionHitGroup, basicTransparentShadowHitGroup});
        builder.AddShaderFile(directory.GetFile("Foliage.hlsl"), new[] {foliageSectionHitGroup, foliageShadowHitGroup});
        builder.AddShaderFile(directory.GetFile("Fluid.hlsl"), new[] {fluidSectionHitGroup, fluidShadowHitGroup});
        builder.AddShaderFile(directory.GetFile("Shadow.hlsl"), names: new[] {"ShadowMiss"});

        BasicOpaqueSectionMaterial = builder.AddMaterial(
            nameof(BasicOpaqueSectionMaterial),
            PipelineBuilder.Groups.Default,
            isOpaque: true,
            basicOpaqueSectionHitGroup,
            basicOpaqueShadowHitGroup);

        BasicTransparentSectionMaterial = builder.AddMaterial(
            nameof(BasicTransparentSectionMaterial),
            PipelineBuilder.Groups.Default,
            isOpaque: false,
            basicTransparentSectionHitGroup,
            basicTransparentShadowHitGroup);

        FoliageSectionMaterial = builder.AddMaterial(
            nameof(FoliageSectionMaterial),
            PipelineBuilder.Groups.Default,
            isOpaque: false,
            foliageSectionHitGroup,
            foliageShadowHitGroup,
            visuals.FoliageQuality > Quality.Low ? builder.AddAnimation(directory.GetFile("FoliageAnimation.hlsl")) : null);

        FluidSectionMaterial = builder.AddMaterial(
            nameof(FluidSectionMaterial),
            PipelineBuilder.Groups.NoShadow,
            isOpaque: true, // Despite having transparency, no any-hit shader is used, so it is considered opaque.
            fluidSectionHitGroup,
            fluidShadowHitGroup);

        builder.SetFirstTextureSlot(textureSlots.Item1);
        builder.SetSecondTextureSlot(textureSlots.Item2);

        builder.SetCustomDataBufferType<RaytracingData>();

        loaded &= builder.Build(client, loadingContext, out raytracingDataBuffer);
    }

    /// <summary>
    ///     Update all orthographic projection matrices.
    /// </summary>
    public void UpdateOrthographicProjection()
    {
        if (!loaded) return;

        return; // todo: remove the return, and maybe the code below (as projection matrix could be known in c++ code or are just not necessary for these steps anymore)

        Overlay.SetMatrix4(
            "projection",
            Matrix4d.CreateOrthographic(width: 1.0, 1.0 / Application.Client.Instance.AspectRatio, depthNear: 0.0, depthFar: 1.0).ToMatrix4());

        ScreenElement.SetMatrix4(
            "projection",
            Matrix4d.CreateOrthographic(Screen.Size.X, Screen.Size.Y, depthNear: 0.0, depthFar: 1.0).ToMatrix4());
    }

    /// <summary>
    ///     Update the current time.
    /// </summary>
    /// <param name="time">The current time, since the game has started.</param>
    public void SetTime(float time)
    {
        if (!loaded) return;

        foreach (Shader shader in timedSet) shader.SetFloat(TimeUniform, time);
    }

    /// <summary>
    ///     Set the view plane distances.
    /// </summary>
    /// <param name="near">The near plane distance.</param>
    /// <param name="far">The far plane distance.</param>
    public void SetPlanes(double near, double far)
    {
        if (!loaded) return;

        foreach (Shader shader in nearPlaneSet) shader.SetFloat(NearPlaneUniform, (float) near);

        foreach (Shader shader in farPlaneSet) shader.SetFloat(FarPlaneUniform, (float) far);
    }

    /// <summary>
    ///     Data passed to the raytracing shaders.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct RaytracingData : IEquatable<RaytracingData>
    {
        /// <summary>
        ///     Whether to render in wireframe mode.
        /// </summary>
        [MarshalAs(UnmanagedType.Bool)] public bool wireframe;

        /// <summary>
        ///     The wind direction, used for foliage swaying.
        /// </summary>
        public Vector3 windDirection;

        private (bool, Vector3) Pack => (wireframe, windDirection);

        /// <inheritdoc />
        public bool Equals(RaytracingData other)
        {
            return Pack.Equals(other.Pack);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is RaytracingData other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Pack.GetHashCode();
        }

        /// <summary>
        ///     Check if two <see cref="RaytracingData" />s are equal.
        /// </summary>
        public static bool operator ==(RaytracingData left, RaytracingData right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Check if two <see cref="RaytracingData" />s are not equal.
        /// </summary>
        public static bool operator !=(RaytracingData left, RaytracingData right)
        {
            return !left.Equals(right);
        }
    }
}
