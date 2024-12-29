// <copyright file="BiomeLoader.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Drawing;
using VoxelGame.Core.Generation.Worlds.Default.Decorations;
using VoxelGame.Core.Generation.Worlds.Default.Palettes;
using VoxelGame.Core.Generation.Worlds.Default.Structures;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Toolkit.Collections;

namespace VoxelGame.Core.Generation.Worlds.Default.Biomes;

/// <summary>
/// Loads all biomes for this world generator, as well as the biome distribution.
/// </summary>
public sealed class BiomeLoader : IResourceLoader
{
    String? ICatalogEntry.Instance => null;

    /// <inheritdoc />
    public IEnumerable<IResource> Load(IResourceContext context) =>
        context.Require<Palette>(palette =>
            context.Require<IDecorationProvider>(decorations =>
                context.Require<IStructureGeneratorDefinitionProvider>(structures =>
                {
                    Registry<BiomeDefinition> registry = new(biome => biome.Name);
                    Biomes biomes = new(registry, palette, decorations, structures);

                    // The distribution here is like a diagram:
                    // On the x-axis, going right, the temperature increases.
                    // On the y-axis, going down, the humidity increases.

                    BiomeDistributionDefinition distribution = new(new Array2D<BiomeDefinition?>([
                        [biomes.PolarDesert, biomes.Tundra, biomes.Taiga, biomes.Grassland, biomes.Grassland, biomes.Grassland, biomes.Desert, biomes.Desert, biomes.Desert, biomes.Desert],
                        [null, biomes.Tundra, biomes.Taiga, biomes.Shrubland, biomes.Shrubland, biomes.Shrubland, biomes.Shrubland, biomes.Savanna, biomes.Desert, biomes.Desert],
                        [null, null, biomes.Taiga, biomes.SeasonalForest, biomes.SeasonalForest, biomes.SeasonalForest, biomes.Shrubland, biomes.Savanna, biomes.Savanna, biomes.Savanna],
                        [null, null, null, biomes.SeasonalForest, biomes.SeasonalForest, biomes.SeasonalForest, biomes.SeasonalForest, biomes.DryForest, biomes.DryForest, biomes.DryForest],
                        [null, null, null, null, biomes.SeasonalForest, biomes.SeasonalForest, biomes.SeasonalForest, biomes.DryForest, biomes.DryForest, biomes.DryForest],
                        [null, null, null, null, null, biomes.TemperateRainforest, biomes.TemperateRainforest, biomes.TropicalRainforest, biomes.DryForest, biomes.DryForest],
                        [null, null, null, null, null, null, biomes.TemperateRainforest, biomes.TropicalRainforest, biomes.TropicalRainforest, biomes.DryForest],
                        [null, null, null, null, null, null, null, biomes.TropicalRainforest, biomes.TropicalRainforest, biomes.TropicalRainforest],
                        [null, null, null, null, null, null, null, null, biomes.TropicalRainforest, biomes.TropicalRainforest],
                        [null, null, null, null, null, null, null, null, null, biomes.TropicalRainforest]
                    ]))
                    {
                        Beach = biomes.Beach,
                        Mountain = biomes.Mountains,
                        Desert = biomes.Desert,
                        GrassyCliff = biomes.GrassyCliff,
                        SandyCliff = biomes.SandyCliff,
                        Ocean = biomes.Ocean,
                        PolarDesert = biomes.PolarDesert,
                        PolarOcean = biomes.PolarOcean
                    };

                    return [..registry.Values, distribution];
                })));

    private sealed class Biomes(
        Registry<BiomeDefinition> biomes,
        Palette palette,
        IDecorationProvider decorations,
        IStructureGeneratorDefinitionProvider structures)
    {
        private static readonly RID tallGrass = RID.Named<Decoration>("TallGrass");
        private static readonly RID tallFlower = RID.Named<Decoration>("TallFlower");
        private static readonly RID normalTree = RID.Named<Decoration>("NormalTree");
        private static readonly RID normalTree2 = RID.Named<Decoration>("NormalTree2");
        private static readonly RID tropicalTree = RID.Named<Decoration>("TropicalTree");
        private static readonly RID roots = RID.Named<Decoration>("Roots");
        private static readonly RID vines = RID.Named<Decoration>("Vines");
        private static readonly RID needleTree = RID.Named<Decoration>("NeedleTree");
        private static readonly RID savannaTree = RID.Named<Decoration>("SavannaTree");
        private static readonly RID shrub = RID.Named<Decoration>("Shrub");
        private static readonly RID boulder = RID.Named<Decoration>("Boulder");
        private static readonly RID cactus = RID.Named<Decoration>("Cactus");
        private static readonly RID palmTree = RID.Named<Decoration>("PalmTree");

        /// <summary>
        ///     The polar desert biome.
        /// </summary>
        public BiomeDefinition PolarDesert { get; } = biomes.Register(new BiomeDefinition(nameof(PolarDesert), palette)
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
        });

