// <copyright file="Shaders.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Graphics.Definition;
using VoxelGame.Graphics.Graphics;
using VoxelGame.Graphics.Graphics.Raytracing;
using VoxelGame.Graphics.Objects;

namespace VoxelGame.Client.Visuals;

/// <summary>
///     A utility class for loading, compiling and managing graphics pipelines used by the game.
/// </summary>
public sealed class Pipelines : IDisposable
{
    private readonly DirectoryInfo directory;

    private readonly List<VFX> renderers = [];
    private readonly List<IDisposable> bindings = [];

    private ILoadingContext? loadingContext;
    private Boolean loaded;

    private RasterPipeline postProcessingPipeline = null!;
    private ShaderBuffer<RaytracingData>? raytracingDataBuffer;

    private Pipelines(DirectoryInfo directory)
    {
        this.directory = directory;
    }

    /// <summary>
    ///     Get the selection box renderer, which is used to draw selection boxes around blocks.
    /// </summary>
    public SelectionBoxVFX SelectionBoxVFX { get; private set; } = null!;

    /// <summary>
    ///     Get the crosshair renderer, which is used to draw the crosshair.
    /// </summary>
    public ScreenElementVFX CrosshairVFX { get; private set; } = null!;

    /// <summary>
    ///     Get the overlay renderer, which is used to draw overlays, e.g. when stuck in a block.
    /// </summary>
    public OverlayVFX OverlayVFX { get; private set; } = null!;

