// <copyright file="Biome.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Drawing;

namespace VoxelGame.Core.Generation.Default;

/// <summary>
///     A biome is an are with a certain fauna and flora.
/// </summary>
public enum Biome
{
    /// <summary>
    ///     Polar desert.
    /// </summary>
    PolarDesert,

    /// <summary>
    ///     Tropical rainforest.
    /// </summary>
    TropicalRainforest,

    /// <summary>
    ///     Temperate rainforest.
    /// </summary>
    TemperateRainforest,

    /// <summary>
    ///     Taiga.
    /// </summary>
    Taiga,

    /// <summary>
    ///     Tundra.
    /// </summary>
    Tundra,

    /// <summary>
    ///     Savanna.
    /// </summary>
    Savanna,

    /// <summary>
    ///     Seasonal forest.
    /// </summary>
    Forest,

    /// <summary>
    ///     Shrubland.
    /// </summary>
    Shrubland,

    /// <summary>
    ///     Desert.
    /// </summary>
    Desert,

    /// <summary>
    ///     Grassland.
    /// </summary>
    Grassland
}

/// <summary>
///     Offers helper methods for working with biomes.
/// </summary>
public static class BiomeExtensions
{
    /// <summary>
    ///     Gets a color representing the biome.
    /// </summary>
    public static Color GetColor(this Biome biome)
    {
        return biome switch
        {
            Biome.TropicalRainforest => Color.DarkGreen,
            Biome.TemperateRainforest => Color.Green,
            Biome.Taiga => Color.Navy,
            Biome.Tundra => Color.CadetBlue,
            Biome.Savanna => Color.Olive,
            Biome.Forest => Color.LimeGreen,
            Biome.Shrubland => Color.Salmon,
            Biome.Desert => Color.Yellow,
            Biome.Grassland => Color.SaddleBrown,
            Biome.PolarDesert => Color.Gray,
            _ => throw new ArgumentException(message: null, nameof(biome))
        };
    }
}
