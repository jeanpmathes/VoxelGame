// <copyright file="BiomeLoader.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Core.Generation.Worlds.Default.SubBiomes;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;
using VoxelGame.Toolkit.Collections;

namespace VoxelGame.Core.Generation.Worlds.Default.Biomes;

/// <summary>
///     Loads all biomes for this world generator, as well as the biome distribution.
/// </summary>
public sealed class BiomeLoader : IResourceLoader
{
    String? ICatalogEntry.Instance => null;

    /// <inheritdoc />
    public IEnumerable<IResource> Load(IResourceContext context)
    {
        return context.Require<ISubBiomeDefinitionProvider>(subBiomes =>
        {
            Registry<BiomeDefinition> registry = new(biome => biome.Name);
            Biomes biomes = new(registry, subBiomes);

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
        });
    }

    private sealed class Biomes(Registry<BiomeDefinition> biomes, ISubBiomeDefinitionProvider subBiomes)
    {
        /// <summary>
        ///     The polar desert biome.
        /// </summary>
        public BiomeDefinition PolarDesert { get; } = biomes.Register(new BiomeDefinition(nameof(PolarDesert))
        {
            Color = ColorS.Gray,
            SubBiomes =
            [
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("SnowField")), 5),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("LooseSnow")), 2),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("SnowyDunes")), 10),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("FrostyRidge")), 1),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("FrozenBasin")), 3)
            ]
        });

        /// <summary>
        ///     The tropical rainforest biome.
        /// </summary>
        public BiomeDefinition TropicalRainforest { get; } = biomes.Register(new BiomeDefinition(nameof(TropicalRainforest))
        {
            Color = ColorS.DarkGreen,
            SubBiomes =
            [
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("TropicalRainforestHills")), 6),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("TropicalRainforestFlats")), 6),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("TropicalRubberTreeGroup")), 3),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("TropicalBloomingClearing")), 1),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("TropicalRainforestPond")), 2)
            ]
        });

        /// <summary>
        ///     The temperate rainforest biome.
        /// </summary>
        public BiomeDefinition TemperateRainforest { get; } = biomes.Register(new BiomeDefinition(nameof(TemperateRainforest))
        {
            Color = ColorS.Green,
            SubBiomes =
            [
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("TemperateRainforestHills")), 6),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("TemperateRainforestFlats")), 6),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("CherryGrove")), 1),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("TemperateRainforestPond")), 2),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("MossyStones")), 2)
            ]
        });

        /// <summary>
        ///     The taiga biome.
        /// </summary>
        public BiomeDefinition Taiga { get; } = biomes.Register(new BiomeDefinition(nameof(Taiga))
        {
            Color = ColorS.Navy,
            SubBiomes =
            [
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("BorealForest")), 8),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("BorealPineForest")), 4),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("BorealSpruceForest")), 3),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("BorealFirForest")), 4),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("BorealWetland")), 5),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("BorealShrubland")), 2)
            ]
        });

        /// <summary>
        ///     The tundra biome.
        /// </summary>
        public BiomeDefinition Tundra { get; } = biomes.Register(new BiomeDefinition(nameof(Tundra))
        {
            Color = ColorS.CadetBlue,
            SubBiomes =
            [
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("ColdShrubland")), 12),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("ColdGrassland")), 8),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("ColdRidge")), 4),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("PermafrostPatch")), 1),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("ThawBasin")), 3),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("LichenField")), 2)
            ]
        });

        /// <summary>
        ///     The savanna biome.
        /// </summary>
        public BiomeDefinition Savanna { get; } = biomes.Register(new BiomeDefinition(nameof(Savanna))
        {
            Color = ColorS.Olive,
            SubBiomes =
            [
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("SavannaWoodland")), 10),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("SavannaDenseWoodland")), 5),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("SavannaShrubland")), 5),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("SavannaGrassland")), 5),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("Waterhole")), 1)
            ]
        });

        /// <summary>
        ///     The seasonal forest biome.
        /// </summary>
        public BiomeDefinition SeasonalForest { get; } = biomes.Register(new BiomeDefinition(nameof(SeasonalForest))
        {
            Color = ColorS.Lime,
            SubBiomes =
            [
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("Woodland")), 12),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("BirchGrove")), 2),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("Clearing")), 3),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("Pond")), 1)
            ]
        });

        /// <summary>
        ///     The dry forest biome.
        /// </summary>
        public BiomeDefinition DryForest { get; } = biomes.Register(new BiomeDefinition(nameof(DryForest))
        {
            Color = ColorS.SeaGreen,
            SubBiomes =
            [
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("DryWoodland")), 10),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("DryShrubland")), 4),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("DryGrassland")), 4),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("DryRocks")), 1),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("DriedPond")), 3)
            ]
        });

        /// <summary>
        ///     The shrubland biome.
        /// </summary>
        public BiomeDefinition Shrubland { get; } = biomes.Register(new BiomeDefinition(nameof(Shrubland))
        {
            Color = ColorS.Salmon,
            SubBiomes =
            [
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("ShrubbyShrubland")), 20),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("VeryShrubbyShrubland")), 5),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("ShrubbyGrassland")), 8),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("ShrubbyFlowerPatch")), 3),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("ShrubbyDryPatch")), 1)
            ]
        });

        /// <summary>
        ///     The desert biome.
        /// </summary>
        public BiomeDefinition Desert { get; } = biomes.Register(new BiomeDefinition(nameof(Desert))
        {
            Color = ColorS.Yellow,
            SubBiomes =
            [
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("DesertDefault")), 30),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("DesertDunes")), 15),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("DesertStones")), 10),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("DesertOasis")), 3),
                (subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>("DesertSalt")), 1)
            ]
        });

        /// <summary>
        ///     The grassland biome.
        /// </summary>
        public BiomeDefinition Grassland { get; } = biomes.Register(new BiomeDefinition(nameof(Grassland))
        {
            Color = ColorS.SaddleBrown,
            SubBiomes = [(subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>(nameof(Grassland))), 1)]
        });

        /// <summary>
        ///     The normal ocean biome.
        /// </summary>
        public BiomeDefinition Ocean { get; } = biomes.Register(new BiomeDefinition(nameof(Ocean))
        {
            Color = ColorS.White,
            SubBiomes = [(subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>(nameof(Ocean))), 1)]
        });

        /// <summary>
        ///     The polar ocean biome. It is covered in ice and occurs in cold regions.
        /// </summary>
        public BiomeDefinition PolarOcean { get; } = biomes.Register(new BiomeDefinition(nameof(PolarOcean))
        {
            Color = ColorS.White,
            SubBiomes = [(subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>(nameof(PolarOcean))), 1)]
        });

        /// <summary>
        ///     The mountain biome. It is a special biome that depends on the height of the terrain.
        /// </summary>
        public BiomeDefinition Mountains { get; } = biomes.Register(new BiomeDefinition(nameof(Mountains))
        {
            Color = ColorS.Black,
            SubBiomes = [(subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>(nameof(Mountains))), 1)]
        });

        /// <summary>
        ///     The beach biome. It is found at low heights next to coastlines.
        /// </summary>
        public BiomeDefinition Beach { get; } = biomes.Register(new BiomeDefinition(nameof(Beach))
        {
            Color = ColorS.Orange,
            SubBiomes = [(subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>(nameof(Beach))), 1)]
        });

        /// <summary>
        ///     The grass covered cliff biome, which is found at large height differences.
        /// </summary>
        public BiomeDefinition GrassyCliff { get; } = biomes.Register(new BiomeDefinition(nameof(GrassyCliff))
        {
            Color = ColorS.LightGray,
            SubBiomes = [(subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>(nameof(GrassyCliff))), 1)]
        });

        /// <summary>
        ///     The sand covered cliff biome, which is found at large height differences.
        /// </summary>
        public BiomeDefinition SandyCliff { get; } = biomes.Register(new BiomeDefinition(nameof(SandyCliff))
        {
            Color = ColorS.SlateGray,
            SubBiomes = [(subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>(nameof(SandyCliff))), 1)]
        });
    }
}
