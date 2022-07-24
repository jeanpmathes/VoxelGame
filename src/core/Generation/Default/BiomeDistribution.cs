// <copyright file="BiomeDistribution.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Diagnostics;

namespace VoxelGame.Core.Generation.Default;

/// <summary>
///     The distribution of biomes according to temperature and moisture.
/// </summary>
public class BiomeDistribution
{
    private const int Resolution = 10;
    private readonly Biome?[,] biomes;

    private BiomeDistribution()
    {
        biomes = new Biome?[,]
        {
            {Biome.PolarDesert, Biome.Tundra, Biome.Grassland, Biome.Grassland, Biome.Grassland, Biome.Grassland, Biome.Desert, Biome.Desert, Biome.Desert, Biome.Desert},
            {null, Biome.Tundra, Biome.Taiga, Biome.Shrubland, Biome.Shrubland, Biome.Shrubland, Biome.Shrubland, Biome.Savanna, Biome.Desert, Biome.Desert},
            {null, null, Biome.Taiga, Biome.Forest, Biome.Forest, Biome.Forest, Biome.Shrubland, Biome.Savanna, Biome.Savanna, Biome.Savanna},
            {null, null, null, Biome.Forest, Biome.Forest, Biome.Forest, Biome.Forest, Biome.Savanna, Biome.Savanna, Biome.Savanna},
            {null, null, null, null, Biome.Forest, Biome.Forest, Biome.Forest, Biome.Savanna, Biome.Savanna, Biome.Savanna},
            {null, null, null, null, null, Biome.TemperateRainforest, Biome.TemperateRainforest, Biome.TropicalRainforest, Biome.Savanna, Biome.Savanna},
            {null, null, null, null, null, null, Biome.TemperateRainforest, Biome.TropicalRainforest, Biome.TropicalRainforest, Biome.Savanna},
            {null, null, null, null, null, null, null, Biome.TropicalRainforest, Biome.TropicalRainforest, Biome.TropicalRainforest},
            {null, null, null, null, null, null, null, null, Biome.TropicalRainforest, Biome.TropicalRainforest},
            {null, null, null, null, null, null, null, null, null, Biome.TropicalRainforest}
        };
    }

    /// <summary>
    ///     Get the default biome distribution.
    /// </summary>
    public static BiomeDistribution Default => new();

    /// <summary>
    ///     Get the biome at the given temperature and moisture.
    /// </summary>
    /// <param name="temperature">The temperature, must be in the range [0, 1].</param>
    /// <param name="moisture">The moisture, must be in the range [0, 1].</param>
    /// <returns>The biome at the given temperature and moisture.</returns>
    public Biome GetBiome(float temperature, float moisture)
    {
        Debug.Assert(temperature is >= 0 and <= 1);
        Debug.Assert(moisture is >= 0 and <= 1);

        moisture = Math.Clamp(moisture, min: 0, temperature);

        var x = (int) Math.Floor(moisture * Resolution);
        var y = (int) Math.Floor(temperature * Resolution);

        x = Math.Clamp(x, min: 0, Resolution - 1);
        y = Math.Clamp(y, min: 0, Resolution - 1);

        Biome? biome = biomes[x, y];

        Debug.Assert(biome is not null);

        return biome.Value;
    }
}