    /// <summary>
    ///     Get the raytracing data buffer.
    /// </summary>
    public ShaderBuffer<RaytracingData> RaytracingDataBuffer => raytracingDataBuffer!;

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
        Application.Client client,
        (TextureArray, TextureArray) textureSlots,
        VisualConfiguration visuals,
        ILoadingContext loadingContext)
    {
        Pipelines pipelines = new(directory);

        using (loadingContext.BeginStep("Shader Setup"))
        {
            pipelines.loadingContext = loadingContext;
            pipelines.LoadAll(client, textureSlots, visuals);
            pipelines.loadingContext = null!;
        }

        Graphics.Initialize(pipelines.loaded ? pipelines : null);

        return pipelines;
    }

    private void LoadAll(Application.Client client, (TextureArray, TextureArray) textureSlots, VisualConfiguration visuals)
    {
        loaded = true;

        LoadBasicRasterPipelines(client, textureSlots);
        LoadRaytracingPipeline(client, textureSlots, visuals);
        LoadEffectRasterPipelines(client);

        if (!loaded) return;

        client.SetPostProcessingPipeline(postProcessingPipeline);
    }

    private void LoadBasicRasterPipelines(Application.Client client, (TextureArray, TextureArray) textureSlots)
    {
        if (!loaded) return;

        postProcessingPipeline = Require(LoadPipeline(client, "PostProcessing", new ShaderPresets.PostProcessing()));

        CrosshairVFX = Require(ScreenElementVFX.Create(client, this, (0.5f, 0.5f)), renderers);
        bindings.Add(client.Settings.CrosshairColor.Bind(args => CrosshairVFX.SetColor(args.NewValue)));
        bindings.Add(client.Settings.CrosshairScale.Bind(args => CrosshairVFX.SetScale(args.NewValue)));

        OverlayVFX = Require(OverlayVFX.Create(client, this, textureSlots), renderers);
    }

    private void LoadEffectRasterPipelines(Application.Client client)
    {
        if (!loaded) return;

        SelectionBoxVFX = Require(SelectionBoxVFX.Create(client, this), renderers);
        bindings.Add(client.Settings.DarkSelectionColor.Bind(args => SelectionBoxVFX.SetDarkColor(args.NewValue)));
        bindings.Add(client.Settings.BrightSelectionColor.Bind(args => SelectionBoxVFX.SetBrightColor(args.NewValue)));
    }

    private TConcrete Require<TConcrete>(TConcrete? value) where TConcrete : class
    {
        loaded &= value != null;

        return value!;
    }

    private TConcrete Require<TConcrete, TBase>(TConcrete? value, ICollection<TBase> registry) where TConcrete : class, TBase
    {
        loaded &= value != null;

        if (value != null) registry.Add(value);

        return value!;
    }

    /// <summary>
    ///     Load a raster pipeline with a buffer.
    ///     Only valid to call during the loading phase.
    /// </summary>
    /// <param name="client">The client to use.</param>
    /// <param name="name">The name of the pipeline, which is also the name of the shader file.</param>
    /// <param name="preset">The preset to use.</param>
    /// <typeparam name="T">The type of the buffer.</typeparam>
    /// <returns>The pipeline and the buffer, if loading was successful.</returns>
    public (RasterPipeline, ShaderBuffer<T>)? LoadPipelineWithBuffer<T>(VoxelGame.Graphics.Core.Client client, String name, ShaderPresets.IPreset preset) where T : unmanaged, IEquatable<T>
    {
        Debug.Assert(loadingContext != null);

        FileInfo path = directory.GetFile($"{name}.hlsl");

        (RasterPipeline, ShaderBuffer<T>)? result = client.CreateRasterPipeline<T>(
            RasterPipelineDescription.Create(path, preset),
            error =>
            {
                loadingContext.ReportFailure(nameof(RasterPipeline), path, error);
                loaded = false;

                Debugger.Break();
            });

        if (loaded) loadingContext.ReportSuccess(nameof(RasterPipeline), path);

        return loaded ? result : null;
    }

    /// <summary>
    ///     Load a raster pipeline.
    ///     Only valid to call during the loading phase.
    /// </summary>
    /// <param name="client">The client to use.</param>
    /// <param name="name">The name of the pipeline, which is also the name of the shader file.</param>
    /// <param name="preset">The preset to use.</param>
    /// <returns>The pipeline, if loading was successful.</returns>
    private RasterPipeline? LoadPipeline(VoxelGame.Graphics.Core.Client client, String name, ShaderPresets.IPreset preset)
    {
        Debug.Assert(loadingContext != null);

        FileInfo path = directory.GetFile($"{name}.hlsl");

        RasterPipeline? pipeline = client.CreateRasterPipeline(
            RasterPipelineDescription.Create(path, preset),
            error =>
            {
                loadingContext.ReportFailure(nameof(RasterPipeline), path, error);
                loaded = false;

                Debugger.Break();
            });

        if (loaded) loadingContext.ReportSuccess(nameof(RasterPipeline), path);

        return loaded ? pipeline : null;
    }

    private void LoadRaytracingPipeline(VoxelGame.Graphics.Core.Client client, (TextureArray, TextureArray) textureSlots, VisualConfiguration visuals)
    {
        if (!loaded) return;

        PipelineBuilder builder = new();

        builder.AddShaderFile(directory.GetFile("RayGen.hlsl"), names: ["RayGen"]);
        builder.AddShaderFile(directory.GetFile("Miss.hlsl"), names: ["Miss"]);
        builder.AddShaderFile(directory.GetFile("Shadow.hlsl"), names: ["ShadowMiss"]);

        SectionVFX.InitializeRequiredResources(directory, visuals, builder);

        builder.SetFirstTextureSlot(textureSlots.Item1);
        builder.SetSecondTextureSlot(textureSlots.Item2);

        builder.SetCustomDataBufferType<RaytracingData>();

        loaded &= builder.Build(client, loadingContext!, out raytracingDataBuffer);
    }

    /// <summary>
    ///     Data passed to the raytracing shaders.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = ShaderBuffers.Pack)]
    public struct RaytracingData : IEquatable<RaytracingData>
    {
        /// <summary>
        ///     Whether to render in wireframe mode.
        /// </summary>
        [MarshalAs(UnmanagedType.Bool)] public Boolean wireframe;

        /// <summary>
        ///     The wind direction, used for foliage swaying.
        /// </summary>
        public Vector3 windDirection;

        /// <summary>
        ///     The size of the part of the view plane that is inside a fog volume. Given in relative size, positive values start
        ///     from the bottom, negative values from the top.
        /// </summary>
        public Single fogOverlapSize;

        /// <summary>
        ///     Color of the fog volume the view plane is currently in, represented as a RGB vector.
        /// </summary>
        public Vector3 fogOverlapColor;

        private (Boolean, Vector3, Single, Vector3) Pack => (wireframe, windDirection, fogOverlapSize, fogOverlapColor);

        /// <inheritdoc />
        public Boolean Equals(RaytracingData other)
        {
            return Pack.Equals(other.Pack);
        }

        /// <inheritdoc />
        public override Boolean Equals(Object? obj)
        {
            return obj is RaytracingData other && Equals(other);
        }

        /// <inheritdoc />
        public override Int32 GetHashCode()
        {
            return Pack.GetHashCode();
        }

        /// <summary>
        ///     Check if two <see cref="RaytracingData" />s are equal.
        /// </summary>
        public static Boolean operator ==(RaytracingData left, RaytracingData right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Check if two <see cref="RaytracingData" />s are not equal.
        /// </summary>
        public static Boolean operator !=(RaytracingData left, RaytracingData right)
        {
            return !left.Equals(right);
        }
    }

    #region IDisposable Support

    private Boolean disposed;

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;
        if (!disposing) return;

        foreach (VFX renderer in renderers) renderer.Dispose();
        foreach (IDisposable binding in bindings) binding.Dispose();

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
    ~Pipelines()
    {
        Dispose(disposing: false);
    }

    #endregion IDisposable Support
}
