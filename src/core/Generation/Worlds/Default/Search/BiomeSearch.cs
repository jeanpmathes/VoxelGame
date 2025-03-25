// <copyright file="BiomeSearch.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Generation.Worlds.Default.Biomes;
using VoxelGame.Core.Logic;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Generation.Worlds.Default.Search;

/// <summary>
///     Searches for biomes in the world.
/// </summary>
public class BiomeSearch(Dictionary<String, Biome> biomes, Searcher searcher) : SearchCategory<Biome>(biomes, [InnerModifier, BorderModifier], searcher)
{
    /// <summary>
    ///     The search mode, determines how the search is performed and thus which positions are returned.
    /// </summary>
    public enum Mode
    {
        /// <summary>
        ///     Searches only at cell centers, which guarantees that the found positions are inside the biome.
        /// </summary>
        Inner,

        /// <summary>
        ///     Searches the lines between cell centers to find the border of the biome where a switch to another biome occurs.
        /// </summary>
        Border
    }

    private const String InnerModifier = "Inner";
    private const String BorderModifier = "Border";

    /// <inheritdoc />
    protected override IEnumerable<Vector3i> SearchElement(Biome element, String? modifier, Vector3i start, UInt32 maxBlockDistance)
    {
        Mode mode = modifier switch
        {
            InnerModifier => Mode.Inner,
            BorderModifier => Mode.Border,
            null => Mode.Inner,
            _ => throw Exceptions.UnsupportedValue(modifier)
        };

        return SearchBiomes(Set.Of(element), mode, start, maxBlockDistance);
    }

    /// <summary>
    ///     Searches for biomes in the world.
    /// </summary>
    /// <param name="biomes">The biomes to search for.</param>
    /// <param name="mode">The search mode.</param>
    /// <param name="start">The starting position.</param>
    /// <param name="maxBlockDistance">The maximum distance in blocks to search.</param>
    /// <returns>The found positions.</returns>
    public IEnumerable<Vector3i> SearchBiomes(IReadOnlySet<Biome> biomes, Mode mode, Vector3i start, UInt32 maxBlockDistance)
    {
        var maxCellDistance = (Int32) Math.Clamp(maxBlockDistance / Map.CellSize + 1, min: 0, 2 * World.SectionLimit);

        for (var distance = 0; distance < maxCellDistance; distance++)
            foreach (Vector3i position in SearchAtDistance(biomes, mode, start, distance))
                yield return position;
    }

    private IEnumerable<Vector3i> SearchAtDistance(IReadOnlySet<Biome> biomes, Mode mode, Vector3i anchor, Int32 distance)
    {
        Vector2i center = Map.GetCellFromColumn(anchor.Xz);

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

                if (!Map.IsValidCell(current)) continue;

                if (SearchInCell(biomes, mode, current, out Vector3i found))
                    yield return found;

                dz++;
            }
        }
    }

    private Boolean SearchInCell(IReadOnlySet<Biome> biomes, Mode mode, Vector2i currentCell, out Vector3i found)
    {
        Boolean inCenter = SearchCellCenter(biomes, currentCell, out found);

        if (!inCenter)
            return false;

        return mode switch
        {
            Mode.Inner => true,
            Mode.Border => SearchForBorder(biomes, currentCell, out found),
            _ => throw Exceptions.UnsupportedValue(mode)
        };
    }

    private Boolean SearchCellCenter(IReadOnlySet<Biome> biomes, Vector2i currentCell, out Vector3i found)
    {
        Vector3i center = Map.GetCellCenter(currentCell, y: 0);
        Int32 y = Generator.GetWorldHeight(center);

        found = center with {Y = y};

        Map.Sample sample = Generator.Map.GetSample(center with {Y = y});

        return biomes.Contains(sample.ActualBiome);
    }

    private Boolean SearchForBorder(IReadOnlySet<Biome> biomes, Vector2i currentCell, out Vector3i found)
    {
        if (CheckBorder(currentCell + (-1, 0), out found))
            return true;

        if (CheckBorder(currentCell + (0, -1), out found))
            return true;

        if (CheckBorder(currentCell + (1, 0), out found))
            return true;

        if (CheckBorder(currentCell + (0, 1), out found))
            return true;

        return false;

        Boolean CheckBorder(Vector2i neighborCell, out Vector3i found)
        {
            found = default;

            if (!Map.IsValidCell(neighborCell))
                return false;

            // The neighbor cell should contain a different biome.

            if (SearchCellCenter(biomes, neighborCell, out _))
                return false;

            return SearchAlongLine(biomes, currentCell, neighborCell, out found);
        }
    }

    private Boolean SearchAlongLine(IReadOnlySet<Biome> biomes, Vector2i from, Vector2i to, out Vector3i found)
    {
        Vector3i start = Map.GetCellCenter(from, y: 0);
        Vector3i end = Map.GetCellCenter(to, y: 0);

        while ((start - end).ManhattanLength > 5)
        {
            Vector3i mid = (start + end) / 2;

            if (biomes.Contains(GetBiome(mid))) start = mid;
            else end = mid;
        }

        found = start;

        return true;

        Biome GetBiome(Vector3i position)
        {
            Int32 y = Generator.GetWorldHeight(position);

            return Generator.Map.GetSample(position with {Y = y}).ActualBiome;
        }
    }
}
