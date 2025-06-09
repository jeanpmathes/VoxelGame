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
///     A biome distribution is a collection of biomes and their distribution in the world.
///     It combines the defined distribution of a <see cref="BiomeDistributionDefinition" />
///     with the actual biome instances.
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
    private readonly Biome continentalIceSheet;
    private readonly Biome oceanicIceSheet;
    private readonly Biome ocean;
    private readonly Biome mountain;

    /// <summary>
    ///     Create a new biome distribution.
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
        continentalIceSheet = biomeMap[definition.ContinentalIceSheet];
        oceanicIceSheet = biomeMap[definition.OceanicIceSheet];
        ocean = biomeMap[definition.Ocean];
        mountain = biomeMap[definition.Mountain];
    }

    /// <summary>
    /// Determine the biome for a location based on the given conditions.
    /// </summary>
    /// <param name="conditions">The special cell conditions in effect.</param>
    /// <param name="temperature">The temperature, which must be in the range [0, 1].</param>
    /// <param name="humidity">The humidity, which must be in the range [0, 1].</param>
    /// <param name="isLand">Whether the location is land or water.</param>
    /// <returns>The appropriate biome.</returns>
    public Biome DetermineBiome(Map.CellConditions conditions, Single temperature, Single humidity, Boolean isLand)
    {
        if (!isLand)
            return DetermineOceanBiome(temperature, humidity);

        if (conditions.HasFlag(Map.CellConditions.Mountainous))
            return mountain;

        if (Map.HasCliff(conditions))
            return DetermineCliffBiome(temperature, humidity);

        if (conditions.HasFlag(Map.CellConditions.Coastline))
            return beach;

        return DetermineBiome(temperature, humidity);
    }

    private Biome DetermineBiome(Single temperature, Single humidity)
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

    private Biome DetermineCliffBiome(Single temperature, Single humidity)
    {
        Biome biome = DetermineBiome(temperature, humidity);

        return biome == desert ? sandyCliff : grassyCliff;
    }

    private Biome DetermineOceanBiome(Single temperature, Single humidity)
    {
        Biome biome = DetermineBiome(temperature, humidity);

        if (biome == polarDesert)
            return polarOcean;

        if (biome == continentalIceSheet)
            return oceanicIceSheet;

        return ocean;
    }
}
