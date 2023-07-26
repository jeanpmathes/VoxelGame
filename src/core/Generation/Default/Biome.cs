// <copyright file="Biome.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using OpenTK.Mathematics;
using VoxelGame.Core.Generation.Default.Deco;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Generation.Default;

/// <summary>
///     A biome is a collection of attributes of an area in the world.
/// </summary>
public class Biome
{
    private readonly string name;
    private (Layer layer, int depth)[] lowerHorizon = null!;

    /// <summary>
    ///     The depth to the solid layer, without dampening.
    /// </summary>
    private int minDepthToSolid;

    /// <summary>
    ///     The minimum width of the biome, without dampening.
    /// </summary>
    private int minWidth;

    private FastNoiseLite noise = null!;

    private (Layer layer, int depth)[] upperHorizon = null!;

    /// <summary>
    ///     Create a new biome. Most values must be set with the init-properties.
    /// </summary>
    /// <param name="name">The name of the biome.</param>
    public Biome(string name)
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
    public int IceWidth { get; init; }

    /// <summary>
    ///     The amplitude of the noise used to generate the biome.
    /// </summary>
    public float Amplitude { get; init; }

    /// <summary>
    ///     The frequency of the noise used to generate the biome.
    /// </summary>
    public float Frequency { get; init; }

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
    private int MaxDampenWidth { get; set; }

    /// <summary>
    ///     The depth to the dampening layer.
    /// </summary>
    private int DepthToDampen { get; set; }

    /// <summary>
    ///     The dampen layer.
    /// </summary>
    private Layer? Dampen { get; set; }

    /// <summary>
    ///     Setup the biome. This must be called after all init-properties have been set.
    /// </summary>
    /// <param name="factory">The noise generator factory to use.</param>
    /// <param name="palette">The palette to use for the biome.</param>
    public void SetupBiome(NoiseFactory factory, Palette palette)
    {
        SetupNoise(factory.GetNextNoise());
        SetupLayers(palette);

        Cover.SetupNoise(factory.GetNextNoise());
    }

    private void SetupNoise(FastNoiseLite noiseGenerator)
    {
        noise = noiseGenerator;

        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetFrequency(Frequency);

        noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        noise.SetFractalOctaves(octaves: 3);
        noise.SetFractalLacunarity(lacunarity: 2.0f);
        noise.SetFractalGain(gain: 0.5f);
        noise.SetFractalWeightedStrength(weightedStrength: 0.0f);

        noise.SetDomainWarpType(FastNoiseLite.DomainWarpType.OpenSimplex2);
        noise.SetDomainWarpAmp(domainWarpAmp: 30.0f);
    }


    private void SetupLayers(Palette palette)
    {
        minWidth = 0;
        minDepthToSolid = 0;

        var hasReachedSolid = false;

        List<(Layer layer, int depth)> newUpperHorizon = new();
        List<(Layer layer, int depth)> newLowerHorizon = new();

        List<(Layer layer, int depth)> currentHorizon = newUpperHorizon;

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
    ///     Get a offset value for the given column, which can be applied to the height.
    /// </summary>
    /// <param name="position">The position of the column.</param>
    /// <returns>The offset value.</returns>
    public float GetOffset(Vector2i position)
    {
        return noise.GetNoise(position.X, position.Y) * Amplitude;
    }

    /// <summary>
    ///     Calculate the dampening that is applied to a column, depending on the offset.
    /// </summary>
    /// <param name="originalOffset">The offset of the colum.</param>
    /// <returns>The applied dampening.</returns>
    public Dampening CalculateDampening(int originalOffset)
    {
        const int dampenThreshold = 2;
        int normalWidth = MaxDampenWidth / 2;

        if (Math.Abs(originalOffset) <= dampenThreshold) return new Dampening(originalOffset, originalOffset, normalWidth);

        int maxDampening = MaxDampenWidth / 2;
        int dampenedOffset = Math.Clamp(Math.Abs(originalOffset) - dampenThreshold, min: 0, maxDampening) * Math.Sign(originalOffset);

        return new Dampening(dampenedOffset, originalOffset, normalWidth + dampenedOffset);
    }

    /// <summary>
    ///     Get the total width of the biome, depending on the dampening.
    /// </summary>
    /// <param name="dampening">The dampening.</param>
    /// <returns>The total width of the biome.</returns>
    public int GetTotalWidth(Dampening dampening)
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
    public Content GetContent(int depthBelowSurface, Dampening dampening, Map.StoneType stoneType, bool isFilled)
    {
        Layer current;
        int depthInLayer;
        int actualOffset;

        bool isInUpperHorizon = depthBelowSurface < DepthToDampen;

        if (isInUpperHorizon)
        {
            (current, depthInLayer) = upperHorizon[depthBelowSurface];
            actualOffset = dampening.OriginalOffset;
        }
        else
        {
            (actualOffset, _, int usedWidth) = dampening;
            int depthToLowerHorizon = DepthToDampen + usedWidth;

            if (depthBelowSurface < depthToLowerHorizon) (current, depthInLayer) = (Dampen!, depthBelowSurface - DepthToDampen);
            else (current, depthInLayer) = lowerHorizon[depthBelowSurface - depthToLowerHorizon];
        }

        int actualDepthToSolid = minDepthToSolid + dampening.Width;
        bool isFilledAtCurrentDepth = depthBelowSurface < actualDepthToSolid && isFilled;

        return current.GetContent(depthInLayer, actualOffset, stoneType, isFilledAtCurrentDepth);
    }

    /// <summary>
    ///     Get the depth to the first solid layer, depending on the dampening.
    /// </summary>
    public int GetDepthToSolid(Dampening dampening)
    {
        return minDepthToSolid + dampening.Width;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return name;
    }

    /// <summary>
    ///     The dampening applied to a column.
    /// </summary>
    public record struct Dampening(int DampenedOffset, int OriginalOffset, int Width);
}
