// <copyright file="BiomeDefinitions.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using System.Drawing;
using VoxelGame.Core.Generation.Default.Deco;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Generation.Default;

/// <summary>
///     Defines all biomes.
/// </summary>
public class Biomes
{
    private readonly List<Biome> biomes = new();

    private Biomes()
    {
        biomes.Add(PolarDesert);
        biomes.Add(TropicalRainforest);
        biomes.Add(TemperateRainforest);
        biomes.Add(Taiga);
        biomes.Add(Tundra);
        biomes.Add(Savanna);
        biomes.Add(SeasonalForest);
        biomes.Add(DryForest);
        biomes.Add(Shrubland);
        biomes.Add(Desert);
        biomes.Add(Grassland);
        biomes.Add(Ocean);
        biomes.Add(PolarOcean);
        biomes.Add(Mountains);
        biomes.Add(Beach);
        biomes.Add(GrassyCliff);
        biomes.Add(SandyCliff);
    }

    /// <summary>
    ///     The polar desert biome.
    /// </summary>
    public Biome PolarDesert { get; } = new(nameof(PolarDesert))
    {
        Color = Color.Gray,
        Amplitude = 2f,
        Frequency = 0.007f,
        Cover = new Cover(hasPlants: false),
        Layers = new List<Layer>
        {
            Layer.CreateSnow(width: 3),
            Layer.CreateSimple(Blocks.Instance.Dirt, width: 5, isSolid: false),
            Layer.CreatePermeableDampen(Blocks.Instance.Dirt, maxWidth: 4),
            Layer.CreateSimple(Blocks.Instance.Permafrost, width: 27, isSolid: true),
            Layer.CreateLoose(width: 27),
            Layer.CreateGroundwater(width: 8),
            Layer.CreateSimple(Blocks.Instance.Clay, width: 21, isSolid: true)
        }
    };

    /// <summary>
    ///     The tropical rainforest biome.
    /// </summary>
    public Biome TropicalRainforest { get; } = new(nameof(TropicalRainforest))
    {
        Color = Color.DarkGreen,
        Amplitude = 15f,
        Frequency = 0.005f,
        Cover = new Cover(hasPlants: true),
        Layers = new List<Layer>
        {
            Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
            Layer.CreateSimple(Blocks.Instance.Dirt, width: 3, isSolid: false),
            Layer.CreatePermeableDampen(Blocks.Instance.Dirt, maxWidth: 26),
            Layer.CreateLoose(width: 3),
            Layer.CreateGroundwater(width: 6),
            Layer.CreateSimple(Blocks.Instance.Clay, width: 3, isSolid: true),
            Layer.CreateLoose(width: 33),
            Layer.CreateGroundwater(width: 18),
            Layer.CreateSimple(Blocks.Instance.Clay, width: 21, isSolid: true)
        },
        Decorations = new List<Decoration>
        {
            Decorations.Instance.TallGrass,
            Decorations.Instance.TallFlower,
            Decorations.Instance.NormalTree,
            Decorations.Instance.TropicalTree,
            Decorations.Instance.Roots,
            Decorations.Instance.Vines
        },
        Structure = Structures.Instance.LargeTropicalTree
    };

    /// <summary>
    ///     The temperate rainforest biome.
    /// </summary>
    public Biome TemperateRainforest { get; } = new(nameof(TemperateRainforest))
    {
        Color = Color.Green,
        Amplitude = 15f,
        Frequency = 0.005f,
        Cover = new Cover(hasPlants: true),
        Layers = new List<Layer>
        {
            Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
            Layer.CreateSimple(Blocks.Instance.Dirt, width: 3, isSolid: false),
            Layer.CreatePermeableDampen(Blocks.Instance.Dirt, maxWidth: 26),
            Layer.CreateLoose(width: 3),
            Layer.CreateGroundwater(width: 6),
            Layer.CreateSimple(Blocks.Instance.Clay, width: 3, isSolid: true),
            Layer.CreateLoose(width: 33),
            Layer.CreateGroundwater(width: 18),
            Layer.CreateSimple(Blocks.Instance.Clay, width: 21, isSolid: true)
        },
        Decorations = new List<Decoration>
        {
            Decorations.Instance.TallGrass,
            Decorations.Instance.TallFlower,
            Decorations.Instance.NormalTree,
            Decorations.Instance.Roots
        }
    };

