// <copyright file="Shaders.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
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
using VoxelGame.Logging;
using VoxelGame.Support.Definition;
using VoxelGame.Support.Graphics;
using VoxelGame.Support.Graphics.Raytracing;
using VoxelGame.Support.Objects;

namespace VoxelGame.Client.Rendering;

/// <summary>
///     A utility class for loading, compiling and managing graphics pipelines used by the game.
/// </summary>
public sealed class Pipelines : IDisposable
{
    private readonly DirectoryInfo directory;

    private readonly List<Renderer> renderers = new();
    private readonly List<IDisposable> bindings = new();

    private LoadingContext? loadingContext;
    private bool loaded;

    private RasterPipeline postProcessingPipeline = null!;
    private ShaderBuffer<RaytracingData>? raytracingDataBuffer;

    private Pipelines(DirectoryInfo directory)
    {
        this.directory = directory;
    }

    /// <summary>
    ///     Get the selection box renderer, which is used to draw selection boxes around blocks.
    /// </summary>
    public SelectionBoxRenderer SelectionBoxRenderer { get; private set; } = null!;

    /// <summary>
    ///     Get the crosshair renderer, which is used to draw the crosshair.
    /// </summary>
    public ScreenElementRenderer CrosshairRenderer { get; private set; } = null!;

    /// <summary>
    ///     Get the overlay renderer, which is used to draw overlays, e.g. when stuck in a block.
    /// </summary>
    public OverlayRenderer OverlayRenderer { get; private set; } = null!;

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
        LoadingContext loadingContext)
    {
        Pipelines pipelines = new(directory);

        using (loadingContext.BeginStep(Events.RenderPipelineSetup, "Shader Setup"))
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

        CrosshairRenderer = Require(ScreenElementRenderer.Create(client, this, (0.5f, 0.5f)), renderers);
        bindings.Add(client.Settings.CrosshairColor.Bind(args => CrosshairRenderer.SetColor(args.NewValue)));
        bindings.Add(client.Settings.CrosshairScale.Bind(args => CrosshairRenderer.SetScale(args.NewValue)));

        OverlayRenderer = Require(OverlayRenderer.Create(client, this, textureSlots), renderers);
    }

    private void LoadEffectRasterPipelines(Application.Client client)
    {
        if (!loaded) return;

        SelectionBoxRenderer = Require(SelectionBoxRenderer.Create(client, this), renderers);
        bindings.Add(client.Settings.DarkSelectionColor.Bind(args => SelectionBoxRenderer.SetDarkColor(args.NewValue)));
        bindings.Add(client.Settings.BrightSelectionColor.Bind(args => SelectionBoxRenderer.SetBrightColor(args.NewValue)));
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
    public (RasterPipeline, ShaderBuffer<T>)? LoadPipelineWithBuffer<T>(Support.Core.Client client, string name, ShaderPresets.IPreset preset) where T : unmanaged, IEquatable<T>
    {
        Debug.Assert(loadingContext != null);

        FileInfo path = directory.GetFile($"{name}.hlsl");

        (RasterPipeline, ShaderBuffer<T>)? result = client.CreateRasterPipeline<T>(
            RasterPipelineDescription.Create(path, preset),
            error =>
            {
                loadingContext.ReportFailure(Events.RenderPipelineError, nameof(RasterPipeline), path, error);
                loaded = false;

                Debugger.Break();
            });

        if (loaded) loadingContext.ReportSuccess(Events.RenderPipelineSetup, nameof(RasterPipeline), path);

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
    private RasterPipeline? LoadPipeline(Support.Core.Client client, string name, ShaderPresets.IPreset preset)
    {
        Debug.Assert(loadingContext != null);

        FileInfo path = directory.GetFile($"{name}.hlsl");

        RasterPipeline? pipeline = client.CreateRasterPipeline(
            RasterPipelineDescription.Create(path, preset),
            error =>
            {
                loadingContext.ReportFailure(Events.RenderPipelineError, nameof(RasterPipeline), path, error);
                loaded = false;

                Debugger.Break();
            });

        if (loaded) loadingContext.ReportSuccess(Events.RenderPipelineSetup, nameof(RasterPipeline), path);

        return loaded ? pipeline : null;
    }

    private void LoadRaytracingPipeline(Support.Core.Client client, (TextureArray, TextureArray) textureSlots, VisualConfiguration visuals)
    {
        if (!loaded) return;

        PipelineBuilder builder = new();

        builder.AddShaderFile(directory.GetFile("RayGen.hlsl"), names: new[] {"RayGen"});
        builder.AddShaderFile(directory.GetFile("Miss.hlsl"), names: new[] {"Miss"});
        builder.AddShaderFile(directory.GetFile("Shadow.hlsl"), names: new[] {"ShadowMiss"});

        SectionRenderer.InitializeRequiredResources(directory, visuals, builder);

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

    #region IDisposable Support

    private bool disposed;

    private void Dispose(bool disposing)
    {
        if (disposed) return;
        if (!disposing) return;

        foreach (Renderer renderer in renderers) renderer.Dispose();
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
