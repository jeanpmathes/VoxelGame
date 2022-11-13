// <copyright file="BiomeDefinitions.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using System.Drawing;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Generation.Default;

public partial class Biome
{
    private static readonly Decoration decoration = new();

    /// <summary>
    ///     The polar desert biome.
    /// </summary>
    public static readonly Biome PolarDesert = new(nameof(PolarDesert))
    {
        Color = Color.Gray,
        Amplitude = 2f,
        Frequency = 0.007f,
        Cover = new Cover(hasPlants: false),
        Layers = new List<Layer>
        {
            Layer.CreateSnow(width: 3),
            Layer.CreateSimple(Block.Dirt, width: 5, isSolid: false),
            Layer.CreatePermeableDampen(Block.Dirt, maxWidth: 4),
            Layer.CreateSimple(Block.Permafrost, width: 27, isSolid: true),
            Layer.CreateLoose(width: 27),
            Layer.CreateGroundwater(width: 8),
            Layer.CreateSimple(Block.Clay, width: 21, isSolid: true)
        }
    };

    /// <summary>
    ///     The tropical rainforest biome.
    /// </summary>
    public static readonly Biome TropicalRainforest = new(nameof(TropicalRainforest))
    {
        Color = Color.DarkGreen,
        Amplitude = 15f,
        Frequency = 0.005f,
        Cover = new Cover(hasPlants: true),
        Layers = new List<Layer>
        {
            Layer.CreateTop(Block.Grass, Block.Dirt, width: 1),
            Layer.CreateSimple(Block.Dirt, width: 3, isSolid: false),
            Layer.CreatePermeableDampen(Block.Dirt, maxWidth: 26),
            Layer.CreateLoose(width: 3),
            Layer.CreateGroundwater(width: 6),
            Layer.CreateSimple(Block.Clay, width: 3, isSolid: true),
            Layer.CreateLoose(width: 33),
            Layer.CreateGroundwater(width: 18),
            Layer.CreateSimple(Block.Clay, width: 21, isSolid: true)
        }
    };

    /// <summary>
    ///     The temperate rainforest biome.
    /// </summary>
    public static readonly Biome TemperateRainforest = new(nameof(TemperateRainforest))
    {
        Color = Color.Green,
        Amplitude = 15f,
        Frequency = 0.005f,
        Cover = new Cover(hasPlants: true),
        Layers = new List<Layer>
        {
            Layer.CreateTop(Block.Grass, Block.Dirt, width: 1),
            Layer.CreateSimple(Block.Dirt, width: 3, isSolid: false),
            Layer.CreatePermeableDampen(Block.Dirt, maxWidth: 26),
            Layer.CreateLoose(width: 3),
            Layer.CreateGroundwater(width: 6),
            Layer.CreateSimple(Block.Clay, width: 3, isSolid: true),
            Layer.CreateLoose(width: 33),
            Layer.CreateGroundwater(width: 18),
            Layer.CreateSimple(Block.Clay, width: 21, isSolid: true)
        }
    };

    /// <summary>
    ///     The taiga biome.
    /// </summary>
    public static readonly Biome Taiga = new(nameof(Taiga))
    {
        Color = Color.Navy,
        Amplitude = 3f,
        Frequency = 0.007f,
        Cover = new Cover(hasPlants: true),
        Layers = new List<Layer>
        {
            Layer.CreateTop(Block.Grass, Block.Dirt, width: 1),
            Layer.CreateSimple(Block.Dirt, width: 7, isSolid: false),
            Layer.CreatePermeableDampen(Block.Dirt, maxWidth: 6),
            Layer.CreateSimple(Block.Permafrost, width: 28, isSolid: true),
            Layer.CreateLoose(width: 27),
            Layer.CreateGroundwater(width: 8),
            Layer.CreateSimple(Block.Clay, width: 21, isSolid: true)
        }
    };

    /// <summary>
    ///     The tundra biome.
    /// </summary>
    public static readonly Biome Tundra = new(nameof(Tundra))
    {
        Color = Color.CadetBlue,
        Amplitude = 3f,
        Frequency = 0.007f,
        Cover = new Cover(hasPlants: true),
        Layers = new List<Layer>
        {
            Layer.CreateTop(Block.Grass, Block.Dirt, width: 1),
            Layer.CreateSimple(Block.Dirt, width: 7, isSolid: false),
            Layer.CreatePermeableDampen(Block.Dirt, maxWidth: 6),
            Layer.CreateSimple(Block.Permafrost, width: 28, isSolid: true),
            Layer.CreateLoose(width: 27),
            Layer.CreateGroundwater(width: 8),
            Layer.CreateSimple(Block.Clay, width: 21, isSolid: true)
        }
    };

