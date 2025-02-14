// <copyright file="BiomeSearch.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Generation.Worlds.Default.Biomes;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Generation.Worlds.Default.Search;

/// <summary>
///     Searches for biomes in the world.
/// </summary>
public class BiomeSearch(Dictionary<String, Biome> biomes, Searcher searcher) : SearchCategory<Biome>(biomes, searcher)
{
    /// <inheritdoc />
    protected override Int32 ConvertDistance(UInt32 blockDistance)
    {
        return (Int32) Math.Clamp(blockDistance / Map.CellSize + 1, min: 0, 2 * World.SectionLimit);
    }

    /// <inheritdoc />
    protected override IEnumerable<Vector3i> SearchAtDistance(Biome element, Vector3i anchor, Int32 distance)
    {
        Vector2i center = Map.GetCellIndex(anchor);

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

                Vector2i current = center + (dx, dz);

                if (!Map.IsInLimits(current)) continue;

                if (SearchInCell(element, current, out Vector3i found))
                    yield return found;

                dz++;
            }
        }
    }

    private Boolean SearchInCell(Biome biome, Vector2i currentCell, out Vector3i found)
    {
        Vector3i center = Map.GetCellCenter(currentCell, y: 0);
        Int32 y = Generator.GetWorldHeight(center);

        found = center with {Y = y};

        Map.Sample sample = Generator.Map.GetSample(center with {Y = y});

        return sample.ActualBiome == biome;
    }
}
