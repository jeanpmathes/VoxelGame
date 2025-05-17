// <copyright file="SubBiomeLoader.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using VoxelGame.Core.Generation.Worlds.Default.Decorations;
using VoxelGame.Core.Generation.Worlds.Default.Palettes;
using VoxelGame.Core.Generation.Worlds.Default.Structures;
using VoxelGame.Core.Logic.Definitions.Blocks.Conventions;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Generation.Worlds.Default.SubBiomes;

/// <summary>
///     Loads all sub-biomes.
/// </summary>
public class SubBiomeLoader : IResourceLoader
{
    String? ICatalogEntry.Instance => null;

    /// <inheritdoc />
    public IEnumerable<IResource> Load(IResourceContext context)
    {
        return context.Require<Palette>(palette =>
            context.Require<IDecorationProvider>(decorations =>
                context.Require<IStructureGeneratorDefinitionProvider>(structures =>
                {
                    Registry<SubBiomeDefinition> registry = new(subBiomeDefinition => subBiomeDefinition.Name);
                    SubBiomes subBiomes = new(registry, palette, decorations, structures);

                    return [..subBiomes.Registry.Values];
                })));
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class SubBiomes(
        Registry<SubBiomeDefinition> subBiomes,
        Palette palette,
        IDecorationProvider decorations,
        IStructureGeneratorDefinitionProvider structures)
    {
        private static readonly RID tallGrass = RID.Named<Decoration>("TallGrass");
        private static readonly RID tallRedFlower = RID.Named<Decoration>("TallRedFlower");
        private static readonly RID tallYellowFlower = RID.Named<Decoration>("TallYellowFlower");
        private static readonly RID roots = RID.Named<Decoration>("Roots");
        private static readonly RID vines = RID.Named<Decoration>("Vines");
        private static readonly RID boulder = RID.Named<Decoration>("Boulder");
        private static readonly RID termiteMound = RID.Named<Decoration>("TermiteMound");
        private static readonly RID cactus = RID.Named<Decoration>("Cactus");

        public Registry<SubBiomeDefinition> Registry => subBiomes;

        private static IEnumerable<Layer> Permafrost =>
        [
            Layer.CreateSimple(Blocks.Instance.Permafrost, width: 27, isSolid: true),
            Layer.CreateLoose(width: 27),
            Layer.CreateGroundwater(width: 8),
            Layer.CreateSimple(Blocks.Instance.Clay, width: 21, isSolid: true)
        ];

        private static IEnumerable<Layer> SingleGroundwaterStone =>
        [
            Layer.CreateStone(width: 27),
            Layer.CreateLoose(width: 27),
            Layer.CreateGroundwater(width: 8),
            Layer.CreateStone(width: 21)
        ];

        private static IEnumerable<Layer> Clay =>
        [
            Layer.CreateLoose(width: 3),
            Layer.CreateGroundwater(width: 6),
            Layer.CreateSimple(Blocks.Instance.Clay, width: 3, isSolid: true),
            Layer.CreateLoose(width: 33),
            Layer.CreateGroundwater(width: 18),
            Layer.CreateSimple(Blocks.Instance.Clay, width: 21, isSolid: true)
        ];

        private static IEnumerable<Layer> DoubleGroundwaterStone =>
        [
            Layer.CreateLoose(width: 3),
            Layer.CreateGroundwater(width: 6),
            Layer.CreateStone(width: 3),
            Layer.CreateLoose(width: 33),
            Layer.CreateGroundwater(width: 18),
            Layer.CreateStone(width: 21)
        ];

        #region Ocean

        /// <summary>
        ///     The normal ocean sub-biome.
        /// </summary>
        public SubBiomeDefinition Ocean { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(Ocean), palette)
        {
            Amplitude = 5.0f,
            Frequency = 0.005f,
            Cover = new Cover.NoVegetation(),
            Layers = (List<Layer>)
            [
                Layer.CreateSimple(Blocks.Instance.Sand, width: 5, isSolid: false),
                Layer.CreateSimple(Blocks.Instance.Gravel, width: 3, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Gravel, maxWidth: 10),
                Layer.CreateSimple(Blocks.Instance.Limestone, width: 26, isSolid: true),
                Layer.CreateLoose(width: 37),
                Layer.CreateSimple(Blocks.Instance.Limestone, width: 21, isSolid: true)
            ]
        });

        #endregion

        #region PolarOcean

        /// <summary>
        ///     The polar ocean sub-biome. It is covered in ice and occurs in cold regions.
        /// </summary>
        public SubBiomeDefinition PolarOcean { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(PolarOcean), palette)
        {
            Amplitude = 5.0f,
            Frequency = 0.005f,
            IceWidth = 6,
            Cover = new Cover.NoVegetation(),
            Layers = (List<Layer>)
            [
                Layer.CreateSimple(Blocks.Instance.Sand, width: 5, isSolid: false),
                Layer.CreateSimple(Blocks.Instance.Gravel, width: 3, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Gravel, maxWidth: 10),
                Layer.CreateSimple(Blocks.Instance.Limestone, width: 26, isSolid: true),
                Layer.CreateLoose(width: 37),
                Layer.CreateSimple(Blocks.Instance.Limestone, width: 21, isSolid: true)
            ]
        });

        #endregion PolarOcean

        #pragma warning disable S3242 // Types have meaning.
        private static RID Get(Wood wood)
        {
            return RID.Named<Decoration>(wood.NamedID);
        }
        #pragma warning restore S3242

        #region PolarDesert