        /// <summary>
        ///     The tropical rainforest biome.
        /// </summary>
        public BiomeDefinition TropicalRainforest { get; } = biomes.Register(new BiomeDefinition(nameof(TropicalRainforest), palette)
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
                decorations.GetDecoration(tallGrass),
                decorations.GetDecoration(tallFlower),
                decorations.GetDecoration(normalTree),
                decorations.GetDecoration(tropicalTree),
                decorations.GetDecoration(roots),
                decorations.GetDecoration(vines),
            },
            Structure = structures.GetStructure(RID.Named<StructureGeneratorDefinition>("LargeTropicalTree"))
        });

        /// <summary>
        ///     The temperate rainforest biome.
        /// </summary>
        public BiomeDefinition TemperateRainforest { get; } = biomes.Register(new BiomeDefinition(nameof(TemperateRainforest), palette)
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
                decorations.GetDecoration(tallGrass),
                decorations.GetDecoration(tallFlower),
                decorations.GetDecoration(normalTree),
                decorations.GetDecoration(roots),
            }
        });

        /// <summary>
        ///     The taiga biome.
        /// </summary>
        public BiomeDefinition Taiga { get; } = biomes.Register(new BiomeDefinition(nameof(Taiga), palette)
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
                decorations.GetDecoration(tallGrass),
                decorations.GetDecoration(needleTree),
            }
        });

        /// <summary>
        ///     The tundra biome.
        /// </summary>
        public BiomeDefinition Tundra { get; } = biomes.Register(new BiomeDefinition(nameof(Tundra), palette)
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
            Structure = structures.GetStructure(RID.Named<StructureGeneratorDefinition>("BuriedTower"))
        });

        /// <summary>
        ///     The savanna biome.
        /// </summary>
        public BiomeDefinition Savanna { get; } = biomes.Register(new BiomeDefinition(nameof(Savanna), palette)
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
                decorations.GetDecoration(tallGrass),
                decorations.GetDecoration(savannaTree),
            }
        });

        /// <summary>
        ///     The seasonal forest biome.
        /// </summary>
        public BiomeDefinition SeasonalForest { get; } = biomes.Register(new BiomeDefinition(nameof(SeasonalForest), palette)
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
                decorations.GetDecoration(tallGrass),
                decorations.GetDecoration(tallFlower),
                decorations.GetDecoration(normalTree),
                decorations.GetDecoration(normalTree2),
                decorations.GetDecoration(roots),
            }
        });

        /// <summary>
        ///     The dry forest biome.
        /// </summary>
        public BiomeDefinition DryForest { get; } = biomes.Register(new BiomeDefinition(nameof(DryForest), palette)
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
                decorations.GetDecoration(tallGrass),
                decorations.GetDecoration(normalTree),
                decorations.GetDecoration(shrub),
                decorations.GetDecoration(roots),
            }
        });

        /// <summary>
        ///     The shrubland biome.
        /// </summary>
        public BiomeDefinition Shrubland { get; } = biomes.Register(new BiomeDefinition(nameof(Shrubland), palette)
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
                decorations.GetDecoration(tallGrass),
                decorations.GetDecoration(boulder),
                decorations.GetDecoration(shrub),
            }
        });

        /// <summary>
        ///     The desert biome.
        /// </summary>
        public BiomeDefinition Desert { get; } = biomes.Register(new BiomeDefinition(nameof(Desert), palette)
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
                decorations.GetDecoration(cactus),
            },
            Structure = structures.GetStructure(RID.Named<StructureGeneratorDefinition>("SmallPyramid"))
        });

        /// <summary>
        ///     The grassland biome.
        /// </summary>
        public BiomeDefinition Grassland { get; } = biomes.Register(new BiomeDefinition(nameof(Grassland), palette)
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
                decorations.GetDecoration(tallGrass),
                decorations.GetDecoration(boulder),
            },
            Structure = structures.GetStructure(RID.Named<StructureGeneratorDefinition>("OldTower"))
        });

        /// <summary>
        ///     The normal ocean biome.
        /// </summary>
        public BiomeDefinition Ocean { get; } = biomes.Register(new BiomeDefinition(nameof(Ocean), palette)
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
        });

        /// <summary>
        ///     The polar ocean biome. It is covered in ice and occurs in cold regions.
        /// </summary>
        public BiomeDefinition PolarOcean { get; } = biomes.Register(new BiomeDefinition(nameof(PolarOcean), palette)
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
        });

        /// <summary>
        ///     The mountain biome. It is a special biome that depends on the height of the terrain.
        /// </summary>
        public BiomeDefinition Mountains { get; } = biomes.Register(new BiomeDefinition(nameof(Mountains), palette)
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
        });

        /// <summary>
        ///     The beach biome. It is found at low heights next to coastlines.
        /// </summary>
        public BiomeDefinition Beach { get; } = biomes.Register(new BiomeDefinition(nameof(Beach), palette)
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
                decorations.GetDecoration(palmTree),
            }
        });

        /// <summary>
        ///     The grass covered cliff biome, which is found at coastlines with large height differences.
        /// </summary>
        public BiomeDefinition GrassyCliff { get; } = biomes.Register(new BiomeDefinition(nameof(GrassyCliff), palette)
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
        });

        /// <summary>
        ///     The sand covered cliff biome, which is found at coastlines with large height differences.
        /// </summary>
        public BiomeDefinition SandyCliff { get; } = biomes.Register(new BiomeDefinition(nameof(SandyCliff), palette)
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
        });
    }
}
