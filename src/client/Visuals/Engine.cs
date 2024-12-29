// <copyright file="Shaders.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Graphics.Objects;

namespace VoxelGame.Client.Visuals;

/// <summary>
///     The graphics engine, consisting of all renderers and pipelines.
/// </summary>
public sealed class Engine : IResource
{
    private readonly List<IDisposable> bindings = [];
    private readonly ShaderBuffer<RaytracingData>? raytracingDataBuffer;

    internal Engine(Application.Client client, ScreenElementVFX crosshairVFX, OverlayVFX overlayVFX, SelectionBoxVFX selectionBoxVFX, ShaderBuffer<RaytracingData>? rtData)
    {
        CrosshairVFX = crosshairVFX;
        OverlayVFX = overlayVFX;
        SelectionBoxVFX = selectionBoxVFX;

        raytracingDataBuffer = rtData;

        bindings.Add(client.Settings.CrosshairColor.Bind(args => CrosshairVFX.SetColor(args.NewValue)));
        bindings.Add(client.Settings.CrosshairScale.Bind(args => CrosshairVFX.SetScale(args.NewValue)));

        bindings.Add(client.Settings.DarkSelectionColor.Bind(args => SelectionBoxVFX.SetDarkColor(args.NewValue)));
        bindings.Add(client.Settings.BrightSelectionColor.Bind(args => SelectionBoxVFX.SetBrightColor(args.NewValue)));
    }

    /// <summary>
    ///     Get the selection box renderer, which is used to draw selection boxes around blocks.
    /// </summary>
    public SelectionBoxVFX SelectionBoxVFX { get; }

    /// <summary>
    ///     Get the crosshair renderer, which is used to draw the crosshair.
    /// </summary>
    public ScreenElementVFX CrosshairVFX { get; }

    /// <summary>
    ///     Get the overlay renderer, which is used to draw overlays, e.g. when stuck in a block.
    /// </summary>
    public OverlayVFX OverlayVFX { get; }

    /// <summary>
    ///     Get the raytracing data buffer.
    /// </summary>
    public ShaderBuffer<RaytracingData> RaytracingDataBuffer => raytracingDataBuffer!;

    /// <inheritdoc />
    public RID Identifier { get; } = RID.Named<Engine>("Default");

    /// <inheritdoc />
    public ResourceType Type => ResourceTypes.Engine;

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

    #region DISPOSABLE

    private Boolean disposed;

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;
        if (!disposing) return;

        foreach (IDisposable binding in bindings)
            binding.Dispose();

        CrosshairVFX.Dispose();
        OverlayVFX.Dispose();

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
