// <copyright file="SubBiomeSearch.cs" company="VoxelGame">
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
using System.Linq;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Generation.Worlds.Standard.Biomes;
using VoxelGame.Core.Generation.Worlds.Standard.SubBiomes;

namespace VoxelGame.Core.Generation.Worlds.Standard.Search;

/// <summary>
///     Searches for sub-biomes in the world.
/// </summary>
public class SubBiomeSearch(Dictionary<String, SubBiome> subBiomes, Searcher searcher, ICollection<Biome> biomes, BiomeSearch biomeSearch)
    : SearchCategory<SubBiome>(subBiomes, [], searcher)
{
    private const Int32 InCellSearchDistanceInGridCells = Map.CellSize / Map.SubBiomeGridSize + 1;

    private readonly Dictionary<SubBiome, IReadOnlySet<Biome>> subBiomesToBiomes = biomes
        .SelectMany(biome => biome.SubBiomes.Select(subBiome => (biome, subBiome))
            .Concat(biome.OceanicSubBiomes.Select(subBiome => (biome, subBiome))))
        .GroupBy(pair => pair.subBiome, pair => pair.biome)
        .ToDictionary(group => group.Key, Set.Of<Biome>);

    /// <inheritdoc />
    protected override IEnumerable<Vector3i> SearchElement(SubBiome element, String? modifier, Vector3i start, UInt32 maxBlockDistance)
    {
        foreach (Vector3i cell in biomeSearch.SearchBiomes(subBiomesToBiomes[element], BiomeSearch.Mode.Inner, start, maxBlockDistance))
            for (var distance = 0; distance < InCellSearchDistanceInGridCells; distance++)
                foreach (Vector3i position in SearchAtDistance(Set.Of(element), cell, distance))
                    yield return position;
    }

    /// <summary>
    ///     Search for sub-biomes in the world.
    /// </summary>
    /// <param name="subBiomes">The set of sub-biomes to search for. Must not be empty.</param>
    /// <param name="start">The position at which to start the search.</param>
    /// <param name="maxBlockDistance">The maximum distance from the start position to search.</param>
    /// <returns>Positions at which the sub-biomes are found.</returns>
    public IEnumerable<Vector3i> SearchSubBiomes(IReadOnlySet<SubBiome> subBiomes, Vector3i start, UInt32 maxBlockDistance)
    {
        IReadOnlySet<Biome> containing;

        if (subBiomes.Count == 1) containing = subBiomesToBiomes[subBiomes.First()];
        else
            containing = subBiomes
                .SelectMany(subBiome => subBiomesToBiomes[subBiome])
                .Distinct()
                .ToHashSet();

        foreach (Vector3i cell in biomeSearch.SearchBiomes(containing, BiomeSearch.Mode.Inner, start, maxBlockDistance))
            for (var distance = 0; distance < InCellSearchDistanceInGridCells; distance++)
                foreach (Vector3i position in SearchAtDistance(subBiomes, cell, distance))
                    yield return position;
    }

    private IEnumerable<Vector3i> SearchAtDistance(IReadOnlySet<SubBiome> subBiomes, Vector3i anchor, Int32 distance)
    {
        Vector2i center = Map.GetSubBiomeGridCellFromColumn(anchor.Xz);

        for (Int32 dx = -distance; dx <= distance; dx++)
        {
            Int32 dz = -distance;

            while (dz <= distance)
            {
                if (Math.Abs(dx) != distance && Math.Abs(dz) != distance)
                {
                    dz = distance;

                    continue;
                }

                Vector2i current = center + new Vector2i(dx, dz) * Map.SubBiomeGridSize;

                if (!Map.IsValidSubBiomeGridCell(current)) continue;

                if (SearchInSubBiomeGridCell(subBiomes, current, out Vector3i found))
                    yield return found;

                dz++;
            }
        }
    }

    private Boolean SearchInSubBiomeGridCell(IReadOnlySet<SubBiome> subBiomes, Vector2i current, out Vector3i found)
    {
        found = Map.GetSubBiomeGridCellCenter(current, y: 0);
        Map.Sample sample = Generator.Map.GetSample(found);

        if (subBiomes.Contains(sample.ActualSubBiome))
        {
            found.Y = sample.GroundHeight;

            return true;
        }

        if (sample.ActualOceanicSubBiome != null && subBiomes.Contains(sample.ActualOceanicSubBiome))
        {
            found.Y = sample.OceanicHeight;

            return true;
        }

        return false;
    }
}
