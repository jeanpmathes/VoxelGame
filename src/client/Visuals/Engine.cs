// <copyright file="Shaders.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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
using System.IO;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Graphics.Objects;
using VoxelGame.Toolkit.Interop;
using VoxelGame.Toolkit.Utilities.Constants;

namespace VoxelGame.Client.Visuals;

/// <summary>
///     The graphics engine, consisting of all renderers and pipelines.
/// </summary>
public sealed class Engine : IResource
{
    /// <summary>
    ///     The shader directory.
    /// </summary>
    public static readonly DirectoryInfo ShaderDirectory = FileSystem.GetResourceDirectory("Shaders");

    private readonly Application.Client client;
    private readonly List<IDisposable> bindings = [];

    private readonly ShaderBuffer<RaytracingData>? raytracingDataBuffer;
    private readonly ShaderBuffer<PostProcessingData>? postProcessingBuffer;

    internal Engine(
        Application.Client client,
        ScreenElementPipeline crosshairPipeline,
        OverlayPipeline overlayPipeline,
        TargetingBoxPipeline targetingBoxPipeline,
        ShaderBuffer<RaytracingData>? rtData,
        ShaderBuffer<PostProcessingData>? ppBuffer)
    {
        this.client = client;

        CrosshairPipeline = crosshairPipeline;
        OverlayPipeline = overlayPipeline;
        TargetingBoxPipeline = targetingBoxPipeline;

        raytracingDataBuffer = rtData;
        postProcessingBuffer = ppBuffer;
    }

    /// <summary>
    ///     Get the targeting box pipeline, which is used to draw selection boxes around blocks.
    /// </summary>
    public TargetingBoxPipeline TargetingBoxPipeline { get; }

    /// <summary>
    ///     Get the crosshair pipeline, which is used to draw the crosshair.
    /// </summary>
    public ScreenElementPipeline CrosshairPipeline { get; }

    /// <summary>
    ///     Get the overlay pipeline, which is used to draw overlays, e.g. when stuck in a block.
    /// </summary>
    public OverlayPipeline OverlayPipeline { get; }

    /// <summary>
    ///     Get the raytracing data buffer.
    /// </summary>
    public ShaderBuffer<RaytracingData> RaytracingDataBuffer => raytracingDataBuffer!;

    /// <summary>
    ///     Get the post-processing data buffer.
    /// </summary>
    public ShaderBuffer<PostProcessingData> PostProcessingBuffer => postProcessingBuffer!;

    /// <inheritdoc />
    public RID Identifier { get; } = RID.Named<Engine>("Default");

    /// <inheritdoc />
    public ResourceType Type => ResourceTypes.Engine;

    /// <summary>
    ///     Initialize the engine, must be called by <see cref="Graphics" />.
    /// </summary>
    internal void Initialize()
    {
        bindings.Add(client.Settings.CrosshairColor.Bind(args => CrosshairPipeline.SetColor(args.NewValue)));
        bindings.Add(client.Settings.CrosshairScale.Bind(args => CrosshairPipeline.SetScale(args.NewValue)));

        bindings.Add(client.Settings.DarkSelectionColor.Bind(args => TargetingBoxPipeline.SetDarkColor(args.NewValue)));
        bindings.Add(client.Settings.BrightSelectionColor.Bind(args => TargetingBoxPipeline.SetBrightColor(args.NewValue)));

        bindings.Add(client.Graphics.PostProcessingAntiAliasingQuality.Bind(args =>
            Graphics.Instance.ApplyPostProcessingAntiAliasingQuality(args.NewValue)));

        bindings.Add(client.Graphics.RenderingAntiAliasingQuality.Bind(args =>
            Graphics.Instance.ApplyRenderingAntiAliasingQuality(args.NewValue)));
    }

    /// <summary>
    ///     Data defining the antialiasing settings used in raytracing.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = ShaderBuffers.Pack)]
    public struct AntiAliasingSettings : IEquatable<AntiAliasingSettings>
    {
        /// <summary>
        ///     Creates a new instance of <see cref="AntiAliasingSettings" />.
        /// </summary>
        public AntiAliasingSettings() {}