    /// <summary>
    ///     The savanna biome.
    /// </summary>
    public static readonly Biome Savanna = new(nameof(Savanna))
    {
        Color = Color.Olive,
        Amplitude = 1f,
        Frequency = 0.01f,
        Cover = new Cover(hasPlants: true),
        Layers = new List<Layer>
        {
            Layer.CreateTop(Block.Grass, Block.Dirt, width: 1),
            Layer.CreateSimple(Block.Dirt, width: 7, isSolid: false),
            Layer.CreatePermeableDampen(Block.Dirt, maxWidth: 2),
            Layer.CreateLoose(width: 3),
            Layer.CreateGroundwater(width: 2),
            Layer.CreateSimple(Block.Clay, width: 3, isSolid: true),
            Layer.CreateLoose(width: 37),
            Layer.CreateGroundwater(width: 18),
            Layer.CreateSimple(Block.Clay, width: 21, isSolid: true)
        }
    };

    /// <summary>
    ///     The seasonal forest biome.
    /// </summary>
    public static readonly Biome SeasonalForest = new(nameof(SeasonalForest))
    {
        Color = Color.LimeGreen,
        Amplitude = 10f,
        Frequency = 0.005f,
        Cover = new Cover(hasPlants: true),
        Layers = new List<Layer>
        {
            Layer.CreateTop(Block.Grass, Block.Dirt, width: 1),
            Layer.CreateSimple(Block.Dirt, width: 5, isSolid: false),
            Layer.CreatePermeableDampen(Block.Dirt, maxWidth: 20),
            Layer.CreateLoose(width: 3),
            Layer.CreateGroundwater(width: 2),
            Layer.CreateSimple(Block.Clay, width: 3, isSolid: true),
            Layer.CreateLoose(width: 37),
            Layer.CreateGroundwater(width: 18),
            Layer.CreateSimple(Block.Clay, width: 21, isSolid: true)
        }
    };

    /// <summary>
    ///     The dry forest biome.
    /// </summary>
    public static readonly Biome DryForest = new(nameof(DryForest))
    {
        Color = Color.SeaGreen,
        Amplitude = 15f,
        Frequency = 0.005f,
        Cover = new Cover(hasPlants: true),
        Layers = new List<Layer>
        {
            Layer.CreateTop(Block.Grass, Block.Dirt, width: 1),
            Layer.CreateSimple(Block.Dirt, width: 3, isSolid: false),
            Layer.CreatePermeableDampen(Block.Dirt, maxWidth: 26),
            Layer.CreateLoose(width: 3),
            Layer.CreateGroundwater(width: 6),
            Layer.CreateSimple(Block.Clay, width: 3, isSolid: true),
            Layer.CreateLoose(width: 33),
            Layer.CreateGroundwater(width: 18),
            Layer.CreateSimple(Block.Clay, width: 21, isSolid: true)
        }
    };

    /// <summary>
    ///     The shrubland biome.
    /// </summary>
    public static readonly Biome Shrubland = new(nameof(Shrubland))
    {
        Color = Color.Salmon,
        Amplitude = 1f,
        Frequency = 0.01f,
        Cover = new Cover(hasPlants: true),
        Layers = new List<Layer>
        {
            Layer.CreateTop(Block.Grass, Block.Dirt, width: 1),
            Layer.CreateSimple(Block.Dirt, width: 7, isSolid: false),
            Layer.CreatePermeableDampen(Block.Dirt, maxWidth: 2),
            Layer.CreateLoose(width: 3),
            Layer.CreateGroundwater(width: 2),
            Layer.CreateSimple(Block.Clay, width: 3, isSolid: true),
            Layer.CreateLoose(width: 37),
            Layer.CreateGroundwater(width: 18),
            Layer.CreateSimple(Block.Clay, width: 21, isSolid: true)
        }
    };