    /// <summary>
    ///     The taiga biome.
    /// </summary>
    public Biome Taiga { get; } = new(nameof(Taiga))
    {
        Color = Color.Navy,
        Amplitude = 3f,
        Frequency = 0.007f,
        Cover = new Cover(hasPlants: true),
        Layers = new List<Layer>
        {
            Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
            Layer.CreateSimple(Blocks.Instance.Dirt, width: 7, isSolid: false),
            Layer.CreatePermeableDampen(Blocks.Instance.Dirt, maxWidth: 6),
            Layer.CreateSimple(Blocks.Instance.Permafrost, width: 28, isSolid: true),
            Layer.CreateLoose(width: 27),
            Layer.CreateGroundwater(width: 8),
            Layer.CreateSimple(Blocks.Instance.Clay, width: 21, isSolid: true)
        },
        Decorations = new List<Decoration>
        {
            Decorations.Instance.TallGrass,
            Decorations.Instance.NeedleTree
        }
    };

    /// <summary>
    ///     The tundra biome.
    /// </summary>
    public Biome Tundra { get; } = new(nameof(Tundra))
    {
        Color = Color.CadetBlue,
        Amplitude = 3f,
        Frequency = 0.007f,
        Cover = new Cover(hasPlants: true),
        Layers = new List<Layer>
        {
            Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
            Layer.CreateSimple(Blocks.Instance.Dirt, width: 7, isSolid: false),
            Layer.CreatePermeableDampen(Blocks.Instance.Dirt, maxWidth: 6),
            Layer.CreateSimple(Blocks.Instance.Permafrost, width: 28, isSolid: true),
            Layer.CreateLoose(width: 27),
            Layer.CreateGroundwater(width: 8),
            Layer.CreateSimple(Blocks.Instance.Clay, width: 21, isSolid: true)
        },
        Structure = Structures.Instance.BuriedTower
    };

    /// <summary>
    ///     The savanna biome.
    /// </summary>
    public Biome Savanna { get; } = new(nameof(Savanna))
    {
        Color = Color.Olive,
        Amplitude = 1f,
        Frequency = 0.01f,
        Cover = new Cover(hasPlants: true),
        Layers = new List<Layer>
        {
            Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
            Layer.CreateSimple(Blocks.Instance.Dirt, width: 7, isSolid: false),
            Layer.CreatePermeableDampen(Blocks.Instance.Dirt, maxWidth: 2),
            Layer.CreateLoose(width: 3),
            Layer.CreateGroundwater(width: 2),
            Layer.CreateSimple(Blocks.Instance.Clay, width: 3, isSolid: true),
            Layer.CreateLoose(width: 37),
            Layer.CreateGroundwater(width: 18),
            Layer.CreateSimple(Blocks.Instance.Clay, width: 21, isSolid: true)
        },
        Decorations = new List<Decoration>
        {
            Decorations.Instance.TallGrass,
            Decorations.Instance.SavannaTree
        }
    };

    /// <summary>
    ///     The seasonal forest biome.
    /// </summary>
    public Biome SeasonalForest { get; } = new(nameof(SeasonalForest))
    {
        Color = Color.LimeGreen,
        Amplitude = 10f,
        Frequency = 0.005f,
        Cover = new Cover(hasPlants: true),
        Layers = new List<Layer>
        {
            Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
            Layer.CreateSimple(Blocks.Instance.Dirt, width: 5, isSolid: false),
            Layer.CreatePermeableDampen(Blocks.Instance.Dirt, maxWidth: 20),
            Layer.CreateLoose(width: 3),
            Layer.CreateGroundwater(width: 2),
            Layer.CreateSimple(Blocks.Instance.Clay, width: 3, isSolid: true),
            Layer.CreateLoose(width: 37),
            Layer.CreateGroundwater(width: 18),
            Layer.CreateSimple(Blocks.Instance.Clay, width: 21, isSolid: true)
        },
        Decorations = new List<Decoration>
        {
            Decorations.Instance.TallGrass,
            Decorations.Instance.TallFlower,
            Decorations.Instance.NormalTree,
            Decorations.Instance.NormalTree2,
            Decorations.Instance.Roots
        }
    };