        /// <summary>
        ///     A snow-filled flat field.
        /// </summary>
        public SubBiomeDefinition PolarDesertSnowy { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(PolarDesertSnowy), palette)
        {
            Amplitude = 2f,
            Frequency = 0.004f,
            Cover = new Cover.NoVegetation(),
            Layers =
            [
                Layer.CreateSnow(width: 3, loose: false),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 5, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 4),
                .. Permafrost
            ]
        });

        /// <summary>
        ///     Flat fields with loose snow, where one can sink into the snow.
        /// </summary>
        public SubBiomeDefinition PolarDesertLooseSnow { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(PolarDesertLooseSnow), palette)
        {
            Amplitude = 4f,
            Frequency = 0.03f,
            Offset = -3,
            Cover = new Cover.NoVegetation(isSnowLoose: true),
            Stuffer = new Stuffer.Ice(),
            Layers =
            [
                Layer.CreateSnow(width: 3, loose: true),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 5, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 9),
                .. Permafrost
            ]
        });

        /// <summary>
        ///     Snowy dunes.
        /// </summary>
        public SubBiomeDefinition PolarDesertDunes { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(PolarDesertDunes), palette)
        {
            Amplitude = 4f,
            Frequency = 0.01f,
            Cover = new Cover.NoVegetation(),
            Layers =
            [
                Layer.CreateSnow(width: 3, loose: false),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 5, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 4),
                .. Permafrost
            ]
        });

        /// <summary>
        ///     A frosty, stony area.
        /// </summary>
        public SubBiomeDefinition PolarDesertRidge { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(PolarDesertRidge), palette)
        {
            Amplitude = 5f,
            Frequency = 0.09f,
            Cover = new Cover.NoVegetation(),
            Layers =
            [
                Layer.CreateStonyTop(width: 5),
                Layer.CreateStonyDampen(maxWidth: 9),
                .. SingleGroundwaterStone
            ]
        });

        /// <summary>
        ///     Frozen water basins.
        /// </summary>
        public SubBiomeDefinition PolarDesertBasin { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(PolarDesertBasin), palette)
        {
            Amplitude = 4f,
            Frequency = 0.03f,
            Offset = -3,
            Cover = new Cover.NoVegetation(),
            Stuffer = new Stuffer.Ice(),
            Layers =
            [
                Layer.CreateStonyTop(width: 5),
                Layer.CreateStonyDampen(maxWidth: 9),
                .. SingleGroundwaterStone
            ]
        });

        #endregion PolarDesert

        #region TropicalRainforest

        /// <summary>
        ///     Hilly sub-biome of a rainforest.
        /// </summary>
        public SubBiomeDefinition TropicalRainforestHills { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(TropicalRainforestHills), palette)
        {
            Amplitude = 15f,
            Frequency = 0.005f,
            Cover = new Cover.GrassAndFlowers(),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 3, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 26),
                .. Clay
            ],
            Decorations =
            [
                (decorations.GetDecoration(tallGrass), 1.0f),
                (decorations.GetDecoration(tallRedFlower), 8.0f),
                (decorations.GetDecoration(tallYellowFlower), 8.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Mahogany)), 3.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Teak)), 3.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Ebony)), 30.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.RubberTree)), 30.0f),
                (decorations.GetDecoration(roots), 1000.0f),
                (decorations.GetDecoration(vines), 1.0f)
            ],
            Structure = structures.GetStructure(RID.Named<StructureGeneratorDefinition>("LargeTropicalTree"))
        });

        /// <summary>
        ///     Flat sub-biome of a rainforest.
        /// </summary>
        public SubBiomeDefinition TropicalRainforestFlats { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(TropicalRainforestFlats), palette)
        {
            Amplitude = 5f,
            Frequency = 0.005f,
            Cover = new Cover.GrassAndFlowers(),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 3, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 26),
                .. Clay
            ],
            Decorations =
            [
                (decorations.GetDecoration(tallGrass), 1.0f),
                (decorations.GetDecoration(tallRedFlower), 8.0f),
                (decorations.GetDecoration(tallYellowFlower), 8.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Mahogany)), 3.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Teak)), 3.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Ebony)), 30.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.RubberTree)), 30.0f),
                (decorations.GetDecoration(roots), 1000.0f),
                (decorations.GetDecoration(vines), 1.0f)
            ],
            Structure = structures.GetStructure(RID.Named<StructureGeneratorDefinition>("LargeTropicalTree"))
        });

        /// <summary>
        ///     A group of rubber trees.
        /// </summary>
        public SubBiomeDefinition TropicalRainforestRubberTrees { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(TropicalRainforestRubberTrees), palette)
        {
            Amplitude = 10f,
            Frequency = 0.005f,
            Cover = new Cover.GrassAndFlowers(),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 3, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 26),
                .. Clay
            ],
            Decorations =
            [
                (decorations.GetDecoration(tallGrass), 1.0f),
                (decorations.GetDecoration(tallRedFlower), 8.0f),
                (decorations.GetDecoration(tallYellowFlower), 8.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.RubberTree)), 3.0f),
                (decorations.GetDecoration(roots), 1000.0f),
                (decorations.GetDecoration(vines), 1.0f)
            ]
        });

        /// <summary>
        ///     A clearing filled with flowers.
        /// </summary>
        public SubBiomeDefinition TropicalRainforestBloomingClearing { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(TropicalRainforestBloomingClearing), palette)
        {
            Amplitude = 5f,
            Frequency = 0.005f,
            Cover = new Cover.GrassAndFlowers(isBlooming: true),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 3, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 26),
                .. Clay
            ],
            Decorations =
            [
                (decorations.GetDecoration(tallGrass), 4.0f),
                (decorations.GetDecoration(tallRedFlower), 4.0f),
                (decorations.GetDecoration(tallYellowFlower), 4.0f)
            ]
        });

        /// <summary>
        ///     A pond sub-biome in a rainforest.
        /// </summary>
        public SubBiomeDefinition TropicalRainforestPond { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(TropicalRainforestPond), palette)
        {
            Amplitude = 10f,
            Frequency = 0.005f,
            Offset = -9,
            Cover = new Cover.GrassAndFlowers(),
            Stuffer = new Stuffer.Water(),
            Layers =
            [
                Layer.CreateMud(width: 4),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 26),
                .. Clay
            ],
            Decorations =
            [
                (decorations.GetDecoration(tallGrass), 1.0f),
                (decorations.GetDecoration(tallRedFlower), 8.0f),
                (decorations.GetDecoration(tallYellowFlower), 8.0f)
            ]
        });

        #endregion TropicalRainforest

        #region TemperateRainforest

        /// <summary>
        ///     A hilly sub-biome of a temperate rainforest.
        /// </summary>
        public SubBiomeDefinition TemperateRainforestHills { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(TemperateRainforestHills), palette)
        {
            Amplitude = 15f,
            Frequency = 0.005f,
            Cover = new Cover.Fern(),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 3, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 26),
                .. Clay
            ],
            Decorations =
            [
                (decorations.GetDecoration(Get(Blocks.Instance.Oak)), 3.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Maple)), 3.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Cherry)), 3.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Walnut)), 3.0f),
                (decorations.GetDecoration(roots), 1000.0f)
            ]
        });

        /// <summary>
        ///     A flat sub-biome of a temperate rainforest.
        /// </summary>
        public SubBiomeDefinition TemperateRainforestFlats { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(TemperateRainforestFlats), palette)
        {
            Amplitude = 5f,
            Frequency = 0.005f,
            Cover = new Cover.Fern(),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 3, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 26),
                .. Clay
            ],
            Decorations =
            [
                (decorations.GetDecoration(Get(Blocks.Instance.Oak)), 3.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Maple)), 3.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Cherry)), 3.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Walnut)), 3.0f),
                (decorations.GetDecoration(roots), 1000.0f)
            ]
        });

        /// <summary>
        ///     A part of a temperate rainforest with cherry trees.
        /// </summary>
        public SubBiomeDefinition TemperateRainforestCherryGrove { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(TemperateRainforestCherryGrove), palette)
        {
            Amplitude = 5f,
            Frequency = 0.005f,
            Cover = new Cover.Fern(),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 3, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 26),
                .. Clay
            ],
            Decorations =
            [
                (decorations.GetDecoration(Get(Blocks.Instance.Cherry)), 2.0f),
                (decorations.GetDecoration(roots), 1000.0f)
            ]
        });

        /// <summary>
        ///     A pond in a temperate rainforest.
        /// </summary>
        public SubBiomeDefinition TemperateRainforestPond { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(TemperateRainforestPond), palette)
        {
            Amplitude = 8f,
            Frequency = 0.01f,
            Offset = -11,
            Cover = new Cover.Moss(),
            Stuffer = new Stuffer.Water(),
            Layers =
            [
                Layer.CreateMud(width: 4),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 26),
                .. Clay
            ]
        });

        /// <summary>
        ///     A stony, moss-covered area.
        /// </summary>
        public SubBiomeDefinition TemperateRainforestMossyStones { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(TemperateRainforestMossyStones), palette)
        {
            Amplitude = 5f,
            Frequency = 0.09f,
            Cover = new Cover.Moss(),
            Layers =
            [
                Layer.CreateStonyTop(width: 4),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 26),
                .. Clay
            ]
        });

        #endregion TemperateRainforest

        #region Taiga

        /// <summary>
        ///     Boreal forest sub-biome.
        /// </summary>
        public SubBiomeDefinition TaigaForest { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(TaigaForest), palette)
        {
            Amplitude = 3f,
            Frequency = 0.007f,
            Cover = new Cover.Moss(),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 7, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 6),
                ..Permafrost
            ],
            Decorations =
            [
                (decorations.GetDecoration(Get(Blocks.Instance.Pine)), 8.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Spruce)), 8.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Fir)), 8.0f)
            ]
        });

        /// <summary>
        ///     Boreal pine forest sub-biome.
        /// </summary>
        public SubBiomeDefinition TaigaPineForest { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(TaigaPineForest), palette)
        {
            Amplitude = 3f,
            Frequency = 0.007f,
            Cover = new Cover.Moss(),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 7, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 6),
                ..Permafrost
            ],
            Decorations =
            [
                (decorations.GetDecoration(Get(Blocks.Instance.Pine)), 40.0f)
            ]
        });

        /// <summary>
        ///     Boreal spruce forest sub-biome.
        /// </summary>
        public SubBiomeDefinition TaigaSpruceForest { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(TaigaSpruceForest), palette)
        {
            Amplitude = 3f,
            Frequency = 0.007f,
            Cover = new Cover.Moss(),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 7, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 6),
                ..Permafrost
            ],
            Decorations =
            [
                (decorations.GetDecoration(Get(Blocks.Instance.Spruce)), 40.0f)
            ]
        });

        /// <summary>
        ///     Boreal fir forest sub-biome.
        /// </summary>
        public SubBiomeDefinition TaigaFirForest { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(TaigaFirForest), palette)
        {
            Amplitude = 3f,
            Frequency = 0.007f,
            Cover = new Cover.Moss(),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 7, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 6),
                ..Permafrost
            ],
            Decorations =
            [
                (decorations.GetDecoration(Get(Blocks.Instance.Fir)), 40.0f)
            ]
        });

        /// <summary>
        ///     A boreal wetland - a swampy area.
        /// </summary>
        public SubBiomeDefinition TaigaWetland { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(TaigaWetland), palette)
        {
            Amplitude = 2f,
            Frequency = 0.03f,
            Offset = -2,
            Cover = new Cover.Moss(),
            Stuffer = new Stuffer.Water(),
            Layers =
            [
                Layer.CreateMud(width: 5),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 9),
                .. Permafrost
            ],
            Decorations =
            [
                (decorations.GetDecoration(Get(Blocks.Instance.Spruce)), 40.0f)
            ]
        });

        /// <summary>
        ///     Boreal shrubland sub-biome.
        /// </summary>
        public SubBiomeDefinition TaigaShrubland { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(TaigaShrubland), palette)
        {
            Amplitude = 3f,
            Frequency = 0.007f,
            Cover = new Cover.Moss(),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 7, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 6),
                ..Permafrost
            ],
            Decorations =
            [
                (decorations.GetDecoration(Get(Blocks.Instance.Juniper)), 100.0f)
            ]
        });

        #endregion Taiga

        #region Tundra

        /// <summary>
        ///     A very cold shrubland.
        /// </summary>
        public SubBiomeDefinition TundraShrubland { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(TundraShrubland), palette)
        {
            Amplitude = 3f,
            Frequency = 0.007f,
            Cover = new Cover.Lichen(Cover.Lichen.Density.Low),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 7, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 6),
                .. Permafrost
            ],
            Decorations =
            [
                (decorations.GetDecoration(Get(Blocks.Instance.Juniper)), 500.0f)
            ],
            Structure = structures.GetStructure(RID.Named<StructureGeneratorDefinition>("BuriedTower"))
        });

        /// <summary>
        ///     Very cold grassland.
        /// </summary>
        public SubBiomeDefinition TundraGrassland { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(TundraGrassland), palette)
        {
            Amplitude = 3f,
            Frequency = 0.007f,
            Cover = new Cover.Lichen(Cover.Lichen.Density.Low),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 7, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 6),
                .. Permafrost
            ],
            Structure = structures.GetStructure(RID.Named<StructureGeneratorDefinition>("BuriedTower"))
        });

        /// <summary>
        ///     A cold, stony area.
        /// </summary>
        public SubBiomeDefinition TundraRidge { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(TundraRidge), palette)
        {
            Amplitude = 5f,
            Frequency = 0.06f,
            Cover = new Cover.Lichen(Cover.Lichen.Density.Low),
            Layers =
            [
                Layer.CreateStonyTop(width: 5),
                Layer.CreateStonyDampen(maxWidth: 9),
                .. SingleGroundwaterStone
            ]
        });

        /// <summary>
        ///     A patch of permafrost directly at the surface.
        /// </summary>
        public SubBiomeDefinition TundraPermafrostPatch { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(TundraPermafrostPatch), palette)
        {
            Amplitude = 1f,
            Frequency = 0.007f,
            Cover = new Cover.NoVegetation(),
            Layers =
            [
                Layer.CreateSimple(Blocks.Instance.Permafrost, width: 8, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Permafrost, maxWidth: 6),
                .. Permafrost
            ]
        });

        /// <summary>
        ///     A basin filled with thawed water - or ice, depending on the temperature.
        /// </summary>
        public SubBiomeDefinition TundraBasin { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(TundraBasin), palette)
        {
            Amplitude = 4f,
            Frequency = 0.03f,
            Offset = -3,
            Cover = new Cover.NoVegetation(),
            Stuffer = new Stuffer.Water(),
            Layers =
            [
                Layer.CreateMud(width: 5),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 9),
                .. Permafrost
            ]
        });

        /// <summary>
        ///     An area covered in lichen.
        /// </summary>
        public SubBiomeDefinition TundraLichen { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(TundraLichen), palette)
        {
            Amplitude = 2f,
            Frequency = 0.007f,
            Cover = new Cover.Lichen(Cover.Lichen.Density.High),
            Layers =
            [
                Layer.CreateStonyTop(width: 5),
                Layer.CreateStonyDampen(maxWidth: 9),
                .. SingleGroundwaterStone
            ]
        });

        #endregion Tundra

        #region Savanna

        /// <summary>
        ///     A woodland sub-biome in a savanna.
        /// </summary>
        public SubBiomeDefinition SavannaWoodland { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(SavannaWoodland), palette)
        {
            Amplitude = 1f,
            Frequency = 0.01f,
            Cover = new Cover.Grass(),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 7, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 2),
                ..Clay
            ],
            Decorations =
            [
                (decorations.GetDecoration(tallGrass), 1.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Acacia)), 50.0f),
                (decorations.GetDecoration(termiteMound), 500.0f)
            ]
        });

        /// <summary>
        ///     A dense woodland sub-biome in a savanna.
        /// </summary>
        public SubBiomeDefinition SavannaDenseWoodland { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(SavannaDenseWoodland), palette)
        {
            Amplitude = 1f,
            Frequency = 0.01f,
            Cover = new Cover.Grass(),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 7, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 2),
                ..Clay
            ],
            Decorations =
            [
                (decorations.GetDecoration(tallGrass), 1.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Acacia)), 40.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Baobab)), 40.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.ShepherdsTree)), 40.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Mesquite)), 40.0f),
                (decorations.GetDecoration(termiteMound), 200.0f)
            ]
        });

        /// <summary>
        ///     A shrubland sub-biome in a savanna.
        /// </summary>
        public SubBiomeDefinition SavannaShrubland { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(SavannaShrubland), palette)
        {
            Amplitude = 1f,
            Frequency = 0.01f,
            Cover = new Cover.Grass(),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 7, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 2),
                ..Clay
            ],
            Decorations =
            [
                (decorations.GetDecoration(tallGrass), 1.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.ShepherdsTree)), 100.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Mesquite)), 100.0f),
                (decorations.GetDecoration(termiteMound), 500.0f)
            ]
        });

        /// <summary>
        ///     A grassland sub-biome in a savanna.
        /// </summary>
        public SubBiomeDefinition SavannaGrassland { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(SavannaGrassland), palette)
        {
            Amplitude = 1f,
            Frequency = 0.01f,
            Cover = new Cover.Grass(),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 7, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 2),
                ..Clay
            ],
            Decorations =
            [
                (decorations.GetDecoration(tallGrass), 1.0f),
                (decorations.GetDecoration(termiteMound), 1000.0f)
            ]
        });

        /// <summary>
        ///     A savanna waterhole.
        /// </summary>
        public SubBiomeDefinition SavannaWaterhole { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(SavannaWaterhole), palette)
        {
            Amplitude = 5f,
            Frequency = 0.03f,
            Offset = -7,
            Stuffer = new Stuffer.Water(),
            Cover = new Cover.NoVegetation(),
            Layers =
            [
                Layer.CreateMud(width: 4),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 20),
                .. Clay
            ]
        });

        #endregion Savanna

        #region SeasonalForest

        /// <summary>
        ///     A woodland sub-biome.
        /// </summary>
        public SubBiomeDefinition SeasonalForestWoodland { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(SeasonalForestWoodland), palette)
        {
            Amplitude = 7f,
            Frequency = 0.005f,
            Cover = new Cover.GrassAndFlowers(mushrooms: true),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 5, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 20),
                .. Clay
            ],
            Decorations =
            [
                (decorations.GetDecoration(tallGrass), 1.0f),
                (decorations.GetDecoration(tallRedFlower), 8.0f),
                (decorations.GetDecoration(tallYellowFlower), 8.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Oak)), 3.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Maple)), 3.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Birch)), 3.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Walnut)), 10.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Cherry)), 3.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.AshTree)), 3.0f),
                (decorations.GetDecoration(roots), 1000.0f)
            ]
        });

        /// <summary>
        ///     A birch grove sub-biome, similar to woodland but with only birch trees.
        /// </summary>
        public SubBiomeDefinition SeasonalForestBirchGrove { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(SeasonalForestBirchGrove), palette)
        {
            Amplitude = 7f,
            Frequency = 0.005f,
            Cover = new Cover.GrassAndFlowers(),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 5, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 20),
                .. Clay
            ],
            Decorations =
            [
                (decorations.GetDecoration(tallGrass), 1.0f),
                (decorations.GetDecoration(tallRedFlower), 8.0f),
                (decorations.GetDecoration(tallYellowFlower), 8.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Birch)), 1.0f),
                (decorations.GetDecoration(roots), 1000.0f)
            ]
        });

        /// <summary>
        ///     A clearing in a woodland.
        /// </summary>
        public SubBiomeDefinition SeasonalForestClearing { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(SeasonalForestClearing), palette)
        {
            Amplitude = 7f,
            Frequency = 0.005f,
            Cover = new Cover.GrassAndFlowers(),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 5, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 20),
                .. Clay
            ],
            Decorations =
            [
                (decorations.GetDecoration(tallGrass), 1.0f),
                (decorations.GetDecoration(tallRedFlower), 8.0f),
                (decorations.GetDecoration(tallYellowFlower), 8.0f),
                (decorations.GetDecoration(roots), 1000.0f)
            ]
        });

        /// <summary>
        ///     A pond sub-biome in a woodland.
        /// </summary>
        public SubBiomeDefinition SeasonalForestPond { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(SeasonalForestPond), palette)
        {
            Amplitude = 5f,
            Frequency = 0.01f,
            Offset = -7,
            Stuffer = new Stuffer.Water(),
            Cover = new Cover.NoVegetation(),
            Layers =
            [
                Layer.CreateMud(width: 4),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 20),
                .. Clay
            ]
        });

        #endregion SeasonalForest

        #region DryForest

        /// <summary>
        ///     A woodland, but drier.
        /// </summary>
        public SubBiomeDefinition DryForestWoodland { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(DryForestWoodland), palette)
        {
            Amplitude = 7f,
            Frequency = 0.005f,
            Cover = new Cover.Grass(),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 3, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 26),
                .. Clay
            ],
            Decorations =
            [
                (decorations.GetDecoration(tallGrass), 1.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Teak)), 10.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Acacia)), 10.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Mesquite)), 10.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Walnut)), 10.0f),
                (decorations.GetDecoration(roots), 1000.0f)
            ]
        });

        /// <summary>
        ///     A dry area with shrubs.
        /// </summary>
        public SubBiomeDefinition DryForestShrubland { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(DryForestShrubland), palette)
        {
            Amplitude = 7f,
            Frequency = 0.005f,
            Cover = new Cover.Grass(),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 3, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 26),
                .. Clay
            ],
            Decorations =
            [
                (decorations.GetDecoration(tallGrass), 1.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Mesquite)), 30.0f)
            ]
        });

        /// <summary>
        ///     A dry area with no shrubs or trees.
        /// </summary>
        public SubBiomeDefinition DryForestGrassland { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(DryForestGrassland), palette)
        {
            Amplitude = 7f,
            Frequency = 0.005f,
            Cover = new Cover.Grass(),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 3, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 26),
                .. Clay
            ],
            Decorations = [(decorations.GetDecoration(tallGrass), 1.0f)]
        });

        /// <summary>
        ///     A dry, rocky area.
        /// </summary>
        public SubBiomeDefinition DryForestRocks { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(DryForestRocks), palette)
        {
            Amplitude = 10f,
            Frequency = 0.09f,
            Cover = new Cover.NoVegetation(),
            Layers =
            [
                Layer.CreateStonyTop(width: 4),
                Layer.CreateStonyDampen(maxWidth: 26),
                .. DoubleGroundwaterStone
            ],
            Decorations = [(decorations.GetDecoration(tallGrass), 1.0f)]
        });

        /// <summary>
        ///     A dried out pond.
        /// </summary>
        public SubBiomeDefinition DryForestDriedPond { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(DryForestDriedPond), palette)
        {
            Amplitude = 7f,
            Frequency = 0.01f,
            Offset = -8,
            Cover = new Cover.NoVegetation(),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.CrackedDriedMud, Blocks.Instance.CrackedDriedMud, width: 4),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 20),
                .. Clay
            ]
        });

        #endregion DryForest

        #region Shrubland

        /// <summary>
        ///     The part of the shrubland that has the shrubs.
        /// </summary>
        public SubBiomeDefinition ShrublandDefault { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(ShrublandDefault), palette)
        {
            Amplitude = 1f,
            Frequency = 0.01f,
            Cover = new Cover.Grass(),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 7, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 2),
                .. Clay
            ],
            Decorations =
            [
                (decorations.GetDecoration(tallGrass), 1.0f),
                (decorations.GetDecoration(boulder), 2000.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Juniper)), 100.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Mesquite)), 100.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.ShepherdsTree)), 1000.0f)
            ]
        });

        /// <summary>
        ///     The part of the shrubland that has many shrubs.
        /// </summary>
        public SubBiomeDefinition ShrublandDense { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(ShrublandDense), palette)
        {
            Amplitude = 1f,
            Frequency = 0.01f,
            Cover = new Cover.Grass(hasSucculents: true),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 7, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 2),
                .. Clay
            ],
            Decorations =
            [
                (decorations.GetDecoration(tallGrass), 1.0f),
                (decorations.GetDecoration(boulder), 2000.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Juniper)), 10.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Mesquite)), 10.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.ShepherdsTree)), 100.0f)
            ]
        });

        /// <summary>
        ///     The part of the shrubland that has no shrubs.
        /// </summary>
        public SubBiomeDefinition ShrublandGrassland { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(ShrublandGrassland), palette)
        {
            Amplitude = 1f,
            Frequency = 0.01f,
            Cover = new Cover.Grass(),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 7, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 2),
                .. Clay
            ],
            Decorations =
            [
                (decorations.GetDecoration(tallGrass), 1.0f),
                (decorations.GetDecoration(boulder), 2000.0f)
            ]
        });

        /// <summary>
        ///     The part of the shrubland that has no shrubs but many flowers.
        /// </summary>
        public SubBiomeDefinition ShrublandFlowerPatch { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(ShrublandFlowerPatch), palette)
        {
            Amplitude = 1f,
            Frequency = 0.01f,
            Cover = new Cover.GrassAndFlowers(isBlooming: true),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 7, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 2),
                .. Clay
            ],
            Decorations =
            [
                (decorations.GetDecoration(tallGrass), 1.0f),
                (decorations.GetDecoration(boulder), 2000.0f)
            ]
        });

        /// <summary>
        ///     The part of the shrubland that has a stony cover and nearly no vegetation.
        /// </summary>
        public SubBiomeDefinition ShrublandDryPatch { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(ShrublandDryPatch), palette)
        {
            Amplitude = 2f,
            Frequency = 0.1f,
            Cover = new Cover.NoVegetation(),
            Layers =
            [
                Layer.CreateStonyTop(width: 3, amplitude: 0),
                Layer.CreateStone(width: 3),
                Layer.CreateStonyDampen(maxWidth: 2),
                .. DoubleGroundwaterStone
            ]
        });

        #endregion Shrubland

        #region Desert

        /// <summary>
        ///     The default desert sub-biome.
        /// </summary>
        public SubBiomeDefinition DesertDefault { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(DesertDefault), palette)
        {
            Amplitude = 4f,
            Frequency = 0.008f,
            Cover = new Cover.NoVegetation(),
            Layers =
            [
                Layer.CreateSimple(Blocks.Instance.Sand, width: 9, isSolid: false),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 4, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 8),
                Layer.CreateSimple(Blocks.Instance.Sandstone, width: 18, isSolid: true),
                Layer.CreateLoose(width: 22),
                Layer.CreateGroundwater(width: 18),
                Layer.CreateSimple(Blocks.Instance.Clay, width: 21, isSolid: true)
            ],
            Decorations =
            [
                (decorations.GetDecoration(cactus), 50.0f)
            ],
            Structure = structures.GetStructure(RID.Named<StructureGeneratorDefinition>("SmallPyramid"))
        });

        /// <summary>
        ///     The desert sub-biome with dunes.
        /// </summary>
        public SubBiomeDefinition DesertDunes { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(DesertDunes), palette)
        {
            Amplitude = 4f,
            Frequency = 0.01f,
            Cover = new Cover.NoVegetation(),
            Layers =
            [
                Layer.CreateSimple(Blocks.Instance.Sand, width: 9, isSolid: false),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 4, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 8),
                Layer.CreateSimple(Blocks.Instance.Sandstone, width: 18, isSolid: true),
                Layer.CreateLoose(width: 22),
                Layer.CreateGroundwater(width: 18),
                Layer.CreateSimple(Blocks.Instance.Clay, width: 21, isSolid: true)
            ],
            Structure = structures.GetStructure(RID.Named<StructureGeneratorDefinition>("SmallPyramid"))
        });

        /// <summary>
        ///     The desert sub-biome with stones.
        /// </summary>
        public SubBiomeDefinition DesertStones { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(DesertStones), palette)
        {
            Amplitude = 2f,
            Frequency = 0.008f,
            Offset = 6,
            Cover = new Cover.NoVegetation(),
            Layers =
            [
                Layer.CreateSimple(Blocks.Instance.Sandstone, width: 13, isSolid: false),
                Layer.CreateStonyDampen(maxWidth: 8),
                Layer.CreateSimple(Blocks.Instance.Sandstone, width: 18, isSolid: true),
                Layer.CreateLoose(width: 22),
                Layer.CreateGroundwater(width: 18),
                Layer.CreateSimple(Blocks.Instance.Clay, width: 21, isSolid: true)
            ]
        });

        /// <summary>
        ///     The desert sub-biome with water.
        /// </summary>
        public SubBiomeDefinition DesertOasis { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(DesertOasis), palette)
        {
            Amplitude = 4f,
            Frequency = 0.04f,
            Offset = -4,
            Cover = new Cover.NoVegetation(),
            Stuffer = new Stuffer.Water(),
            Layers =
            [
                Layer.CreateOasisTop(width: 13, subBiomeOffset: 3),
                Layer.CreateStonyDampen(maxWidth: 8),
                Layer.CreateSimple(Blocks.Instance.Sandstone, width: 18, isSolid: true),
                Layer.CreateLoose(width: 22),
                Layer.CreateGroundwater(width: 18),
                Layer.CreateSimple(Blocks.Instance.Clay, width: 21, isSolid: true)
            ],
            Decorations =
            [
                (decorations.GetDecoration(Get(Blocks.Instance.DatePalm)), 10.0f)
            ]
        });

        /// <summary>
        ///     The desert sub-biome with salt.
        /// </summary>
        public SubBiomeDefinition DesertSalt { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(DesertSalt), palette)
        {
            Amplitude = 1f,
            Frequency = 0.008f,
            Cover = new Cover.Salt(),
            Layers =
            [
                Layer.CreateSimple(Blocks.Instance.Sand, width: 9, isSolid: false),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 4, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 8),
                Layer.CreateSimple(Blocks.Instance.Sandstone, width: 18, isSolid: true),
                Layer.CreateLoose(width: 22),
                Layer.CreateGroundwater(width: 18),
                Layer.CreateSimple(Blocks.Instance.Clay, width: 21, isSolid: true)
            ],
            Structure = structures.GetStructure(RID.Named<StructureGeneratorDefinition>("SmallPyramid"))
        });

        #endregion Desert

        #region Grassland

        /// <summary>
        ///     The main sub-biome of the grassland biome.
        /// </summary>
        public SubBiomeDefinition GrasslandMain { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(GrasslandMain), palette)
        {
            Amplitude = 4f,
            Frequency = 0.004f,
            Cover = new Cover.Grass(),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 7, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 8),
                ..Clay
            ],
            Decorations =
            [
                (decorations.GetDecoration(tallGrass), 1.0f),
                (decorations.GetDecoration(boulder), 2000.0f)
            ],
            Structure = structures.GetStructure(RID.Named<StructureGeneratorDefinition>("OldTower"))
        });

        /// <summary>
        ///     A hilly sub-biome of the grassland biome.
        /// </summary>
        public SubBiomeDefinition GrasslandHills { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(GrasslandHills), palette)
        {
            Amplitude = 10f,
            Frequency = 0.009f,
            Cover = new Cover.Grass(),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 7, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 8),
                ..Clay
            ],
            Decorations =
            [
                (decorations.GetDecoration(tallGrass), 1.0f),
                (decorations.GetDecoration(boulder), 2000.0f)
            ]
        });

        /// <summary>
        ///     A part of a grassland biome with blooming flowers.
        /// </summary>
        public SubBiomeDefinition GrasslandBlooming { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(GrasslandBlooming), palette)
        {
            Amplitude = 4f,
            Frequency = 0.004f,
            Cover = new Cover.GrassAndFlowers(isBlooming: true),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 7, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 8),
                ..Clay
            ],
            Decorations =
            [
                (decorations.GetDecoration(tallGrass), 1.0f),
                (decorations.GetDecoration(boulder), 2000.0f)
            ],
            Structure = structures.GetStructure(RID.Named<StructureGeneratorDefinition>("OldTower"))
        });

        /// <summary>
        ///     A thicket of trees in the grassland biome.
        /// </summary>
        public SubBiomeDefinition GrasslandThicket { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(GrasslandThicket), palette)
        {
            Amplitude = 4f,
            Frequency = 0.004f,
            Cover = new Cover.Grass(),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 7, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 8),
                ..Clay
            ],
            Decorations =
            [
                (decorations.GetDecoration(tallGrass), 1.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Oak)), 50.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Maple)), 50.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.AshTree)), 50.0f)
            ]
        });

        /// <summary>
        ///     A sub-biome of a grassland biome with exposed rocks.
        /// </summary>
        public SubBiomeDefinition GrasslandRocks { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(GrasslandRocks), palette)
        {
            Amplitude = 2f,
            Frequency = 0.004f,
            Offset = 7,
            Cover = new Cover.NoVegetation(),
            Layers =
            [
                Layer.CreateStonyTop(width: 8),
                Layer.CreateStonyDampen(maxWidth: 8),
                .. DoubleGroundwaterStone
            ]
        });

        /// <summary>
        ///     A bog sub-biome in the grassland biome.
        /// </summary>
        public SubBiomeDefinition GrasslandBog { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(GrasslandBog), palette)
        {
            Amplitude = 5f,
            Frequency = 0.001f,
            Offset = -6,
            Cover = new Cover.NoVegetation(),
            Stuffer = new Stuffer.Water(),
            Layers =
            [
                Layer.CreateSimple(Blocks.Instance.Peat, width: 6, isSolid: false),
                Layer.CreateSimple(Blocks.Instance.Mud, width: 3, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 7),
                ..Clay
            ]
        });

        #endregion Grassland

        #region Mountains

        /// <summary>
        ///     A smooth mountains sub-biome.
        /// </summary>
        public SubBiomeDefinition MountainsSmooth { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(MountainsSmooth), palette)
        {
            Amplitude = 30f,
            Frequency = 0.005f,
            Cover = new Cover.NoVegetation(),
            Layers =
            [
                Layer.CreateStonyTop(width: 9, amplitude: 15),
                Layer.CreateStonyDampen(maxWidth: 31),
                Layer.CreateStone(width: 31),
                Layer.CreateLoose(width: 9),
                Layer.CreateGroundwater(width: 1),
                Layer.CreateSimple(Blocks.Instance.Clay, width: 9, isSolid: true)
            ]
        });

        /// <summary>
        ///     A green, grass-covered mountains sub-biome.
        /// </summary>
        public SubBiomeDefinition MountainsGreen { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(MountainsGreen), palette)
        {
            Amplitude = 30f,
            Frequency = 0.005f,
            Cover = new Cover.Grass(),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 3, isSolid: false),
                Layer.CreateStone(width: 5),
                Layer.CreateStonyDampen(maxWidth: 31),
                Layer.CreateStone(width: 31),
                Layer.CreateLoose(width: 9),
                Layer.CreateGroundwater(width: 1),
                Layer.CreateSimple(Blocks.Instance.Clay, width: 9, isSolid: true)
            ]
        });

        /// <summary>
        ///     A rough mountains sub-biome.
        /// </summary>
        public SubBiomeDefinition MountainsRough { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(MountainsRough), palette)
        {
            Amplitude = 30f,
            Frequency = 0.005f,
            Cover = new Cover.NoVegetation(),
            Layers =
            [
                Layer.CreateStonyTop(width: 9, amplitude: 15),
                Layer.CreateStonyDampen(maxWidth: 31),
                Layer.CreateStone(width: 31),
                Layer.CreateLoose(width: 9),
                Layer.CreateGroundwater(width: 1),
                Layer.CreateSimple(Blocks.Instance.Clay, width: 9, isSolid: true)
            ]
        });

        #endregion Mountains

        #region Beach

        /// <summary>
        ///     The default beach sub-biome.
        /// </summary>
        public SubBiomeDefinition BeachDefault { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(BeachDefault), palette)
        {
            Amplitude = 4f,
            Frequency = 0.008f,
            Cover = new Cover.NoVegetation(),
            Layers =
            [
                Layer.CreateSimple(Blocks.Instance.Sand, width: 5, isSolid: false),
                Layer.CreateSimple(Blocks.Instance.Gravel, width: 3, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Gravel, maxWidth: 10),
                Layer.CreateSimple(Blocks.Instance.Limestone, width: 13, isSolid: true),
                Layer.CreateLoose(width: 22),
                Layer.CreateGroundwater(width: 18),
                Layer.CreateSimple(Blocks.Instance.Clay, width: 21, isSolid: true)
            ]
        });

        /// <summary>
        ///     A variant of the beach sub-biome with palm trees.
        /// </summary>
        public SubBiomeDefinition BeachPalms { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(BeachPalms), palette)
        {
            Amplitude = 4f,
            Frequency = 0.008f,
            Cover = new Cover.NoVegetation(),
            Layers =
            [
                Layer.CreateSimple(Blocks.Instance.Sand, width: 5, isSolid: false),
                Layer.CreateSimple(Blocks.Instance.Gravel, width: 3, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Gravel, maxWidth: 10),
                Layer.CreateSimple(Blocks.Instance.Limestone, width: 13, isSolid: true),
                Layer.CreateLoose(width: 22),
                Layer.CreateGroundwater(width: 18),
                Layer.CreateSimple(Blocks.Instance.Clay, width: 21, isSolid: true)
            ],
            Decorations =
            [
                (decorations.GetDecoration(Get(Blocks.Instance.CoconutPalm)), 25.0f)
            ]
        });

        #endregion Beach

        #region Cliffs

        /// <summary>
        ///     The grass covered cliff sub-biome, which is found at large height differences.
        /// </summary>
        public SubBiomeDefinition GrassyCliff { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(GrassyCliff), palette)
        {
            Amplitude = 4f,
            Frequency = 0.008f,
            Cover = new Cover.GrassAndFlowers(),
            Layers = (List<Layer>)
            [
                Layer.CreateCoastlineTop(Blocks.Instance.Grass, Blocks.Instance.Gravel, width: 1),
                Layer.CreateStone(width: 53),
                Layer.CreateStonyDampen(maxWidth: 28),
                Layer.CreateStone(width: 39)
            ]
        });

        /// <summary>
        ///     The sand covered cliff sub-biome, which is found at large height differences.
        /// </summary>
        public SubBiomeDefinition SandyCliff { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(SandyCliff), palette)
        {
            Amplitude = 4f,
            Frequency = 0.008f,
            Cover = new Cover.NoVegetation(),
            Layers = (List<Layer>)
            [
                Layer.CreateSimple(Blocks.Instance.Sand, width: 1, isSolid: false),
                Layer.CreateStone(width: 53),
                Layer.CreateStonyDampen(maxWidth: 28),
                Layer.CreateStone(width: 39)
            ]
        });

        #endregion Cliffs
    }
}
