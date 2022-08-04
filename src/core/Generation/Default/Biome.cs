// <copyright file="Biome.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Diagnostics;
using System.Drawing;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Generation.Default;

/// <summary>
/// A biome is a collection of attributes of an area in the world.
/// </summary>
public class Biome
{
    /// <summary>
    /// The polar desert biome.
    /// </summary>
    public static readonly Biome PolarDesert = new(nameof(PolarDesert))
    {
        Color = Color.Gray,
        Amplitude = 2f,
        Frequency = 0.007f
    };

    /// <summary>
    /// The tropical rainforest biome.
    /// </summary>
    public static readonly Biome TropicalRainforest = new(nameof(TropicalRainforest))
    {
        Color = Color.DarkGreen,
        Amplitude = 15f,
        Frequency = 0.005f
    };

    /// <summary>
    /// The temperate rainforest biome.
    /// </summary>
    public static readonly Biome TemperateRainforest = new(nameof(TemperateRainforest))
    {
        Color = Color.Green,
        Amplitude = 15f,
        Frequency = 0.005f
    };

    /// <summary>
    /// The taiga biome.
    /// </summary>
    public static readonly Biome Taiga = new(nameof(Taiga))
    {
        Color = Color.Navy,
        Amplitude = 3f,
        Frequency = 0.007f
    };

    /// <summary>
    /// The tundra biome.
    /// </summary>
    public static readonly Biome Tundra = new(nameof(Tundra))
    {
        Color = Color.CadetBlue,
        Amplitude = 3f,
        Frequency = 0.007f
    };

    /// <summary>
    /// The savanna biome.
    /// </summary>
    public static readonly Biome Savanna = new(nameof(Savanna))
    {
        Color = Color.Olive,
        Amplitude = 1f,
        Frequency = 0.01f
    };

    /// <summary>
    /// The seasonal forest biome.
    /// </summary>
    public static readonly Biome SeasonalForest = new(nameof(SeasonalForest))
    {
        Color = Color.LimeGreen,
        Amplitude = 10f,
        Frequency = 0.005f
    };

    /// <summary>
    /// The dry forest biome.
    /// </summary>
    public static readonly Biome DryForest = new(nameof(DryForest))
    {
        Color = Color.SeaGreen,
        Amplitude = 15f,
        Frequency = 0.005f
    };

    /// <summary>
    /// The shrubland biome.
    /// </summary>
    public static readonly Biome Shrubland = new(nameof(Shrubland))
    {
        Color = Color.Salmon,
        Amplitude = 1f,
        Frequency = 0.01f
    };

    /// <summary>
    /// The desert biome.
    /// </summary>
    public static readonly Biome Desert = new(nameof(Desert))
    {
        Color = Color.Yellow,
        Amplitude = 4f,
        Frequency = 0.008f
    };

    /// <summary>
    /// The grassland biome.
    /// </summary>
    public static readonly Biome Grassland = new(nameof(Grassland))
    {
        Color = Color.SaddleBrown,
        Amplitude = 4f,
        Frequency = 0.004f
    };

    /// <summary>
    ///     The ocean biome.
    /// </summary>
    public static readonly Biome Ocean = new(nameof(Ocean))
    {
        Color = Color.White,
        Amplitude = 5.0f,
        Frequency = 0.005f
    };

    private readonly string name;

    private FastNoiseLite noise = null!;

    private Biome(string name)
    {
        OnSetup += SetupBiome;
        this.name = name;
    }

    /// <summary>
    /// A color representing the biome.
    /// </summary>
    public Color Color { get; private init; }

    private float Amplitude { get; init; }

    private float Frequency { get; init; }

    private Layer? Top { get; } = new(Block.Grass, width: 1);

    private Layer Permeable { get; } = new(Block.Dirt, width: 3);

    private Layer Solid { get; } = new(Block.Rubble, width: 2);

    /// <summary>
    ///     Get the depths of the different layers. The depth is the number of blocks in the layer and all the layers above.
    /// </summary>
    /// <returns>The depths of the permeable and solid layers.</returns>
    public (int permeable, int solid) Depths
    {
        get
        {
            int top = Top?.Width ?? 0;
            int permeable = top + Permeable.Width;
            int solid = permeable + Solid.Width;

            return (permeable, solid);
        }
    }

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

    /// <summary>
    ///     Get the biome content data for a given depth beneath the surface level.
    /// </summary>
    /// <param name="depthBelowSurface">The depth beneath the terrain surface level.</param>
    /// <param name="isFilled">Whether this column is filled with water.</param>
    /// <returns>The biome content data.</returns>
    public uint GetData(int depthBelowSurface, bool isFilled)
    {
        Layer current;

        (int permeableDepth, _) = Depths;

        if (Top != null && depthBelowSurface < Top.Width) current = isFilled ? Permeable : Top;
        else if (depthBelowSurface < permeableDepth) current = Permeable;
        else current = Solid;

        return current.GetData(isFilled);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return name;
    }
}
