﻿// <copyright file="Biome.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using OpenTK.Mathematics;
using VoxelGame.Core.Generation.Worlds.Default.Deco;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Toolkit.Noise;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Generation.Worlds.Default;

/// <summary>
///     A biome is a collection of attributes of an area in the world.
/// </summary>
public sealed class Biome : IDisposable
{
    private readonly String name;

    private (Layer layer, Int32 depth)[] lowerHorizon = null!;

    /// <summary>
    ///     The depth to the solid layer, without dampening.
    /// </summary>
    private Int32 minDepthToSolid;

    /// <summary>
    ///     The minimum width of the biome, without dampening.
    /// </summary>
    private Int32 minWidth;

    private NoiseGenerator? noise;

    private (Layer layer, Int32 depth)[] upperHorizon = null!;

    /// <summary>
    ///     Create a new biome. Most values must be set with the init-properties.
    /// </summary>
    /// <param name="name">The name of the biome.</param>
    public Biome(String name)
    {
        this.name = name;
    }

    /// <summary>
    ///     A color representing the biome.
    /// </summary>
    public Color Color { get; init; }

    /// <summary>
    ///     Get the normal width of the ice layer on oceans.
    /// </summary>
    public Int32 IceWidth { get; init; }

    /// <summary>
    ///     The amplitude of the noise used to generate the biome.
    /// </summary>
    public Single Amplitude { get; init; }

    /// <summary>
    ///     The frequency of the noise used to generate the biome.
    /// </summary>
    public Single Frequency { get; init; }

    /// <summary>
    ///     All layers that are part of the biome.
    /// </summary>
    public IList<Layer> Layers { get; init; } = null!;

    /// <summary>
    ///     Get all decorations of this biome.
    /// </summary>
    public ICollection<Decoration> Decorations { get; init; } = new List<Decoration>();

    /// <summary>
    ///     Get the structure of this biome, if any.
    ///     Each biome can only have one structure.
    /// </summary>
    public GeneratedStructure? Structure { get; init; }

    /// <summary>
    ///     Get the cover of the biome.
    /// </summary>
    public Cover Cover { get; init; } = null!;

    /// <summary>
    ///     The width of the dampening layer.
    /// </summary>
    private Int32 MaxDampenWidth { get; set; }

    /// <summary>
    ///     The depth to the dampening layer.
    /// </summary>
    private Int32 DepthToDampen { get; set; }

    /// <summary>
    ///     The dampen layer.
    /// </summary>
    private Layer? Dampen { get; set; }

    /// <summary>
    ///     Set up the biome. This must be called after all init-properties have been set.
    /// </summary>
    /// <param name="factory">The noise generator factory to use.</param>
    /// <param name="palette">The palette to use for the biome.</param>
    public void SetUpBiome(NoiseFactory factory, Palette palette)
    {
        SetUpNoise(factory.CreateNext());
        SetUpLayers(palette);

        Cover.SetUpNoise(factory.CreateNext());
    }

    private void SetUpNoise(NoiseBuilder builder)
    {
        noise = builder
            .WithType(NoiseType.GradientNoise)
            .WithFrequency(Frequency)
            .WithFractals()
            .WithOctaves(octaves: 3)
            .WithLacunarity(lacunarity: 2.0f)
            .WithGain(gain: 0.5f)
            .WithWeightedStrength(weightedStrength: 0.0f)
            .Build();
    }

    private void SetUpLayers(Palette palette)
    {
        minWidth = 0;
        minDepthToSolid = 0;

        var hasReachedSolid = false;

        List<(Layer layer, Int32 depth)> newUpperHorizon = new();
        List<(Layer layer, Int32 depth)> newLowerHorizon = new();

        List<(Layer layer, Int32 depth)> currentHorizon = newUpperHorizon;

        foreach (Layer layer in Layers)
        {
            layer.SetPalette(palette);

            if (!hasReachedSolid && layer.IsSolid)
            {
                hasReachedSolid = true;
                minDepthToSolid = minWidth;
            }

            if (layer.IsDampen)
            {
                DepthToDampen = minWidth;
                MaxDampenWidth = layer.Width;
                Dampen = layer;

                currentHorizon = newLowerHorizon;

                continue;
            }

            minWidth += layer.Width;

            for (var depth = 0; depth < layer.Width; depth++) currentHorizon.Add((layer, depth));
        }

        Debug.Assert(hasReachedSolid);
        Debug.Assert(Dampen != null);

        upperHorizon = newUpperHorizon.ToArray();
        lowerHorizon = newLowerHorizon.ToArray();
    }

