// <copyright file="BiomeDistribution.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using VoxelGame.Toolkit.Collections;

namespace VoxelGame.Core.Generation.Worlds.Default.Biomes;

/// <summary>
/// A biome distribution is a collection of biomes and their distribution in the world.
/// It combines the defined distribution of a <see cref="BiomeDistributionDefinition"/>
/// with the actual biome instances.
/// </summary>
public class BiomeDistribution
{
    private readonly Array2D<Biome?> distribution;

    private readonly Biome beach;
    private readonly Biome desert;
    private readonly Biome sandyCliff;
    private readonly Biome grassyCliff;
    private readonly Biome polarDesert;
    private readonly Biome polarOcean;
    private readonly Biome ocean;
    private readonly Biome mountain;

    /// <summary>
    /// Create a new biome distribution.
    /// </summary>
    /// <param name="definition">The definition of the biome distribution.</param>
    /// <param name="biomeMap">A map from biome definitions to biomes.</param>
    public BiomeDistribution(
        BiomeDistributionDefinition definition,
        IReadOnlyDictionary<BiomeDefinition, Biome> biomeMap)
    {
        distribution = definition.GetDistribution(biomeMap);

        beach = biomeMap[definition.Beach];
        desert = biomeMap[definition.Desert];
        sandyCliff = biomeMap[definition.SandyCliff];
        grassyCliff = biomeMap[definition.GrassyCliff];
        polarDesert = biomeMap[definition.PolarDesert];
        polarOcean = biomeMap[definition.PolarOcean];
        ocean = biomeMap[definition.Ocean];
        mountain = biomeMap[definition.Mountain];
    }

    /// <summary>
    ///     Get the biome at the given temperature and humidity.
    /// </summary>
    /// <param name="temperature">The temperature, must be in the range [0, 1].</param>
    /// <param name="humidity">The humidity, must be in the range [0, 1].</param>
    /// <returns>The biome at the given temperature and humidity.</returns>
    public Biome GetBiome(Single temperature, Single humidity)
    {
        Debug.Assert(temperature is >= 0 and <= 1);
        Debug.Assert(humidity is >= 0 and <= 1);

        humidity = Math.Clamp(humidity, min: 0, temperature);

        var x = (Int32) Math.Floor(humidity * BiomeDistributionDefinition.Resolution);
        var y = (Int32) Math.Floor(temperature * BiomeDistributionDefinition.Resolution);

        x = Math.Clamp(x, min: 0, BiomeDistributionDefinition.Resolution - 1);
        y = Math.Clamp(y, min: 0, BiomeDistributionDefinition.Resolution - 1);

        Biome? biome = distribution[x, y];

        Debug.Assert(biome is not null);

        return biome;
    }

    /// <summary>
    ///     Get the appropriate mountain biome.
    /// </summary>
    /// <returns></returns>
    #pragma warning disable S4049 // For symmetry with GetCoastlineBiome and GetOceanBiome
    public Biome GetMountainBiome()
    #pragma warning restore S4049
    {
        return mountain;
    }

    /// <summary>
    ///     Get the appropriate coastline biome.
    /// </summary>
    /// <param name="temperature">The temperature, must be in the range [0, 1].</param>
    /// <param name="humidity">The humidity, must be in the range [0, 1].</param>
    /// <param name="isCliff">Whether the coastline is a cliff.</param>
    /// <returns>The appropriate coastline biome.</returns>
    public Biome GetCoastlineBiome(Single temperature, Single humidity, Boolean isCliff)
    {
        if (!isCliff) return beach;

        Biome biome = GetBiome(temperature, humidity);

        return biome == desert ? sandyCliff : grassyCliff;
    }

    /// <summary>
    ///     Get the appropriate ocean biome.
    /// </summary>
    /// <param name="temperature">The temperature, must be in the range [0, 1].</param>
    /// <param name="humidity">The humidity, must be in the range [0, 1].</param>
    /// <returns>The appropriate ocean biome.</returns>
    public Biome GetOceanBiome(Single temperature, Single humidity)
    {
        Biome biome = GetBiome(temperature, humidity);

        return biome == polarDesert ? polarOcean : ocean;
    }
}
