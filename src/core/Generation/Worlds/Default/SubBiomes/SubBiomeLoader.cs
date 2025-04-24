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
        private static readonly RID cactus = RID.Named<Decoration>("Cactus");
        public Registry<SubBiomeDefinition> Registry => subBiomes;

        private static IEnumerable<Layer> Permafrost =>
        [
            Layer.CreateSimple(Blocks.Instance.Permafrost, width: 27, isSolid: true),
            Layer.CreateLoose(width: 27),
            Layer.CreateGroundwater(width: 8),
            Layer.CreateSimple(Blocks.Instance.Clay, width: 21, isSolid: true)
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

        /// <summary>
        ///     A snow-filled flat field.
        /// </summary>
        public SubBiomeDefinition SnowField { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(SnowField), palette)
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
        public SubBiomeDefinition LooseSnow { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(LooseSnow), palette)
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
        public SubBiomeDefinition SnowyDunes { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(SnowyDunes), palette)
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
        public SubBiomeDefinition FrostyRidge { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(FrostyRidge), palette)
        {
            Amplitude = 5f,
            Frequency = 0.09f,
            Cover = new Cover.NoVegetation(),
            Layers =
            [
                Layer.CreateStone(width: 5),
                Layer.CreateDampen(Blocks.Instance.Permafrost, maxWidth: 9),
                .. Permafrost
            ]
        });

        /// <summary>
        ///     Frozen water basins.
        /// </summary>
        public SubBiomeDefinition FrozenBasin { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(FrozenBasin), palette)
        {
            Amplitude = 4f,
            Frequency = 0.03f,
            Offset = -3,
            Cover = new Cover.NoVegetation(),
            Stuffer = new Stuffer.Ice(),
            Layers =
            [
                Layer.CreateStone(width: 5),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 9),
                .. Permafrost
            ]
        });

        /// <summary>
        ///     Hilly sub-biome of a rainforest.
        /// </summary>
        public SubBiomeDefinition TropicalRainforestHills { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(TropicalRainforestHills), palette)
        {
            Amplitude = 15f,
            Frequency = 0.005f,
            Cover = new Cover.Grass(),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 3, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 26),
                .. Clay
            ],
            Decorations = new List<(Decoration, Single)>
            {
                (decorations.GetDecoration(tallGrass), 1.0f),
                (decorations.GetDecoration(tallRedFlower), 8.0f),
                (decorations.GetDecoration(tallYellowFlower), 8.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Mahogany)), 3.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Teak)), 3.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Ebony)), 30.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.RubberTree)), 30.0f),
                (decorations.GetDecoration(roots), 1000.0f),
                (decorations.GetDecoration(vines), 1.0f)
            },
            Structure = structures.GetStructure(RID.Named<StructureGeneratorDefinition>("LargeTropicalTree"))
        });

        /// <summary>
        ///     Flat sub-biome of a rainforest.
        /// </summary>
        public SubBiomeDefinition TropicalRainforestFlats { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(TropicalRainforestFlats), palette)
        {
            Amplitude = 5f,
            Frequency = 0.005f,
            Cover = new Cover.Grass(),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 3, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 26),
                .. Clay
            ],
            Decorations = new List<(Decoration, Single)>
            {
                (decorations.GetDecoration(tallGrass), 1.0f),
                (decorations.GetDecoration(tallRedFlower), 8.0f),
                (decorations.GetDecoration(tallYellowFlower), 8.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Mahogany)), 3.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Teak)), 3.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Ebony)), 30.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.RubberTree)), 30.0f),
                (decorations.GetDecoration(roots), 1000.0f),
                (decorations.GetDecoration(vines), 1.0f)
            },
            Structure = structures.GetStructure(RID.Named<StructureGeneratorDefinition>("LargeTropicalTree"))
        });

        /// <summary>
        ///     A group of rubber trees.
        /// </summary>
        public SubBiomeDefinition TropicalRubberTreeGroup { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(TropicalRubberTreeGroup), palette)
        {
            Amplitude = 10f,
            Frequency = 0.005f,
            Cover = new Cover.Grass(),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 3, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 26),
                .. Clay
            ],
            Decorations = new List<(Decoration, Single)>
            {
                (decorations.GetDecoration(tallGrass), 1.0f),
                (decorations.GetDecoration(tallRedFlower), 8.0f),
                (decorations.GetDecoration(tallYellowFlower), 8.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.RubberTree)), 3.0f),
                (decorations.GetDecoration(roots), 1000.0f),
                (decorations.GetDecoration(vines), 1.0f)
            }
        });

        /// <summary>
        ///     A clearing filled with flowers.
        /// </summary>
        public SubBiomeDefinition TropicalBloomingClearing { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(TropicalBloomingClearing), palette)
        {
            Amplitude = 5f,
            Frequency = 0.005f,
            Cover = new Cover.Grass(isBlooming: true),
            Layers =
            [
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 3, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 26),
                .. Clay
            ],
            Decorations = new List<(Decoration, Single)>
            {
                (decorations.GetDecoration(tallGrass), 4.0f),
                (decorations.GetDecoration(tallRedFlower), 4.0f),
                (decorations.GetDecoration(tallYellowFlower), 4.0f)
            }
        });

        /// <summary>
        ///     A pond sub-biome in a rainforest.
        /// </summary>
        public SubBiomeDefinition TropicalRainforestPond { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(TropicalRainforestPond), palette)
        {
            Amplitude = 10f,
            Frequency = 0.005f,
            Offset = -9,
            Cover = new Cover.Grass(),
            Stuffer = new Stuffer.Water(),
            Layers =
            [
                Layer.CreateMud(width: 4),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 26),
                .. Clay
            ],
            Decorations = new List<(Decoration, Single)>
            {
                (decorations.GetDecoration(tallGrass), 1.0f),
                (decorations.GetDecoration(tallRedFlower), 8.0f),
                (decorations.GetDecoration(tallYellowFlower), 8.0f)
            }
        });

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
        public SubBiomeDefinition CherryGrove { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(CherryGrove), palette)
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
        public SubBiomeDefinition MossyStones { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(MossyStones), palette)
        {
            Amplitude = 5f,
            Frequency = 0.09f,
            Cover = new Cover.Moss(),
            Layers =
            [
                Layer.CreateStone(width: 4),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 26),
                .. Clay
            ]
        });

        /// <summary>
        ///     Boreal forest sub-biome.
        /// </summary>
        public SubBiomeDefinition BorealForest { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(BorealForest), palette)
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
        public SubBiomeDefinition BorealPineForest { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(BorealPineForest), palette)
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
        public SubBiomeDefinition BorealSpruceForest { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(BorealSpruceForest), palette)
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
        public SubBiomeDefinition BorealFirForest { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(BorealFirForest), palette)
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
        ///     Boreal shrubland sub-biome.
        /// </summary>
        public SubBiomeDefinition BorealShrubland { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(BorealShrubland), palette)
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

        /// <summary>
        ///     A boreal wetland - a swampy area.
        /// </summary>
        public SubBiomeDefinition BorealWetland { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(BorealWetland), palette)
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
        ///     A very cold shrubland.
        /// </summary>
        public SubBiomeDefinition ColdShrubland { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(ColdShrubland), palette)
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
        ///     A very cold grassland.
        /// </summary>
        public SubBiomeDefinition ColdGrassland { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(ColdGrassland), palette)
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
        public SubBiomeDefinition ColdRidge { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(ColdRidge), palette)
        {
            Amplitude = 5f,
            Frequency = 0.06f,
            Cover = new Cover.Lichen(Cover.Lichen.Density.Low),
            Layers =
            [
                Layer.CreateStone(width: 5),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 9),
                .. Permafrost
            ]
        });

        /// <summary>
        ///     A patch of permafrost directly at the surface.
        /// </summary>
        public SubBiomeDefinition PermafrostPatch { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(PermafrostPatch), palette)
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
        public SubBiomeDefinition ThawBasin { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(ThawBasin), palette)
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
        public SubBiomeDefinition LichenField { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(LichenField), palette)
        {
            Amplitude = 2f,
            Frequency = 0.007f,
            Cover = new Cover.Lichen(Cover.Lichen.Density.High),
            Layers =
            [
                Layer.CreateStone(width: 5),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 9),
                .. Permafrost
            ]
        });

        /// <summary>
        ///     The savanna sub-biome.
        /// </summary>
        public SubBiomeDefinition Savanna { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(Savanna), palette)
        {
            Amplitude = 1f,
            Frequency = 0.01f,
            Cover = new Cover.Grass(),
            Layers = new List<Layer>
            {
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 7, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 2),
                Layer.CreateLoose(width: 3),
                Layer.CreateGroundwater(width: 2),
                Layer.CreateSimple(Blocks.Instance.Clay, width: 3, isSolid: true),
                Layer.CreateLoose(width: 37),
                Layer.CreateGroundwater(width: 18),
                Layer.CreateSimple(Blocks.Instance.Clay, width: 21, isSolid: true)
            },
            Decorations = new List<(Decoration, Single)>
            {
                (decorations.GetDecoration(tallGrass), 1.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Acacia)), 30.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Baobab)), 30.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.ShepherdsTree)), 30.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Mesquite)), 30.0f)
            }
        });

        /// <summary>
        ///     A woodland sub-biome.
        /// </summary>
        public SubBiomeDefinition Woodland { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(Woodland), palette)
        {
            Amplitude = 7f,
            Frequency = 0.005f,
            Cover = new Cover.Grass(mushrooms: true),
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
        public SubBiomeDefinition BirchGrove { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(BirchGrove), palette)
        {
            Amplitude = 7f,
            Frequency = 0.005f,
            Cover = new Cover.Grass(),
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
        public SubBiomeDefinition Clearing { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(Clearing), palette)
        {
            Amplitude = 7f,
            Frequency = 0.005f,
            Cover = new Cover.Grass(),
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
        public SubBiomeDefinition Pond { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(Pond), palette)
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

        /// <summary>
        ///     The dry forest sub-biome.
        /// </summary>
        public SubBiomeDefinition DryForest { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(DryForest), palette)
        {
            Amplitude = 7f,
            Frequency = 0.005f,
            Cover = new Cover.Grass(),
            Layers = new List<Layer>
            {
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 3, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 26),
                Layer.CreateLoose(width: 3),
                Layer.CreateGroundwater(width: 6),
                Layer.CreateSimple(Blocks.Instance.Clay, width: 3, isSolid: true),
                Layer.CreateLoose(width: 33),
                Layer.CreateGroundwater(width: 18),
                Layer.CreateSimple(Blocks.Instance.Clay, width: 21, isSolid: true)
            },
            Decorations = new List<(Decoration, Single)>
            {
                (decorations.GetDecoration(tallGrass), 1.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Teak)), 3.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Acacia)), 3.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Mesquite)), 3.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Walnut)), 3.0f),
                (decorations.GetDecoration(roots), 1000.0f)
            }
        });

        /// <summary>
        ///     The shrubland sub-biome.
        /// </summary>
        public SubBiomeDefinition Shrubland { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(Shrubland), palette)
        {
            Amplitude = 1f,
            Frequency = 0.01f,
            Cover = new Cover.Grass(),
            Layers = new List<Layer>
            {
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 7, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 2),
                Layer.CreateLoose(width: 3),
                Layer.CreateGroundwater(width: 2),
                Layer.CreateSimple(Blocks.Instance.Clay, width: 3, isSolid: true),
                Layer.CreateLoose(width: 37),
                Layer.CreateGroundwater(width: 18),
                Layer.CreateSimple(Blocks.Instance.Clay, width: 21, isSolid: true)
            },
            Decorations = new List<(Decoration, Single)>
            {
                (decorations.GetDecoration(tallGrass), 1.0f),
                (decorations.GetDecoration(boulder), 2000.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Juniper)), 100.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Mesquite)), 100.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.ShepherdsTree)), 1000.0f)
            }
        });

        /// <summary>
        ///     The desert sub-biome.
        /// </summary>
        public SubBiomeDefinition Desert { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(Desert), palette)
        {
            Amplitude = 4f,
            Frequency = 0.008f,
            Cover = new Cover.NoVegetation(),
            Layers = new List<Layer>
            {
                Layer.CreateSimple(Blocks.Instance.Sand, width: 9, isSolid: false),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 4, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 8),
                Layer.CreateSimple(Blocks.Instance.Sandstone, width: 18, isSolid: true),
                Layer.CreateLoose(width: 22),
                Layer.CreateGroundwater(width: 18),
                Layer.CreateSimple(Blocks.Instance.Clay, width: 21, isSolid: true)
            },
            Decorations = new List<(Decoration, Single)>
            {
                (decorations.GetDecoration(cactus), 50.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.DatePalm)), 1000.0f)
            },
            Structure = structures.GetStructure(RID.Named<StructureGeneratorDefinition>("SmallPyramid"))
        });

        /// <summary>
        ///     The grassland sub-biome.
        /// </summary>
        public SubBiomeDefinition Grassland { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(Grassland), palette)
        {
            Amplitude = 4f,
            Frequency = 0.004f,
            Cover = new Cover.Grass(),
            Layers = new List<Layer>
            {
                Layer.CreateTop(Blocks.Instance.Grass, Blocks.Instance.Dirt, width: 1),
                Layer.CreateSimple(Blocks.Instance.Dirt, width: 7, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Dirt, maxWidth: 8),
                Layer.CreateLoose(width: 3),
                Layer.CreateGroundwater(width: 2),
                Layer.CreateSimple(Blocks.Instance.Clay, width: 3, isSolid: true),
                Layer.CreateLoose(width: 37),
                Layer.CreateGroundwater(width: 18),
                Layer.CreateSimple(Blocks.Instance.Clay, width: 21, isSolid: true)
            },
            Decorations = new List<(Decoration, Single)>
            {
                (decorations.GetDecoration(tallGrass), 1.0f),
                (decorations.GetDecoration(boulder), 2000.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Oak)), 5000.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.Maple)), 5000.0f),
                (decorations.GetDecoration(Get(Blocks.Instance.AshTree)), 5000.0f)
            },
            Structure = structures.GetStructure(RID.Named<StructureGeneratorDefinition>("OldTower"))
        });

        /// <summary>
        ///     The normal ocean sub-biome.
        /// </summary>
        public SubBiomeDefinition Ocean { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(Ocean), palette)
        {
            Amplitude = 5.0f,
            Frequency = 0.005f,
            Cover = new Cover.NoVegetation(),
            Layers = new List<Layer>
            {
                Layer.CreateSimple(Blocks.Instance.Sand, width: 5, isSolid: false),
                Layer.CreateSimple(Blocks.Instance.Gravel, width: 3, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Gravel, maxWidth: 10),
                Layer.CreateSimple(Blocks.Instance.Limestone, width: 26, isSolid: true),
                Layer.CreateLoose(width: 37),
                Layer.CreateSimple(Blocks.Instance.Limestone, width: 21, isSolid: true)
            }
        });

        /// <summary>
        ///     The polar ocean sub-biome. It is covered in ice and occurs in cold regions.
        /// </summary>
        public SubBiomeDefinition PolarOcean { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(PolarOcean), palette)
        {
            Amplitude = 5.0f,
            Frequency = 0.005f,
            IceWidth = 6,
            Cover = new Cover.NoVegetation(),
            Layers = new List<Layer>
            {
                Layer.CreateSimple(Blocks.Instance.Sand, width: 5, isSolid: false),
                Layer.CreateSimple(Blocks.Instance.Gravel, width: 3, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Gravel, maxWidth: 10),
                Layer.CreateSimple(Blocks.Instance.Limestone, width: 26, isSolid: true),
                Layer.CreateLoose(width: 37),
                Layer.CreateSimple(Blocks.Instance.Limestone, width: 21, isSolid: true)
            }
        });

        /// <summary>
        ///     The mountain sub-biome. It is a special sub-biome that depends on the height of the terrain.
        /// </summary>
        public SubBiomeDefinition Mountains { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(Mountains), palette)
        {
            Amplitude = 30f,
            Frequency = 0.005f,
            Cover = new Cover.NoVegetation(),
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
        ///     The beach sub-biome. It is found at low heights next to coastlines.
        /// </summary>
        public SubBiomeDefinition Beach { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(Beach), palette)
        {
            Amplitude = 4f,
            Frequency = 0.008f,
            Cover = new Cover.NoVegetation(),
            Layers = new List<Layer>
            {
                Layer.CreateSimple(Blocks.Instance.Sand, width: 5, isSolid: false),
                Layer.CreateSimple(Blocks.Instance.Gravel, width: 3, isSolid: false),
                Layer.CreateDampen(Blocks.Instance.Gravel, maxWidth: 10),
                Layer.CreateSimple(Blocks.Instance.Limestone, width: 13, isSolid: true),
                Layer.CreateLoose(width: 22),
                Layer.CreateGroundwater(width: 18),
                Layer.CreateSimple(Blocks.Instance.Clay, width: 21, isSolid: true)
            },
            Decorations = new List<(Decoration, Single)>
            {
                (decorations.GetDecoration(Get(Blocks.Instance.CoconutPalm)), 25.0f)
            }
        });

        /// <summary>
        ///     The grass covered cliff sub-biome, which is found at large height differences.
        /// </summary>
        public SubBiomeDefinition GrassyCliff { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(GrassyCliff), palette)
        {
            Amplitude = 4f,
            Frequency = 0.008f,
            Cover = new Cover.Grass(),
            Layers = new List<Layer>
            {
                Layer.CreateCoastlineTop(Blocks.Instance.Grass, Blocks.Instance.Gravel, width: 1),
                Layer.CreateStone(width: 53),
                Layer.CreateStonyDampen(maxWidth: 28),
                Layer.CreateStone(width: 39)
            }
        });

        /// <summary>
        ///     The sand covered cliff sub-biome, which is found at large height differences.
        /// </summary>
        public SubBiomeDefinition SandyCliff { get; } = subBiomes.Register(new SubBiomeDefinition(nameof(SandyCliff), palette)
        {
            Amplitude = 4f,
            Frequency = 0.008f,
            Cover = new Cover.NoVegetation(),
            Layers = new List<Layer>
            {
                Layer.CreateSimple(Blocks.Instance.Sand, width: 1, isSolid: false),
                Layer.CreateStone(width: 53),
                Layer.CreateStonyDampen(maxWidth: 28),
                Layer.CreateStone(width: 39)
            }
        });

        #pragma warning disable S3242 // Types have meaning.
        private static RID Get(Wood wood)
        {
            return RID.Named<Decoration>(wood.NamedID);
        }
        #pragma warning restore S3242
    }
}
