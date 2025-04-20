// <copyright file="SubBiomeSearch.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Generation.Worlds.Default.Biomes;
using VoxelGame.Core.Generation.Worlds.Default.SubBiomes;

namespace VoxelGame.Core.Generation.Worlds.Default.Search;

/// <summary>
///     Searches for sub-biomes in the world.
/// </summary>
public class SubBiomeSearch(Dictionary<String, SubBiome> subBiomes, Searcher searcher, ICollection<Biome> biomes, BiomeSearch biomeSearch)
    : SearchCategory<SubBiome>(subBiomes, [], searcher)
{
    private const Int32 InCellSearchDistanceInGridCells = Map.CellSize / Map.SubBiomeGridSize + 1;

    private readonly Dictionary<SubBiome, IReadOnlySet<Biome>> subBiomesToBiomes = biomes
        .SelectMany(biome => biome.SubBiomes.Select(subBiome => (biome, subBiome)))
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
        found.Y = Generator.GetWorldHeight(found);

        Map.Sample sample = Generator.Map.GetSample(found);

        return subBiomes.Contains(sample.ActualSubBiome);
    }
}