    /// <summary>
    ///     Get an offset value for the given column, which can be applied to the height.
    /// </summary>
    /// <param name="position">The position of the column.</param>
    /// <returns>The offset value.</returns>
    public Single GetOffset(Vector2i position)
    {
        Debug.Assert(noise != null);

        return noise.GetNoise(position) * Amplitude;
    }

    /// <summary>
    ///     Calculate the dampening that is applied to a column, depending on the offset.
    /// </summary>
    /// <param name="originalOffset">The offset of the colum.</param>
    /// <returns>The applied dampening.</returns>
    public Dampening CalculateDampening(Int32 originalOffset)
    {
        const Int32 dampenThreshold = 2;
        Int32 normalWidth = MaxDampenWidth / 2;

        if (Math.Abs(originalOffset) <= dampenThreshold) return new Dampening(originalOffset, originalOffset, normalWidth);

        Int32 maxDampening = MaxDampenWidth / 2;
        Int32 dampenedOffset = Math.Clamp(Math.Abs(originalOffset) - dampenThreshold, min: 0, maxDampening) * Math.Sign(originalOffset);

        return new Dampening(dampenedOffset, originalOffset, normalWidth + dampenedOffset);
    }

    /// <summary>
    ///     Get the total width of the biome, depending on the dampening.
    /// </summary>
    /// <param name="dampening">The dampening.</param>
    /// <returns>The total width of the biome.</returns>
    public Int32 GetTotalWidth(Dampening dampening)
    {
        return minWidth + dampening.Width;
    }

    /// <summary>
    ///     Get the biome content content for a given depth beneath the surface level.
    /// </summary>
    /// <param name="depthBelowSurface">The depth beneath the terrain surface level.</param>
    /// <param name="dampening">The dampening to apply to the column.</param>
    /// <param name="stoneType">The stone type of the column.</param>
    /// <param name="isFilled">Whether this column is filled with water.</param>
    /// <returns>The biome content.</returns>
    public Content GetContent(Int32 depthBelowSurface, Dampening dampening, Map.StoneType stoneType, Boolean isFilled)
    {
        Layer current;
        Int32 depthInLayer;
        Int32 actualOffset;

        Boolean isInUpperHorizon = depthBelowSurface < DepthToDampen;

        if (isInUpperHorizon)
        {
            (current, depthInLayer) = upperHorizon[depthBelowSurface];
            actualOffset = dampening.OriginalOffset;
        }
        else
        {
            (actualOffset, _, Int32 usedWidth) = dampening;
            Int32 depthToLowerHorizon = DepthToDampen + usedWidth;

            if (depthBelowSurface < depthToLowerHorizon) (current, depthInLayer) = (Dampen!, depthBelowSurface - DepthToDampen);
            else (current, depthInLayer) = lowerHorizon[depthBelowSurface - depthToLowerHorizon];
        }

        Int32 actualDepthToSolid = minDepthToSolid + dampening.Width;
        Boolean isFilledAtCurrentDepth = depthBelowSurface < actualDepthToSolid && isFilled;

        return current.GetContent(depthInLayer, actualOffset, stoneType, isFilledAtCurrentDepth);
    }

    /// <summary>
    ///     Get the depth to the first solid layer, depending on the dampening.
    /// </summary>
    public Int32 GetDepthToSolid(Dampening dampening)
    {
        return minDepthToSolid + dampening.Width;
    }

    /// <inheritdoc />
    public override String ToString()
    {
        return name;
    }

    /// <summary>
    ///     The dampening applied to a column.
    /// </summary>
    public record struct Dampening(Int32 DampenedOffset, Int32 OriginalOffset, Int32 Width);

    #region IDisposable Support

    private Boolean disposed;

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing)
        {
            noise?.Dispose();

            Cover.Dispose();
        }
        else
        {
            Throw.ForMissedDispose(this);
        }

        disposed = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~Biome()
    {
        Dispose(disposing: false);
    }

    #endregion
}
