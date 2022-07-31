// <copyright file="Biome.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Diagnostics;
using System.Drawing;
using OpenTK.Mathematics;

namespace VoxelGame.Core.Generation.Default;

/// <summary>
/// A biome is a collection of attributes of an area in the world.
/// </summary>
public class Biome
{
    /// <summary>
    /// The polar desert biome.
    /// </summary>
    public static readonly Biome PolarDesert = new()
    {
        Color = Color.Gray,
        Amplitude = 2f,
        Frequency = 0.007f
    };

    /// <summary>
    /// The tropical rainforest biome.
    /// </summary>
    public static readonly Biome TropicalRainforest = new()
    {
        Color = Color.DarkGreen,
        Amplitude = 15f,
        Frequency = 0.005f
    };

    /// <summary>
    /// The temperate rainforest biome.
    /// </summary>
    public static readonly Biome TemperateRainforest = new()
    {
        Color = Color.Green,
        Amplitude = 15f,
        Frequency = 0.005f
    };

    /// <summary>
    /// The taiga biome.
    /// </summary>
    public static readonly Biome Taiga = new()
    {
        Color = Color.Navy,
        Amplitude = 3f,
        Frequency = 0.007f
    };

    /// <summary>
    /// The tundra biome.
    /// </summary>
    public static readonly Biome Tundra = new()
    {
        Color = Color.CadetBlue,
        Amplitude = 3f,
        Frequency = 0.007f
    };

    /// <summary>
    /// The savanna biome.
    /// </summary>
    public static readonly Biome Savanna = new()
    {
        Color = Color.Olive,
        Amplitude = 1f,
        Frequency = 0.01f
    };

    /// <summary>
    /// The seasonal forest biome.
    /// </summary>
    public static readonly Biome SeasonalForest = new()
    {
        Color = Color.LimeGreen,
        Amplitude = 10f,
        Frequency = 0.005f
    };

    /// <summary>
    /// The dry forest biome.
    /// </summary>
    public static readonly Biome DryForest = new()
    {
        Color = Color.SeaGreen,
        Amplitude = 15f,
        Frequency = 0.005f
    };

    /// <summary>
    /// The shrubland biome.
    /// </summary>
    public static readonly Biome Shrubland = new()
    {
        Color = Color.Salmon,
        Amplitude = 1f,
        Frequency = 0.01f
    };

    /// <summary>
    /// The desert biome.
    /// </summary>
    public static readonly Biome Desert = new()
    {
        Color = Color.Yellow,
        Amplitude = 4f,
        Frequency = 0.008f
    };

    /// <summary>
    /// The grassland biome.
    /// </summary>
    public static readonly Biome Grassland = new()
    {
        Color = Color.SaddleBrown,
        Amplitude = 4f,
        Frequency = 0.004f
    };

    /// <summary>
    ///     The ocean biome.
    /// </summary>
    public static readonly Biome Ocean = new()
    {
        Color = Color.White,
        Amplitude = 5.0f,
        Frequency = 0.005f
    };

    private FastNoiseLite noise = null!;

    private Biome()
    {
        OnSetup += SetupBiome;
    }

    /// <summary>
    /// A color representing the biome.
    /// </summary>
    public Color Color { get; private init; }

    private float Amplitude { get; init; }

    private float Frequency { get; init; }

    /// <summary>
    ///     Setup all biomes for current world generation.
    ///     Because biomes need setup, only one world can be generated at a time.
    /// </summary>
    /// <param name="seed">The seed to use for the noise generation.</param>
    public static void Setup(int seed)
    {
        Debug.Assert(OnSetup != null);
        OnSetup(sender: null, seed);
    }

    private static event EventHandler<int>? OnSetup;

    private void SetupBiome(object? sender, int seed)
    {
        noise = new FastNoiseLite(seed);

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

    /// <summary>
    ///     Get a offset value for the given column, which can be applied to the height.
    /// </summary>
    /// <param name="position">The position of the column.</param>
    /// <returns>The offset value.</returns>
    public float GetOffset(Vector2i position)
    {
        return noise.GetNoise(position.X, position.Y) * Amplitude;
    }
}
