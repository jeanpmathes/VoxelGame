// <copyright file="BiomeDistributionDefinition.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Toolkit.Collections;

namespace VoxelGame.Core.Generation.Worlds.Standard.Biomes;

/// <summary>
///     The distribution of biomes according to temperature and humidity.
/// </summary>
public sealed class BiomeDistributionDefinition : IResource
{
    /// <summary>
    ///     The resolution of the biome distribution map.
    /// </summary>
    public const Int32 Resolution = 20;

    private readonly Array2D<BiomeDefinition?> distribution;

    /// <summary>
    ///     Create a new biome distribution.
    /// </summary>
    /// <param name="distribution">The distribution of biomes.</param>
    public BiomeDistributionDefinition(Array2D<BiomeDefinition?> distribution)
    {
        Debug.Assert(distribution.Length == Resolution);

        this.distribution = distribution;
    }

    /// <summary>
    ///     The mountain biome.
    /// </summary>
    public required BiomeDefinition Mountain { get; init; }

    /// <summary>
    ///     The beach biome.
    /// </summary>
    public required BiomeDefinition Beach { get; init; }

    /// <summary>
    ///     The desert biome.
    /// </summary>
    public required BiomeDefinition Desert { get; init; }

    /// <summary>
    ///     The grassy cliff biome.
    /// </summary>
    public required BiomeDefinition GrassyCliff { get; init; }

    /// <summary>
    ///     The sandy cliff biome.
    /// </summary>
    public required BiomeDefinition SandyCliff { get; init; }

    /// <summary>
    ///     The ocean biome.
    /// </summary>
    public required BiomeDefinition Ocean { get; init; }

    /// <summary>
    ///     The polar desert biome.
    /// </summary>
    public required BiomeDefinition PolarDesert { get; init; }

    /// <summary>
    ///     The polar ocean biome.
    /// </summary>
    public required BiomeDefinition PolarOcean { get; init; }

    /// <summary>
    ///     The continental ice sheet biome.
    /// </summary>
    public required BiomeDefinition ContinentalIceSheet { get; init; }

    /// <summary>
    ///     The oceanic ice sheet biome.
    /// </summary>
    public required BiomeDefinition OceanicIceSheet { get; init; }

    /// <inheritdoc />
    public RID Identifier { get; } = RID.Named<BiomeDistributionDefinition>("Default");

    /// <inheritdoc />
    public ResourceType Type => ResourceTypes.BiomeDistribution;

    #region DISPOSABLE

    /// <inheritdoc />
    public void Dispose()
    {
        // Nothing to dispose.
    }

    #endregion DISPOSABLE

    /// <summary>
    ///     Get the biome distribution.
    /// </summary>
    /// <param name="biomeMap">A map from biome definitions to biomes.</param>
    /// <returns>The biome distribution.</returns>
    public Array2D<Biome?> GetDistribution(IReadOnlyDictionary<BiomeDefinition, Biome> biomeMap)
    {
        var result = new Array2D<Biome?>(Resolution);

        for (var x = 0; x < Resolution; x++)
        for (var y = 0; y < Resolution; y++)
        {
            BiomeDefinition? biomeDefinition = distribution[x, y];

            if (biomeDefinition == null) continue;

            result[x, y] = biomeMap[biomeDefinition];
        }

        return result;
    }
}