        /// <summary>
        ///     Whether adaptive antialiasing for ray generation is enabled.
        /// </summary>
        public Bool isEnabled;

        /// <summary>
        ///     Whether to visualize the sampling rate in the rendered image.
        /// </summary>
        public Bool showSamplingRate;

        /// <summary>
        ///     The size of the sampling grid used initially per pixel.
        /// </summary>
        public Int32 minGridSize = 1;

        /// <summary>
        ///     The size of the maximum sampling grid used per pixel.
        /// </summary>
        public Int32 maxGridSize = 1;

        /// <summary>
        ///     The color variance threshold, determining if more samples are needed for a pixel.
        /// </summary>
        public Single varianceThreshold;

        /// <summary>
        ///     The depth threshold, determining if more samples are needed for a pixel.
        /// </summary>
        public Single depthThreshold;

        private (Boolean, Boolean, Int32, Int32, Single, Single) Pack => (isEnabled, showSamplingRate, minGridSize, maxGridSize, varianceThreshold, depthThreshold);

        /// <inheritdoc />
        public Boolean Equals(AntiAliasingSettings other)
        {
            return Pack.Equals(other.Pack);
        }

        /// <inheritdoc />
        public override Boolean Equals(Object? obj)
        {
            return obj is AntiAliasingSettings other && Equals(other);
        }

        /// <inheritdoc />
        public override Int32 GetHashCode()
        {
            return Pack.GetHashCode();
        }

        /// <summary>
        ///     Check if two <see cref="AntiAliasingSettings" />s are equal.
        /// </summary>
        public static Boolean operator ==(AntiAliasingSettings left, AntiAliasingSettings right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Check if two <see cref="AntiAliasingSettings" />s are not equal.
        /// </summary>
        public static Boolean operator !=(AntiAliasingSettings left, AntiAliasingSettings right)
        {
            return !left.Equals(right);
        }
    }

    /// <summary>
    ///     Data defining the FXAA post-processing settings.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = ShaderBuffers.Pack)]
    public struct FxaaSettings : IEquatable<FxaaSettings>
    {
        /// <summary>
        ///     Whether FXAA is enabled.
        /// </summary>
        public Bool isEnabled;

        /// <summary>
        ///     The absolute contrast threshold for edge detection.
        /// </summary>
        public Single contrastThreshold;

        /// <summary>
        ///     The relative contrast threshold for edge detection.
        /// </summary>
        public Single relativeThreshold;

        /// <summary>
        ///     The factor controlling the strength of subpixel blending.
        /// </summary>
        public Single subpixelBlending;

        /// <summary>
        ///     The maximum number of iterations when stepping along an edge.
        /// </summary>
        public Int32 edgeStepCount;

        /// <summary>
        ///     The increment used when stepping along an edge.
        /// </summary>
        public Int32 edgeStep;

        /// <summary>
        ///     A heuristic value used to estimate the end of an edge.
        /// </summary>
        public Single edgeGuess;

        private (Boolean, Single, Single, Single, Int32, Int32, Single) Pack =>
            (isEnabled, contrastThreshold, relativeThreshold, subpixelBlending, edgeStepCount, edgeStep, edgeGuess);

        /// <inheritdoc />
        public Boolean Equals(FxaaSettings other)
        {
            return Pack.Equals(other.Pack);
        }

        /// <inheritdoc />
        public override Boolean Equals(Object? obj)
        {
            return obj is FxaaSettings other && Equals(other);
        }

        /// <inheritdoc />
        public override Int32 GetHashCode()
        {
            return Pack.GetHashCode();
        }

        /// <summary>
        ///     Check if two <see cref="FxaaSettings" />s are equal.
        /// </summary>
        public static Boolean operator ==(FxaaSettings left, FxaaSettings right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Check if two <see cref="FxaaSettings" />s are not equal.
        /// </summary>
        public static Boolean operator !=(FxaaSettings left, FxaaSettings right)
        {
            return !left.Equals(right);
        }
    }

