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
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Generation.Worlds.Default.Search;

/// <summary>
///     Searches for biomes in the world.
/// </summary>
public class BiomeSearch(Dictionary<String, Biome> biomes, Searcher searcher) : SearchCategory<Biome>(biomes, [InnerModifier, BorderModifier], searcher)
{
    private const String InnerModifier = "Inner";
    private const String BorderModifier = "Border";

    /// <inheritdoc />
    protected override Int32 ConvertDistance(UInt32 blockDistance)
    {
        return (Int32) Math.Clamp(blockDistance / Map.CellSize + 1, min: 0, 2 * World.SectionLimit);
    }

    /// <inheritdoc />
    protected override IEnumerable<Vector3i> SearchAtDistance(Biome element, String? modifier, Vector3i anchor, Int32 distance)
    {
        Mode mode = modifier switch
        {
            InnerModifier => Mode.Inner,
            BorderModifier => Mode.Border,
            null => Mode.Inner,
            _ => throw Exceptions.UnsupportedValue(modifier)
        };

        return SearchAtDistance(element, mode, anchor, distance);
    }

    private IEnumerable<Vector3i> SearchAtDistance(Biome element, Mode mode, Vector3i anchor, Int32 distance)
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

                if (SearchInCell(element, mode, current, out Vector3i found))
                    yield return found;

                dz++;
            }
        }
    }

    private Boolean SearchInCell(Biome biome, Mode mode, Vector2i currentCell, out Vector3i found)
    {
        Boolean inCenter = SearchCellCenter(biome, currentCell, out found);

        if (!inCenter)
            return false;

        return mode switch
        {
            Mode.Inner => true,
            Mode.Border => SearchForBorder(biome, currentCell, out found),
            _ => throw Exceptions.UnsupportedValue(mode)
        };
    }

    private Boolean SearchCellCenter(Biome biome, Vector2i currentCell, out Vector3i found)
    {
        Vector3i center = Map.GetCellCenter(currentCell, y: 0);
        Int32 y = Generator.GetWorldHeight(center);

        found = center with {Y = y};

        Map.Sample sample = Generator.Map.GetSample(center with {Y = y});

        return sample.ActualBiome == biome;
    }

    private Boolean SearchForBorder(Biome biome, Vector2i currentCell, out Vector3i found)
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

            if (!Map.IsInLimits(neighborCell))
                return false;

            // The neighbor cell should contain a different biome.

            if (SearchCellCenter(biome, neighborCell, out _))
                return false;

            return SearchAlongLine(biome, currentCell, neighborCell, out found);
        }
    }

    private Boolean SearchAlongLine(Biome biome, Vector2i from, Vector2i to, out Vector3i found)
    {
        Vector3i start = Map.GetCellCenter(from, y: 0);
        Vector3i end = Map.GetCellCenter(to, y: 0);

        while ((start - end).ManhattanLength > 5)
        {
            Vector3i mid = (start + end) / 2;

            if (GetBiome(mid) == biome) start = mid;
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

    private enum Mode
    {
        Inner,
        Border
    }
}
