﻿// <copyright file="BiomeDistribution.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Diagnostics;

namespace VoxelGame.Core.Generation.Default;

/// <summary>
///     The distribution of biomes according to temperature and humidity.
/// </summary>
public class BiomeDistribution
{
    private const int Resolution = 10;
    private readonly Biomes biomes;
    private readonly Biome?[,] distribution;

    private BiomeDistribution(Biomes biomes)
    {
        this.biomes = biomes;

        distribution = new[,]
        {
            {biomes.PolarDesert, biomes.Tundra, biomes.Taiga, biomes.Grassland, biomes.Grassland, biomes.Grassland, biomes.Desert, biomes.Desert, biomes.Desert, biomes.Desert},
            {null, biomes.Tundra, biomes.Taiga, biomes.Shrubland, biomes.Shrubland, biomes.Shrubland, biomes.Shrubland, biomes.Savanna, biomes.Desert, biomes.Desert},
            {null, null, biomes.Taiga, biomes.SeasonalForest, biomes.SeasonalForest, biomes.SeasonalForest, biomes.Shrubland, biomes.Savanna, biomes.Savanna, biomes.Savanna},
            {null, null, null, biomes.SeasonalForest, biomes.SeasonalForest, biomes.SeasonalForest, biomes.SeasonalForest, biomes.DryForest, biomes.DryForest, biomes.DryForest},
            {null, null, null, null, biomes.SeasonalForest, biomes.SeasonalForest, biomes.SeasonalForest, biomes.DryForest, biomes.DryForest, biomes.DryForest},
            {null, null, null, null, null, biomes.TemperateRainforest, biomes.TemperateRainforest, biomes.TropicalRainforest, biomes.DryForest, biomes.DryForest},
            {null, null, null, null, null, null, biomes.TemperateRainforest, biomes.TropicalRainforest, biomes.TropicalRainforest, biomes.DryForest},
            {null, null, null, null, null, null, null, biomes.TropicalRainforest, biomes.TropicalRainforest, biomes.TropicalRainforest},
            {null, null, null, null, null, null, null, null, biomes.TropicalRainforest, biomes.TropicalRainforest},
            {null, null, null, null, null, null, null, null, null, biomes.TropicalRainforest}
        };
    }

    /// <summary>
    ///     Get the default biome distribution.
    /// </summary>
    public static BiomeDistribution CreateDefault(Biomes biomes)
    {
        return new BiomeDistribution(biomes);
    }

    /// <summary>
    ///     Get the biome at the given temperature and humidity.
    /// </summary>
    /// <param name="temperature">The temperature, must be in the range [0, 1].</param>
    /// <param name="humidity">The humidity, must be in the range [0, 1].</param>
    /// <returns>The biome at the given temperature and humidity.</returns>
    public Biome GetBiome(float temperature, float humidity)
    {
        Debug.Assert(temperature is >= 0 and <= 1);
        Debug.Assert(humidity is >= 0 and <= 1);

        humidity = Math.Clamp(humidity, min: 0, temperature);

        var x = (int) Math.Floor(humidity * Resolution);
        var y = (int) Math.Floor(temperature * Resolution);

        x = Math.Clamp(x, min: 0, Resolution - 1);
        y = Math.Clamp(y, min: 0, Resolution - 1);

        Biome? biome = distribution[x, y];

        Debug.Assert(biome is not null);

        return biome;
    }

    /// <summary>
    ///     Get the appropriate coastline biome.
    /// </summary>
    /// <param name="temperature">The temperature, must be in the range [0, 1].</param>
    /// <param name="humidity">The humidity, must be in the range [0, 1].</param>
    /// <param name="isCliff">Whether the coastline is a cliff.</param>
    /// <returns>The appropriate coastline biome.</returns>
    public Biome GetCoastlineBiome(float temperature, float humidity, bool isCliff)
    {
        if (!isCliff) return biomes.Beach;

        Biome biome = GetBiome(temperature, humidity);

        return biome == biomes.Desert ? biomes.SandyCliff : biomes.GrassyCliff;
    }

    /// <summary>
    ///     Get the appropriate ocean biome.
    /// </summary>
    /// <param name="temperature">The temperature, must be in the range [0, 1].</param>
    /// <param name="humidity">The humidity, must be in the range [0, 1].</param>
    /// <returns>The appropriate ocean biome.</returns>
    public Biome GetOceanBiome(float temperature, float humidity)
    {
        Biome biome = GetBiome(temperature, humidity);

        return biome == biomes.PolarDesert ? biomes.PolarOcean : biomes.Ocean;
    }

    /// <summary>
    ///     Get the mountain biome.
    /// </summary>
    public Biome GetMountainBiome()
    {
        return biomes.Mountains;
    }
}
