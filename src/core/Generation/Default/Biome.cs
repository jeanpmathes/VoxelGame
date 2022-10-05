// <copyright file="Biome.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
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
        Frequency = 0.007f,
        Layers = new List<Layer>
        {
            Layer.CreateSnow(width: 3),
            Layer.CreatePermeable(Block.Dirt, width: 5),
            Layer.CreatePermeableDampen(Block.Dirt, maxWidth: 4),
            Layer.CreateSolid(Block.Permafrost, width: 27),
            Layer.CreateLoose(width: 27),
            Layer.CreateGroundwater(width: 8),
            Layer.CreateSolid(Block.Clay, width: 21)
        }
    };

    /// <summary>
    /// The tropical rainforest biome.
    /// </summary>
    public static readonly Biome TropicalRainforest = new(nameof(TropicalRainforest))
    {
        Color = Color.DarkGreen,
        Amplitude = 15f,
        Frequency = 0.005f,
        Layers = new List<Layer>
        {
            Layer.CreateTop(Block.Grass, Block.Dirt, width: 1),
            Layer.CreatePermeable(Block.Dirt, width: 3),
            Layer.CreatePermeableDampen(Block.Dirt, maxWidth: 26),
            Layer.CreateLoose(width: 3),
            Layer.CreateGroundwater(width: 6),
            Layer.CreateSolid(Block.Clay, width: 3),
            Layer.CreateLoose(width: 33),
            Layer.CreateGroundwater(width: 18),
            Layer.CreateSolid(Block.Clay, width: 21)
        }
    };

    /// <summary>
    /// The temperate rainforest biome.
    /// </summary>
    public static readonly Biome TemperateRainforest = new(nameof(TemperateRainforest))
    {
        Color = Color.Green,
        Amplitude = 15f,
        Frequency = 0.005f,
        Layers = new List<Layer>
        {
            Layer.CreateTop(Block.Grass, Block.Dirt, width: 1),
            Layer.CreatePermeable(Block.Dirt, width: 3),
            Layer.CreatePermeableDampen(Block.Dirt, maxWidth: 26),
            Layer.CreateLoose(width: 3),
            Layer.CreateGroundwater(width: 6),
            Layer.CreateSolid(Block.Clay, width: 3),
            Layer.CreateLoose(width: 33),
            Layer.CreateGroundwater(width: 18),
            Layer.CreateSolid(Block.Clay, width: 21)
        }
    };

    /// <summary>
    /// The taiga biome.
    /// </summary>
    public static readonly Biome Taiga = new(nameof(Taiga))
    {
        Color = Color.Navy,
        Amplitude = 3f,
        Frequency = 0.007f,
        Layers = new List<Layer>
        {
            Layer.CreateTop(Block.Grass, Block.Dirt, width: 1),
            Layer.CreatePermeable(Block.Dirt, width: 7),
            Layer.CreatePermeableDampen(Block.Dirt, maxWidth: 6),
            Layer.CreateSolid(Block.Permafrost, width: 28),
            Layer.CreateLoose(width: 27),
            Layer.CreateGroundwater(width: 8),
            Layer.CreateSolid(Block.Clay, width: 21)
        }
    };

    /// <summary>
    /// The tundra biome.
    /// </summary>
    public static readonly Biome Tundra = new(nameof(Tundra))
    {
        Color = Color.CadetBlue,
        Amplitude = 3f,
        Frequency = 0.007f,
        Layers = new List<Layer>
        {
            Layer.CreateTop(Block.Grass, Block.Dirt, width: 1),
            Layer.CreatePermeable(Block.Dirt, width: 7),
            Layer.CreatePermeableDampen(Block.Dirt, maxWidth: 6),
            Layer.CreateSolid(Block.Permafrost, width: 28),
            Layer.CreateLoose(width: 27),
            Layer.CreateGroundwater(width: 8),
            Layer.CreateSolid(Block.Clay, width: 21)
        }
    };

    /// <summary>
    /// The savanna biome.
    /// </summary>
    public static readonly Biome Savanna = new(nameof(Savanna))
    {
        Color = Color.Olive,
        Amplitude = 1f,
        Frequency = 0.01f,
        Layers = new List<Layer>
        {
            Layer.CreateTop(Block.Grass, Block.Dirt, width: 1),
            Layer.CreatePermeable(Block.Dirt, width: 7),
            Layer.CreatePermeableDampen(Block.Dirt, maxWidth: 2),
            Layer.CreateLoose(width: 3),
            Layer.CreateGroundwater(width: 2),
            Layer.CreateSolid(Block.Clay, width: 3),
            Layer.CreateLoose(width: 37),
            Layer.CreateGroundwater(width: 18),
            Layer.CreateSolid(Block.Clay, width: 21)
        }
    };

    /// <summary>
    /// The seasonal forest biome.
    /// </summary>
    public static readonly Biome SeasonalForest = new(nameof(SeasonalForest))
    {
        Color = Color.LimeGreen,
        Amplitude = 10f,
        Frequency = 0.005f,
        Layers = new List<Layer>
        {
            Layer.CreateTop(Block.Grass, Block.Dirt, width: 1),
            Layer.CreatePermeable(Block.Dirt, width: 5),
            Layer.CreatePermeableDampen(Block.Dirt, maxWidth: 20),
            Layer.CreateLoose(width: 3),
            Layer.CreateGroundwater(width: 2),
            Layer.CreateSolid(Block.Clay, width: 3),
            Layer.CreateLoose(width: 37),
            Layer.CreateGroundwater(width: 18),
            Layer.CreateSolid(Block.Clay, width: 21)
        }
    };

    /// <summary>
    /// The dry forest biome.
    /// </summary>
    public static readonly Biome DryForest = new(nameof(DryForest))
    {
        Color = Color.SeaGreen,
        Amplitude = 15f,
        Frequency = 0.005f,
        Layers = new List<Layer>
        {
            Layer.CreateTop(Block.Grass, Block.Dirt, width: 1),
            Layer.CreatePermeable(Block.Dirt, width: 3),
            Layer.CreatePermeableDampen(Block.Dirt, maxWidth: 26),
            Layer.CreateLoose(width: 3),
            Layer.CreateGroundwater(width: 6),
            Layer.CreateSolid(Block.Clay, width: 3),
            Layer.CreateLoose(width: 33),
            Layer.CreateGroundwater(width: 18),
            Layer.CreateSolid(Block.Clay, width: 21)
        }
    };

    /// <summary>
    /// The shrubland biome.
    /// </summary>
    public static readonly Biome Shrubland = new(nameof(Shrubland))
    {
        Color = Color.Salmon,
        Amplitude = 1f,
        Frequency = 0.01f,
        Layers = new List<Layer>
        {
            Layer.CreateTop(Block.Grass, Block.Dirt, width: 1),
            Layer.CreatePermeable(Block.Dirt, width: 7),
            Layer.CreatePermeableDampen(Block.Dirt, maxWidth: 2),
            Layer.CreateLoose(width: 3),
            Layer.CreateGroundwater(width: 2),
            Layer.CreateSolid(Block.Clay, width: 3),
            Layer.CreateLoose(width: 37),
            Layer.CreateGroundwater(width: 18),
            Layer.CreateSolid(Block.Clay, width: 21)
        }
    };

    /// <summary>
    /// The desert biome.
    /// </summary>
    public static readonly Biome Desert = new(nameof(Desert))
    {
        Color = Color.Yellow,
        Amplitude = 4f,
        Frequency = 0.008f,
        Layers = new List<Layer>
        {
            Layer.CreatePermeable(Block.Sand, width: 9),
            Layer.CreatePermeable(Block.Dirt, width: 4),
            Layer.CreatePermeableDampen(Block.Dirt, maxWidth: 8),
            Layer.CreateSolid(Block.Sandstone, width: 18),
            Layer.CreateLoose(width: 22),
            Layer.CreateGroundwater(width: 18),
            Layer.CreateSolid(Block.Clay, width: 21)
        }
    };

    /// <summary>
    /// The grassland biome.
    /// </summary>
    public static readonly Biome Grassland = new(nameof(Grassland))
    {
        Color = Color.SaddleBrown,
        Amplitude = 4f,
        Frequency = 0.004f,
        Layers = new List<Layer>
        {
            Layer.CreateTop(Block.Grass, Block.Dirt, width: 1),
            Layer.CreatePermeable(Block.Dirt, width: 7),
            Layer.CreatePermeableDampen(Block.Dirt, maxWidth: 8),
            Layer.CreateLoose(width: 3),
            Layer.CreateGroundwater(width: 2),
            Layer.CreateSolid(Block.Clay, width: 3),
            Layer.CreateLoose(width: 37),
            Layer.CreateGroundwater(width: 18),
            Layer.CreateSolid(Block.Clay, width: 21)
        }
    };

    /// <summary>
    ///     The normal ocean biome.
    /// </summary>
    public static readonly Biome Ocean = new(nameof(Ocean))
    {
        Color = Color.White,
        Amplitude = 5.0f,
        Frequency = 0.005f,
        Layers = new List<Layer>
        {
            Layer.CreatePermeable(Block.Sand, width: 5),
            Layer.CreatePermeable(Block.Gravel, width: 3),
            Layer.CreatePermeableDampen(Block.Gravel, maxWidth: 10),
            Layer.CreateSolid(Block.Limestone, width: 26),
            Layer.CreateLoose(width: 37),
            Layer.CreateSolid(Block.Limestone, width: 21)
        }
    };

    /// <summary>
    ///     The polar ocean biome. It is covered in ice and occurs in cold regions.
    /// </summary>
    public static readonly Biome PolarOcean = new(nameof(PolarOcean))
    {
        Color = Color.White,
        Amplitude = 5.0f,
        Frequency = 0.005f,
        IceWidth = 6,
        Layers = new List<Layer>
        {
            Layer.CreatePermeable(Block.Sand, width: 5),
            Layer.CreatePermeable(Block.Gravel, width: 3),
            Layer.CreatePermeableDampen(Block.Gravel, maxWidth: 10),
            Layer.CreateSolid(Block.Limestone, width: 26),
            Layer.CreateLoose(width: 37),
            Layer.CreateSolid(Block.Limestone, width: 21)
        }
    };

    /// <summary>
    ///     The mountain biome. It is a special biome that depends on the height of the terrain.
    /// </summary>
    public static readonly Biome Mountains = new(nameof(Mountains))
    {
        Color = Color.Black,
        Amplitude = 30f,
        Frequency = 0.005f,
        Layers = new List<Layer>
        {
            Layer.CreateStonyTop(width: 9, amplitude: 15),
            Layer.CreateStonyDampen(maxWidth: 31),
            Layer.CreateStone(width: 31),
            Layer.CreateLoose(width: 9),
            Layer.CreateGroundwater(width: 1),
            Layer.CreateSolid(Block.Clay, width: 9)
        }
    };

    /// <summary>
    ///     The beach biome. It is found at low heights next to coastlines.
    /// </summary>
    public static readonly Biome Beach = new(nameof(Beach))
    {
        Color = Color.Black,
        Amplitude = 4f,
        Frequency = 0.008f,
        Layers = new List<Layer>
        {
            Layer.CreatePermeable(Block.Sand, width: 5),
            Layer.CreatePermeable(Block.Gravel, width: 3),
            Layer.CreatePermeableDampen(Block.Gravel, maxWidth: 10),
            Layer.CreateSolid(Block.Limestone, width: 13),
            Layer.CreateLoose(width: 22),
            Layer.CreateGroundwater(width: 18),
            Layer.CreateSolid(Block.Clay, width: 21)
        }
    };

    /// <summary>
    ///     The grass covered cliff biome, which is found at coastlines with large height differences.
    /// </summary>
    public static readonly Biome GrassyCliff = new(nameof(GrassyCliff))
    {
        Color = Color.Black,
        Amplitude = 4f,
        Frequency = 0.008f,
        Layers = new List<Layer>
        {
            Layer.CreateTop(Block.Grass, Block.Dirt, width: 1),
            Layer.CreateSolid(Block.Limestone, width: 53),
            Layer.CreateStonyDampen(maxWidth: 28),
            Layer.CreateStone(width: 39)
        }
    };

    /// <summary>
    ///     The sand covered cliff biome, which is found at coastlines with large height differences.
    /// </summary>
    public static readonly Biome SandyCliff = new(nameof(SandyCliff))
    {
        Color = Color.Black,
        Amplitude = 4f,
        Frequency = 0.008f,
        Layers = new List<Layer>
        {
            Layer.CreateTop(Block.Grass, Block.Dirt, width: 1),
            Layer.CreateSolid(Block.Limestone, width: 53),
            Layer.CreateStonyDampen(maxWidth: 28),
            Layer.CreateStone(width: 39)
        }
    };

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

    private Biome(string name)
    {
        OnSetup += SetupBiome;
        this.name = name;
    }

    /// <summary>
    /// A color representing the biome.
    /// </summary>
    public Color Color { get; private init; }

    /// <summary>
    ///     Get the normal width of the ice layer on oceans.
    /// </summary>
    public int IceWidth { get; private init; }

    private float Amplitude { get; init; }

    private float Frequency { get; init; }

    private List<Layer> Layers { get; init; } = null!;

    /// <summary>
    /// The width of the dampening layer.
    /// </summary>
    private int MaxDampenWidth { get; set; }

    /// <summary>
    /// The depth to the dampening layer.
    /// </summary>
    private int DepthToDampen { get; set; }

    /// <summary>
    /// The dampen layer.
    /// </summary>
    private Layer? Dampen { get; set; }

    /// <summary>
    ///     Setup all biomes for current world generation.
    ///     Because biomes need setup, only one world can be generated at a time.
    /// </summary>
    /// <param name="seed">The seed to use for the noise generation.</param>
    /// <param name="palette">The palette to use for the generating.</param>
    public static void Setup(int seed, Palette palette)
    {
        Debug.Assert(OnSetup != null);
        OnSetup(sender: null, (seed, palette));
    }

    private static event EventHandler<(int seed, Palette palette)>? OnSetup;

    private void SetupBiome(object? sender, (int seed, Palette palette) arguments)
    {
        SetupNoise(arguments.seed);
        SetupLayers(arguments.palette);
    }

    private void SetupNoise(int seed)
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