    /// <summary>
    ///     The desert biome.
    /// </summary>
    public static readonly Biome Desert = new(nameof(Desert))
    {
        Color = Color.Yellow,
        Amplitude = 4f,
        Frequency = 0.008f,
        Cover = new Cover(hasPlants: false),
        Layers = new List<Layer>
        {
            Layer.CreateSimple(Block.Sand, width: 9, isSolid: false),
            Layer.CreateSimple(Block.Dirt, width: 4, isSolid: false),
            Layer.CreatePermeableDampen(Block.Dirt, maxWidth: 8),
            Layer.CreateSimple(Block.Sandstone, width: 18, isSolid: true),
            Layer.CreateLoose(width: 22),
            Layer.CreateGroundwater(width: 18),
            Layer.CreateSimple(Block.Clay, width: 21, isSolid: true)
        }
    };

    /// <summary>
    ///     The grassland biome.
    /// </summary>
    public static readonly Biome Grassland = new(nameof(Grassland))
    {
        Color = Color.SaddleBrown,
        Amplitude = 4f,
        Frequency = 0.004f,
        Cover = new Cover(hasPlants: true),
        Layers = new List<Layer>
        {
            Layer.CreateTop(Block.Grass, Block.Dirt, width: 1),
            Layer.CreateSimple(Block.Dirt, width: 7, isSolid: false),
            Layer.CreatePermeableDampen(Block.Dirt, maxWidth: 8),
            Layer.CreateLoose(width: 3),
            Layer.CreateGroundwater(width: 2),
            Layer.CreateSimple(Block.Clay, width: 3, isSolid: true),
            Layer.CreateLoose(width: 37),
            Layer.CreateGroundwater(width: 18),
            Layer.CreateSimple(Block.Clay, width: 21, isSolid: true)
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
        Cover = new Cover(hasPlants: false),
        Layers = new List<Layer>
        {
            Layer.CreateSimple(Block.Sand, width: 5, isSolid: false),
            Layer.CreateSimple(Block.Gravel, width: 3, isSolid: false),
            Layer.CreatePermeableDampen(Block.Gravel, maxWidth: 10),
            Layer.CreateSimple(Block.Limestone, width: 26, isSolid: true),
            Layer.CreateLoose(width: 37),
            Layer.CreateSimple(Block.Limestone, width: 21, isSolid: true)
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
        Cover = new Cover(hasPlants: false),
        Layers = new List<Layer>
        {
            Layer.CreateSimple(Block.Sand, width: 5, isSolid: false),
            Layer.CreateSimple(Block.Gravel, width: 3, isSolid: false),
            Layer.CreatePermeableDampen(Block.Gravel, maxWidth: 10),
            Layer.CreateSimple(Block.Limestone, width: 26, isSolid: true),
            Layer.CreateLoose(width: 37),
            Layer.CreateSimple(Block.Limestone, width: 21, isSolid: true)
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
        Cover = new Cover(hasPlants: false),
        Layers = new List<Layer>
        {
            Layer.CreateStonyTop(width: 9, amplitude: 15),
            Layer.CreateStonyDampen(maxWidth: 31),
            Layer.CreateStone(width: 31),
            Layer.CreateLoose(width: 9),
            Layer.CreateGroundwater(width: 1),
            Layer.CreateSimple(Block.Clay, width: 9, isSolid: true)
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
        Cover = new Cover(hasPlants: false),
        Layers = new List<Layer>
        {
            Layer.CreateSimple(Block.Sand, width: 5, isSolid: false),
            Layer.CreateSimple(Block.Gravel, width: 3, isSolid: false),
            Layer.CreatePermeableDampen(Block.Gravel, maxWidth: 10),
            Layer.CreateSimple(Block.Limestone, width: 13, isSolid: true),
            Layer.CreateLoose(width: 22),
            Layer.CreateGroundwater(width: 18),
            Layer.CreateSimple(Block.Clay, width: 21, isSolid: true)
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
        Cover = new Cover(hasPlants: true),
        Layers = new List<Layer>
        {
            Layer.CreateTop(Block.Grass, Block.Dirt, width: 1),
            Layer.CreateSimple(Block.Limestone, width: 53, isSolid: true),
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
        Cover = new Cover(hasPlants: false),
        Layers = new List<Layer>
        {
            Layer.CreateTop(Block.Grass, Block.Dirt, width: 1),
            Layer.CreateSimple(Block.Limestone, width: 53, isSolid: true),
            Layer.CreateStonyDampen(maxWidth: 28),
            Layer.CreateStone(width: 39)
        }
    };
}