    /// <summary>
    ///     The dry forest biome.
    /// </summary>
    public Biome DryForest { get; } = new(nameof(DryForest))
    {
        Color = Color.SeaGreen,
        Amplitude = 15f,
        Frequency = 0.005f,
        Cover = new Cover(hasPlants: true),
        Layers = new List<Layer>
        {
            Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
            Layer.CreateSimple(Blocks.Instance.Dirt, width: 3, isSolid: false),
            Layer.CreatePermeableDampen(Blocks.Instance.Dirt, maxWidth: 26),
            Layer.CreateLoose(width: 3),
            Layer.CreateGroundwater(width: 6),
            Layer.CreateSimple(Blocks.Instance.Clay, width: 3, isSolid: true),
            Layer.CreateLoose(width: 33),
            Layer.CreateGroundwater(width: 18),
            Layer.CreateSimple(Blocks.Instance.Clay, width: 21, isSolid: true)
        },
        Decorations = new List<Decoration>
        {
            Decorations.Instance.TallGrass,
            Decorations.Instance.NormalTree,
            Decorations.Instance.Shrub,
            Decorations.Instance.Roots
        }
    };

    /// <summary>
    ///     The shrubland biome.
    /// </summary>
    public Biome Shrubland { get; } = new(nameof(Shrubland))
    {
        Color = Color.Salmon,
        Amplitude = 1f,
        Frequency = 0.01f,
        Cover = new Cover(hasPlants: true),
        Layers = new List<Layer>
        {
            Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
            Layer.CreateSimple(Blocks.Instance.Dirt, width: 7, isSolid: false),
            Layer.CreatePermeableDampen(Blocks.Instance.Dirt, maxWidth: 2),
            Layer.CreateLoose(width: 3),
            Layer.CreateGroundwater(width: 2),
            Layer.CreateSimple(Blocks.Instance.Clay, width: 3, isSolid: true),
            Layer.CreateLoose(width: 37),
            Layer.CreateGroundwater(width: 18),
            Layer.CreateSimple(Blocks.Instance.Clay, width: 21, isSolid: true)
        },
        Decorations = new List<Decoration>
        {
            Decorations.Instance.TallGrass,
            Decorations.Instance.Boulder,
            Decorations.Instance.Shrub
        }
    };

    /// <summary>
    ///     The desert biome.
    /// </summary>
    public Biome Desert { get; } = new(nameof(Desert))
    {
        Color = Color.Yellow,
        Amplitude = 4f,
        Frequency = 0.008f,
        Cover = new Cover(hasPlants: false),
        Layers = new List<Layer>
        {
            Layer.CreateSimple(Blocks.Instance.Sand, width: 9, isSolid: false),
            Layer.CreateSimple(Blocks.Instance.Dirt, width: 4, isSolid: false),
            Layer.CreatePermeableDampen(Blocks.Instance.Dirt, maxWidth: 8),
            Layer.CreateSimple(Blocks.Instance.Sandstone, width: 18, isSolid: true),
            Layer.CreateLoose(width: 22),
            Layer.CreateGroundwater(width: 18),
            Layer.CreateSimple(Blocks.Instance.Clay, width: 21, isSolid: true)
        },
        Decorations = new List<Decoration>
        {
            Decorations.Instance.Cactus
        },
        Structure = Structures.Instance.SmallPyramid
    };

    /// <summary>
    ///     The grassland biome.
    /// </summary>
    public Biome Grassland { get; } = new(nameof(Grassland))
    {
        Color = Color.SaddleBrown,
        Amplitude = 4f,
        Frequency = 0.004f,
        Cover = new Cover(hasPlants: true),
        Layers = new List<Layer>
        {
            Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
            Layer.CreateSimple(Blocks.Instance.Dirt, width: 7, isSolid: false),
            Layer.CreatePermeableDampen(Blocks.Instance.Dirt, maxWidth: 8),
            Layer.CreateLoose(width: 3),
            Layer.CreateGroundwater(width: 2),
            Layer.CreateSimple(Blocks.Instance.Clay, width: 3, isSolid: true),
            Layer.CreateLoose(width: 37),
            Layer.CreateGroundwater(width: 18),
            Layer.CreateSimple(Blocks.Instance.Clay, width: 21, isSolid: true)
        },
        Decorations = new List<Decoration>
        {
            Decorations.Instance.TallGrass,
            Decorations.Instance.Boulder
        },
        Structure = Structures.Instance.OldTower
    };

    /// <summary>
    ///     The normal ocean biome.
    /// </summary>
    public Biome Ocean { get; } = new(nameof(Ocean))
    {
        Color = Color.White,
        Amplitude = 5.0f,
        Frequency = 0.005f,
        Cover = new Cover(hasPlants: false),
        Layers = new List<Layer>
        {
            Layer.CreateSimple(Blocks.Instance.Sand, width: 5, isSolid: false),
            Layer.CreateSimple(Blocks.Instance.Gravel, width: 3, isSolid: false),
            Layer.CreatePermeableDampen(Blocks.Instance.Gravel, maxWidth: 10),
            Layer.CreateSimple(Blocks.Instance.Limestone, width: 26, isSolid: true),
            Layer.CreateLoose(width: 37),
            Layer.CreateSimple(Blocks.Instance.Limestone, width: 21, isSolid: true)
        }
    };

