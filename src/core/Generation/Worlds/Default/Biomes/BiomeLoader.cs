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

#pragma warning disable S1192 // Similar strings are pure coincidences and not related to each other.

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

            #pragma warning disable S103 // Long lines required for representing the distribution.

            BiomeDistributionDefinition distribution = new(new Array2D<BiomeDefinition?>([
                [biomes.ContinentalIceSheet, biomes.PolarDesert, biomes.Tundra, biomes.Tundra, biomes.Taiga, biomes.Taiga, biomes.Grassland, biomes.Grassland, biomes.Grassland, biomes.Grassland, biomes.Grassland, biomes.Grassland, biomes.Desert, biomes.Desert, biomes.Desert, biomes.Desert, biomes.Desert, biomes.Desert, biomes.Desert, biomes.Desert],
                [null, biomes.PolarDesert, biomes.Tundra, biomes.Tundra, biomes.Taiga, biomes.Taiga, biomes.Grassland, biomes.Grassland, biomes.Grassland, biomes.Grassland, biomes.Grassland, biomes.Grassland, biomes.Desert, biomes.Desert, biomes.Desert, biomes.Desert, biomes.Desert, biomes.Desert, biomes.Desert, biomes.Desert],
                [null, null, biomes.Tundra, biomes.Tundra, biomes.Taiga, biomes.Taiga, biomes.Shrubland, biomes.Shrubland, biomes.Shrubland, biomes.Shrubland, biomes.Shrubland, biomes.Shrubland, biomes.Savanna, biomes.Savanna, biomes.Desert, biomes.Desert, biomes.Desert, biomes.Desert, biomes.Desert, biomes.Desert],
                [null, null, null, biomes.Tundra, biomes.Taiga, biomes.Taiga, biomes.Shrubland, biomes.Shrubland, biomes.Shrubland, biomes.Shrubland, biomes.Shrubland, biomes.Shrubland, biomes.Savanna, biomes.Savanna, biomes.Desert, biomes.Desert, biomes.Desert, biomes.Desert, biomes.Desert, biomes.Desert],
                [null, null, null, null, biomes.Taiga, biomes.Taiga, biomes.SeasonalForest, biomes.SeasonalForest, biomes.SeasonalForest, biomes.SeasonalForest, biomes.SeasonalForest, biomes.SeasonalForest, biomes.Shrubland, biomes.Shrubland, biomes.Savanna, biomes.Savanna, biomes.Savanna, biomes.Savanna, biomes.Savanna, biomes.Savanna],
                [null, null, null, null, null, biomes.Taiga, biomes.SeasonalForest, biomes.SeasonalForest, biomes.SeasonalForest, biomes.SeasonalForest, biomes.SeasonalForest, biomes.SeasonalForest, biomes.Shrubland, biomes.Shrubland, biomes.Savanna, biomes.Savanna, biomes.Savanna, biomes.Savanna, biomes.Savanna, biomes.Savanna],
                [null, null, null, null, null, null, biomes.SeasonalForest, biomes.SeasonalForest, biomes.SeasonalForest, biomes.SeasonalForest, biomes.SeasonalForest, biomes.SeasonalForest, biomes.SeasonalForest, biomes.SeasonalForest, biomes.DryForest, biomes.DryForest, biomes.DryForest, biomes.DryForest, biomes.DryForest, biomes.DryForest],
                [null, null, null, null, null, null, null, biomes.SeasonalForest, biomes.SeasonalForest, biomes.SeasonalForest, biomes.SeasonalForest, biomes.SeasonalForest, biomes.SeasonalForest, biomes.SeasonalForest, biomes.DryForest, biomes.DryForest, biomes.DryForest, biomes.DryForest, biomes.DryForest, biomes.DryForest],
                [null, null, null, null, null, null, null, null, biomes.SeasonalForest, biomes.SeasonalForest, biomes.SeasonalForest, biomes.SeasonalForest, biomes.SeasonalForest, biomes.SeasonalForest, biomes.DryForest, biomes.DryForest, biomes.DryForest, biomes.DryForest, biomes.DryForest, biomes.DryForest],
                [null, null, null, null, null, null, null, null, null, biomes.SeasonalForest, biomes.SeasonalForest, biomes.SeasonalForest, biomes.SeasonalForest, biomes.SeasonalForest, biomes.DryForest, biomes.DryForest, biomes.DryForest, biomes.DryForest, biomes.DryForest, biomes.DryForest],
                [null, null, null, null, null, null, null, null, null, null, biomes.TemperateRainforest, biomes.TemperateRainforest, biomes.TemperateRainforest, biomes.TemperateRainforest, biomes.TropicalRainforest, biomes.TropicalRainforest, biomes.DryForest, biomes.DryForest, biomes.DryForest, biomes.DryForest],
                [null, null, null, null, null, null, null, null, null, null, null, biomes.TemperateRainforest, biomes.TemperateRainforest, biomes.TemperateRainforest, biomes.TropicalRainforest, biomes.TropicalRainforest, biomes.DryForest, biomes.DryForest, biomes.DryForest, biomes.DryForest],
                [null, null, null, null, null, null, null, null, null, null, null, null, biomes.TemperateRainforest, biomes.TemperateRainforest, biomes.TropicalRainforest, biomes.TropicalRainforest, biomes.TropicalRainforest, biomes.TropicalRainforest, biomes.DryForest, biomes.DryForest],
                [null, null, null, null, null, null, null, null, null, null, null, null, null, biomes.TemperateRainforest, biomes.TropicalRainforest, biomes.TropicalRainforest, biomes.TropicalRainforest, biomes.TropicalRainforest, biomes.DryForest, biomes.DryForest],
                [null, null, null, null, null, null, null, null, null, null, null, null, null, null, biomes.TropicalRainforest, biomes.TropicalRainforest, biomes.TropicalRainforest, biomes.TropicalRainforest, biomes.TropicalRainforest, biomes.TropicalRainforest],
                [null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, biomes.TropicalRainforest, biomes.TropicalRainforest, biomes.TropicalRainforest, biomes.TropicalRainforest, biomes.TropicalRainforest],
                [null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, biomes.TropicalRainforest, biomes.TropicalRainforest, biomes.TropicalRainforest, biomes.TropicalRainforest],
                [null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, biomes.TropicalRainforest, biomes.TropicalRainforest, biomes.TropicalRainforest],
                [null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, biomes.TropicalRainforest, biomes.TropicalRainforest],
                [null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, biomes.TropicalRainforest]
            ]))
            {
                Beach = biomes.Beach,
                Mountain = biomes.Mountains,
                Desert = biomes.Desert,
                GrassyCliff = biomes.GrassyCliff,
                SandyCliff = biomes.SandyCliff,
                Ocean = biomes.Ocean,
                PolarDesert = biomes.PolarDesert,
                PolarOcean = biomes.PolarOcean,
                OceanicIceSheet = biomes.OceanicIceSheet,
                ContinentalIceSheet = biomes.ContinentalIceSheet
            };

            #pragma warning restore S103

            return [..registry.Values, distribution];
        });
    }

    private sealed class Biomes(Registry<BiomeDefinition> biomes, ISubBiomeDefinitionProvider subBiomes)
    {
        /// <summary>
        ///     A thick layer of ice that covers the land below.
        /// </summary>
        public BiomeDefinition ContinentalIceSheet { get; } = biomes.Register(new BiomeDefinition(nameof(ContinentalIceSheet))
        {
            Color = ColorS.LightGray,
            SubBiomes =
            [
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(ContinentalIceSheet), "Snowy")), 4),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(ContinentalIceSheet), "Bare")), 3)
            ]
        });

        /// <summary>
        ///     The polar desert biome.
        /// </summary>
        public BiomeDefinition PolarDesert { get; } = biomes.Register(new BiomeDefinition(nameof(PolarDesert))
        {
            Color = ColorS.Gray,
            SubBiomes =
            [
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(PolarDesert), "Snowy")), 5),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(PolarDesert), "LooseSnow")), 2),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(PolarDesert), "Dunes")), 10),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(PolarDesert), "Ridge")), 1),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(PolarDesert), "Basin")), 3)
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
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(TropicalRainforest), "Hills")), 6),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(TropicalRainforest), "Flats")), 6),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(TropicalRainforest), "RubberTrees")), 3),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(TropicalRainforest), "BloomingClearing")), 1),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(TropicalRainforest), "Pond")), 2)
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
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(TemperateRainforest), "Hills")), 6),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(TemperateRainforest), "Flats")), 6),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(TemperateRainforest), "CherryGrove")), 1),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(TemperateRainforest), "Pond")), 2),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(TemperateRainforest), "MossyStones")), 2)
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
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Taiga), "Forest")), 8),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Taiga), "PineForest")), 4),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Taiga), "SpruceForest")), 3),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Taiga), "FirForest")), 4),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Taiga), "Wetland")), 5),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Taiga), "Shrubland")), 2)
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
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Tundra), "Shrubland")), 12),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Tundra), "Grassland")), 8),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Tundra), "Ridge")), 4),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Tundra), "PermafrostPatch")), 1),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Tundra), "Basin")), 3),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Tundra), "Lichen")), 2)
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
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Savanna), "Woodland")), 10),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Savanna), "DenseWoodland")), 5),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Savanna), "Shrubland")), 5),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Savanna), "Grassland")), 5),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Savanna), "Waterhole")), 1)
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
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(SeasonalForest), "Woodland")), 12),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(SeasonalForest), "BirchGrove")), 2),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(SeasonalForest), "Clearing")), 3),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(SeasonalForest), "Pond")), 1)
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
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(DryForest), "Woodland")), 10),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(DryForest), "Shrubland")), 4),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(DryForest), "Grassland")), 4),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(DryForest), "Rocks")), 1),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(DryForest), "DriedPond")), 3)
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
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Shrubland), "Default")), 20),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Shrubland), "Dense")), 5),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Shrubland), "Grassland")), 8),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Shrubland), "FlowerPatch")), 3),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Shrubland), "DryPatch")), 1)
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
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Desert), "Default")), 30),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Desert), "Dunes")), 15),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Desert), "Stones")), 10),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Desert), "Oasis")), 3),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Desert), "Salt")), 1)
            ]
        });

        /// <summary>
        ///     The grassland biome.
        /// </summary>
        public BiomeDefinition Grassland { get; } = biomes.Register(new BiomeDefinition(nameof(Grassland))
        {
            Color = ColorS.SaddleBrown,
            SubBiomes =
            [
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Grassland), "Main")), 1),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Grassland), "Blooming")), 1),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Grassland), "Thicket")), 1),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Grassland), "Hills")), 1),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Grassland), "Rocks")), 1),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Grassland), "Bog")), 1)
            ]
        });

        /// <summary>
        ///     The normal ocean biome.
        /// </summary>
        public BiomeDefinition Ocean { get; } = biomes.Register(new BiomeDefinition(nameof(Ocean))
        {
            Color = ColorS.White,
            SubBiomes = [(subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Ocean), "Floor")), 1)],
            OceanicSubBiomes = [(subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Ocean), "Open")), 1)]
        });

        /// <summary>
        ///     The polar ocean biome. It is covered in ice and occurs in cold regions.
        /// </summary>
        public BiomeDefinition PolarOcean { get; } = biomes.Register(new BiomeDefinition(nameof(PolarOcean))
        {
            Color = ColorS.White,
            SubBiomes = [(subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(PolarOcean), "Floor")), 1)],
            OceanicSubBiomes =
            [
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(PolarOcean), "Open")), 25),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(PolarOcean), "ThinIce")), 15),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(PolarOcean), "ThickIce")), 1),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(PolarOcean), "Icebergs")), 5)
            ]
        });

        /// <summary>
        ///     The oceanic ice sheet biome. It covers an oceanic area with a thick layer of ice.
        /// </summary>
        public BiomeDefinition OceanicIceSheet { get; } = biomes.Register(new BiomeDefinition(nameof(OceanicIceSheet))
        {
            Color = ColorS.LightGray,
            SubBiomes = [(subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(OceanicIceSheet), "Floor")), 1)],
            OceanicSubBiomes =
            [
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(OceanicIceSheet), "Snowy")), 4),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(OceanicIceSheet), "Bare")), 3)
            ]
        });

        /// <summary>
        ///     The mountain biome. It is a special biome that depends on the height of the terrain.
        /// </summary>
        public BiomeDefinition Mountains { get; } = biomes.Register(new BiomeDefinition(nameof(Mountains))
        {
            Color = ColorS.Black,
            SubBiomes =
            [
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Mountains), "Smooth")), 5),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Mountains), "Rough")), 5),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Mountains), "Green")), 1)
            ]
        });

        /// <summary>
        ///     The beach biome. It is found at low heights next to coastlines.
        /// </summary>
        public BiomeDefinition Beach { get; } = biomes.Register(new BiomeDefinition(nameof(Beach))
        {
            Color = ColorS.Orange,
            SubBiomes =
            [
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Beach), "Default")), 1),
                (subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(Beach), "Palms")), 1)
            ]
        });

        /// <summary>
        ///     The grass covered cliff biome, which is found at large height differences.
        /// </summary>
        public BiomeDefinition GrassyCliff { get; } = biomes.Register(new BiomeDefinition(nameof(GrassyCliff))
        {
            Color = ColorS.LightGray,
            SubBiomes = [(subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(GrassyCliff), "")), 1)]
        });

        /// <summary>
        ///     The sand covered cliff biome, which is found at large height differences.
        /// </summary>
        public BiomeDefinition SandyCliff { get; } = biomes.Register(new BiomeDefinition(nameof(SandyCliff))
        {
            Color = ColorS.SlateGray,
            SubBiomes = [(subBiomes.GetSubBiomeDefinition(GetSubBiomeRID(nameof(SandyCliff), "")), 1)]
        });

        private static RID GetSubBiomeRID(String biome, String subBiome)
        {
            return RID.Named<SubBiomeDefinition>($"{biome}{subBiome}");
        }
    }
}