    /// <summary>
    ///     Data passed to the raytracing shaders.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = ShaderBuffers.Pack)]
    public struct RaytracingData : IEquatable<RaytracingData>, IDefault<RaytracingData>
    {
        /// <summary>
        ///     Creates a new instance of <see cref="RaytracingData" />.
        /// </summary>
        public RaytracingData() {}

        /// <inheritdoc />
        [UsedImplicitly] public static RaytracingData Default => new();

        /// <summary>
        ///     Whether to render in wireframe mode.
        /// </summary>
        public Bool wireframe;

        /// <summary>
        ///     The wind direction, used for foliage swaying.
        /// </summary>
        public Vector3 windDirection = new Vector3(x: 0.7f, y: 0.0f, z: 0.7f).Normalized();

        /// <summary>
        ///     The size of the part of the view plane that is inside a fog volume. Given in relative size, positive values start
        ///     from the bottom, negative values from the top.
        /// </summary>
        public Single fogOverlapSize;

        /// <summary>
        ///     Color of the fog volume the view plane is currently in, represented as an RGB vector.
        /// </summary>
        public Vector3 fogOverlapColor;

        /// <summary>
        ///     The color of the sky, represented as an RGB vector.
        /// </summary>
        public Vector3 skyColor = new(x: 0.5f, y: 0.8f, z: 0.9f);

        [UsedImplicitly] private readonly Single padding0;

        /// <summary>
        ///     The color of the air (background) fog, represented as an RGB vector.
        /// </summary>
        public Vector3 airFogColor = new(x: 0.8f, y: 0.85f, z: 0.9f);

        /// <summary>
        ///     The density of the air (background) fog.
        /// </summary>
        public Single airFogDensity;

        /// <summary>
        ///     The antialiasing settings for ray generation.
        /// </summary>
        public AntiAliasingSettings antiAliasing;

        private (Boolean, Vector3, Single, Vector3, Vector3, Vector3, Single, AntiAliasingSettings) Pack =>
            (wireframe, windDirection, fogOverlapSize, fogOverlapColor, skyColor, airFogColor, airFogDensity, antiAliasing);

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

    /// <summary>
    ///     Data passed to the post-processing shader.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = ShaderBuffers.Pack)]
    public struct PostProcessingData : IEquatable<PostProcessingData>, IDefault<PostProcessingData>
    {
        /// <summary>
        ///     Creates a new instance of <see cref="PostProcessingData" />.
        /// </summary>
        public PostProcessingData() {}

        /// <inheritdoc />
        [UsedImplicitly] public static PostProcessingData Default => new();

        /// <summary>
        ///     The FXAA settings used during post-processing.
        /// </summary>
        public FxaaSettings fxaa = new();

        private FxaaSettings Pack => fxaa;

        /// <inheritdoc />
        public Boolean Equals(PostProcessingData other)
        {
            return Pack.Equals(other.Pack);
        }

        /// <inheritdoc />
        public override Boolean Equals(Object? obj)
        {
            return obj is PostProcessingData other && Equals(other);
        }

        /// <inheritdoc />
        public override Int32 GetHashCode()
        {
            return Pack.GetHashCode();
        }

        /// <summary>
        ///     Check if two <see cref="PostProcessingData" />s are equal.
        /// </summary>
        public static Boolean operator ==(PostProcessingData left, PostProcessingData right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Check if two <see cref="PostProcessingData" />s are not equal.
        /// </summary>
        public static Boolean operator !=(PostProcessingData left, PostProcessingData right)
        {
            return !left.Equals(right);
        }
    }

    #region DISPOSABLE

    private Boolean disposed;

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;
        if (!disposing) return;

        foreach (IDisposable binding in bindings)
            binding.Dispose();

        CrosshairPipeline.Dispose();
        OverlayPipeline.Dispose();

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
    ~Engine()
    {
        Dispose(disposing: false);
    }

    #endregion DISPOSABLE
}
