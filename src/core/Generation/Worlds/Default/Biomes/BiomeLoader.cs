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
            SubBiome = subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>(nameof(PolarDesert)))
        });

        /// <summary>
        ///     The tropical rainforest biome.
        /// </summary>
        public BiomeDefinition TropicalRainforest { get; } = biomes.Register(new BiomeDefinition(nameof(TropicalRainforest))
        {
            Color = ColorS.DarkGreen,
            SubBiome = subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>(nameof(TropicalRainforest)))
        });

        /// <summary>
        ///     The temperate rainforest biome.
        /// </summary>
        public BiomeDefinition TemperateRainforest { get; } = biomes.Register(new BiomeDefinition(nameof(TemperateRainforest))
        {
            Color = ColorS.Green,
            SubBiome = subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>(nameof(TemperateRainforest)))
        });

        /// <summary>
        ///     The taiga biome.
        /// </summary>
        public BiomeDefinition Taiga { get; } = biomes.Register(new BiomeDefinition(nameof(Taiga))
        {
            Color = ColorS.Navy,
            SubBiome = subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>(nameof(Taiga)))
        });

        /// <summary>
        ///     The tundra biome.
        /// </summary>
        public BiomeDefinition Tundra { get; } = biomes.Register(new BiomeDefinition(nameof(Tundra))
        {
            Color = ColorS.CadetBlue,
            SubBiome = subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>(nameof(Tundra)))
        });

        /// <summary>
        ///     The savanna biome.
        /// </summary>
        public BiomeDefinition Savanna { get; } = biomes.Register(new BiomeDefinition(nameof(Savanna))
        {
            Color = ColorS.Olive,
            SubBiome = subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>(nameof(Savanna)))
        });

        /// <summary>
        ///     The seasonal forest biome.
        /// </summary>
        public BiomeDefinition SeasonalForest { get; } = biomes.Register(new BiomeDefinition(nameof(SeasonalForest))
        {
            Color = ColorS.Lime,
            SubBiome = subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>(nameof(SeasonalForest)))
        });

        /// <summary>
        ///     The dry forest biome.
        /// </summary>
        public BiomeDefinition DryForest { get; } = biomes.Register(new BiomeDefinition(nameof(DryForest))
        {
            Color = ColorS.SeaGreen,
            SubBiome = subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>(nameof(DryForest)))
        });

        /// <summary>
        ///     The shrubland biome.
        /// </summary>
        public BiomeDefinition Shrubland { get; } = biomes.Register(new BiomeDefinition(nameof(Shrubland))
        {
            Color = ColorS.Salmon,
            SubBiome = subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>(nameof(Shrubland)))
        });

        /// <summary>
        ///     The desert biome.
        /// </summary>
        public BiomeDefinition Desert { get; } = biomes.Register(new BiomeDefinition(nameof(Desert))
        {
            Color = ColorS.Yellow,
            SubBiome = subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>(nameof(Desert)))
        });

        /// <summary>
        ///     The grassland biome.
        /// </summary>
        public BiomeDefinition Grassland { get; } = biomes.Register(new BiomeDefinition(nameof(Grassland))
        {
            Color = ColorS.SaddleBrown,
            SubBiome = subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>(nameof(Grassland)))
        });

        /// <summary>
        ///     The normal ocean biome.
        /// </summary>
        public BiomeDefinition Ocean { get; } = biomes.Register(new BiomeDefinition(nameof(Ocean))
        {
            Color = ColorS.White,
            SubBiome = subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>(nameof(Ocean)))
        });

        /// <summary>
        ///     The polar ocean biome. It is covered in ice and occurs in cold regions.
        /// </summary>
        public BiomeDefinition PolarOcean { get; } = biomes.Register(new BiomeDefinition(nameof(PolarOcean))
        {
            Color = ColorS.White,
            SubBiome = subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>(nameof(PolarOcean)))
        });

        /// <summary>
        ///     The mountain biome. It is a special biome that depends on the height of the terrain.
        /// </summary>
        public BiomeDefinition Mountains { get; } = biomes.Register(new BiomeDefinition(nameof(Mountains))
        {
            Color = ColorS.Black,
            SubBiome = subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>(nameof(Mountains)))
        });

        /// <summary>
        ///     The beach biome. It is found at low heights next to coastlines.
        /// </summary>
        public BiomeDefinition Beach { get; } = biomes.Register(new BiomeDefinition(nameof(Beach))
        {
            Color = ColorS.Orange,
            SubBiome = subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>(nameof(Beach)))
        });

        /// <summary>
        ///     The grass covered cliff biome, which is found at large height differences.
        /// </summary>
        public BiomeDefinition GrassyCliff { get; } = biomes.Register(new BiomeDefinition(nameof(GrassyCliff))
        {
            Color = ColorS.LightGray,
            SubBiome = subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>(nameof(GrassyCliff)))
        });

        /// <summary>
        ///     The sand covered cliff biome, which is found at large height differences.
        /// </summary>
        public BiomeDefinition SandyCliff { get; } = biomes.Register(new BiomeDefinition(nameof(SandyCliff))
        {
            Color = ColorS.SlateGray,
            SubBiome = subBiomes.GetSubBiomeDefinition(RID.Named<SubBiomeDefinition>(nameof(SandyCliff)))
        });
    }
}
