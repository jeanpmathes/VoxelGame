// <copyright file="Biome.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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
using VoxelGame.Core.Generation.Worlds.Standard.SubBiomes;

namespace VoxelGame.Core.Generation.Worlds.Standard.Biomes;

/// <summary>
///     Combines a biome definition with a noise generator.
/// </summary>
public sealed class Biome : IDisposable
{
    private readonly List<(SubBiome, Single)> oceanicSubBiomes;
    private readonly List<(SubBiome, Single)> subBiomes;

    /// <summary>
    ///     Create a new biome.
    /// </summary>
    /// <param name="definition">The definition of the biome.</param>
    /// <param name="subBiomeMap">Mapping from sub-biome definitions to sub-biomes generators.</param>
    public Biome(BiomeDefinition definition, IReadOnlyDictionary<SubBiomeDefinition, SubBiome> subBiomeMap)
    {
        Definition = definition;

        subBiomes = SetUpSubBiomes(definition.SubBiomes, subBiomeMap);
        oceanicSubBiomes = definition.IsOceanic ? SetUpSubBiomes(definition.OceanicSubBiomes, subBiomeMap) : [];
    }

    /// <summary>
    ///     The definition of the biome.
    /// </summary>
    public BiomeDefinition Definition { get; }

    /// <summary>
    ///     Get all sub-biomes used by this biome.
    /// </summary>
    public IEnumerable<SubBiome> SubBiomes
    {
        get
        {
            foreach ((SubBiome subBiome, _) in subBiomes)
                yield return subBiome;
        }
    }

    /// <summary>
    ///     Get all oceanic sub-biomes used by this biome.
    /// </summary>
    public IEnumerable<SubBiome> OceanicSubBiomes
    {
        get
        {
            foreach ((SubBiome subBiome, _) in oceanicSubBiomes)
                yield return subBiome;
        }
    }

    #region DISPOSABLE

    /// <inheritdoc />
    public void Dispose()
    {
        // Sub-biomes are disposed by the map class.
    }

    #endregion DISPOSABLE

    private static List<(SubBiome, Single)> SetUpSubBiomes(IReadOnlyList<(SubBiomeDefinition, Int32)> subBiomes, IReadOnlyDictionary<SubBiomeDefinition, SubBiome> subBiomeMap)
    {
        Single tickets = 0;

        foreach ((_, Int32 count) in subBiomes) tickets += count;

        Debug.Assert(tickets > 0);

        List<(SubBiome, Single)> result = [];

        Single sum = 0;

        foreach ((SubBiomeDefinition subBiomeDefinition, Int32 count) in subBiomes)
        {
            SubBiome subBiome = subBiomeMap[subBiomeDefinition];

            sum += count / tickets;

            result.Add((subBiome, sum));
        }

        return result;
    }

    /// <summary>
    ///     Choose a sub-biome based on a value.
    /// </summary>
    /// <param name="value">A value between 0 and 1.</param>
    /// <returns>The chosen sub-biome.</returns>
    public SubBiome ChooseSubBiome(Single value)
    {
        return ChooseSubBiome(subBiomes, value);
    }

    /// <summary>
    ///     Choose an oceanic sub-biome based on a value.
    /// </summary>
    /// <param name="value">A value between 0 and 1.</param>
    /// <returns>The chosen sub-biome.</returns>
    public SubBiome ChooseOceanicSubBiome(Single value)
    {
        return ChooseSubBiome(oceanicSubBiomes, value);
    }

    private static SubBiome ChooseSubBiome(List<(SubBiome, Single)> subBiomes, Single value)
    {
        Debug.Assert(value is >= 0 and <= 1);

        foreach ((SubBiome subBiome, Single threshold) in subBiomes)
            if (value < threshold)
                return subBiome;

        (SubBiome last, _) = subBiomes[^1];

        return last;
    }
}