    /// <summary>
    ///     The polar ocean biome. It is covered in ice and occurs in cold regions.
    /// </summary>
    public Biome PolarOcean { get; } = new(nameof(PolarOcean))
    {
        Color = Color.White,
        Amplitude = 5.0f,
        Frequency = 0.005f,
        IceWidth = 6,
        Cover = new Cover(hasPlants: false),
        Layers = new List<Layer>
        {
            Layer.CreateSimple(Blocks.Instance.Sand, width: 5, isSolid: false),
            Layer.CreateSimple(Blocks.Instance.Gravel, width: 3, isSolid: false),
            Layer.CreatePermeableDampen(Blocks.Instance.Gravel, maxWidth: 10),
            Layer.CreateSimple(Blocks.Instance.Limestone, width: 26, isSolid: true),
            Layer.CreateLoose(width: 37),
            Layer.CreateSimple(Blocks.Instance.Limestone, width: 21, isSolid: true)
        }
    };

    /// <summary>
    ///     The mountain biome. It is a special biome that depends on the height of the terrain.
    /// </summary>
    public Biome Mountains { get; } = new(nameof(Mountains))
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
            Layer.CreateSimple(Blocks.Instance.Clay, width: 9, isSolid: true)
        }
    };

    /// <summary>
    ///     The beach biome. It is found at low heights next to coastlines.
    /// </summary>
    public Biome Beach { get; } = new(nameof(Beach))
    {
        Color = Color.Black,
        Amplitude = 4f,
        Frequency = 0.008f,
        Cover = new Cover(hasPlants: false),
        Layers = new List<Layer>
        {
            Layer.CreateSimple(Blocks.Instance.Sand, width: 5, isSolid: false),
            Layer.CreateSimple(Blocks.Instance.Gravel, width: 3, isSolid: false),
            Layer.CreatePermeableDampen(Blocks.Instance.Gravel, maxWidth: 10),
            Layer.CreateSimple(Blocks.Instance.Limestone, width: 13, isSolid: true),
            Layer.CreateLoose(width: 22),
            Layer.CreateGroundwater(width: 18),
            Layer.CreateSimple(Blocks.Instance.Clay, width: 21, isSolid: true)
        },
        Decorations = new List<Decoration>
        {
            Decorations.Instance.PalmTree
        }
    };

    /// <summary>
    ///     The grass covered cliff biome, which is found at coastlines with large height differences.
    /// </summary>
    public Biome GrassyCliff { get; } = new(nameof(GrassyCliff))
    {
        Color = Color.Black,
        Amplitude = 4f,
        Frequency = 0.008f,
        Cover = new Cover(hasPlants: true),
        Layers = new List<Layer>
        {
            Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
            Layer.CreateSimple(Blocks.Instance.Limestone, width: 53, isSolid: true),
            Layer.CreateStonyDampen(maxWidth: 28),
            Layer.CreateStone(width: 39)
        }
    };

    /// <summary>
    ///     The sand covered cliff biome, which is found at coastlines with large height differences.
    /// </summary>
    public Biome SandyCliff { get; } = new(nameof(SandyCliff))
    {
        Color = Color.Black,
        Amplitude = 4f,
        Frequency = 0.008f,
        Cover = new Cover(hasPlants: false),
        Layers = new List<Layer>
        {
            Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
            Layer.CreateSimple(Blocks.Instance.Limestone, width: 53, isSolid: true),
            Layer.CreateStonyDampen(maxWidth: 28),
            Layer.CreateStone(width: 39)
        }
    };

    /// <summary>
    ///     Load all biomes. They must be set up after that.
    /// </summary>
    /// <returns>The loaded biomes.</returns>
    public static Biomes Load()
    {
        return new Biomes();
    }

    /// <summary>
    ///     Sets up all biomes.
    /// </summary>
    /// <param name="factory">The noise generator factory.</param>
    /// <param name="palette">The palette to use for biome generation.</param>
    public void Setup(NoiseFactory factory, Palette palette)
    {
        foreach (Biome biome in biomes) biome.SetupBiome(factory, palette);
    }
}
