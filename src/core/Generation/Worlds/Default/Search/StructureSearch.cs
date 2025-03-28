// <copyright file="StructureSearch.cs" company="VoxelGame">
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
using VoxelGame.Core.Generation.Worlds.Default.Structures;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Generation.Worlds.Default.Search;

/// <summary>
///     Searches for structures in the world.
/// </summary>
public class StructureSearch(Dictionary<String, StructureGenerator> structures, Searcher searcher, ICollection<Biome> biomes, BiomeSearch biomeSearch)
    : SearchCategory<StructureGenerator>(structures, [], searcher)
{
    private const Int32 InCellSearchDistanceInSections = Map.CellSize / Section.Size + 1;

    private readonly Dictionary<StructureGenerator, IReadOnlySet<Biome>> structureToBiomes = biomes
        .Where(biome => biome.SubBiome.Structure != null)
        .ToDictionary(biome => biome.SubBiome.Structure!, Set.Of);

    /// <inheritdoc />
    protected override IEnumerable<Vector3i> SearchElement(StructureGenerator element, String? modifier, Vector3i start, UInt32 maxBlockDistance)
    {
        foreach (Vector3i cell in biomeSearch.SearchBiomes(structureToBiomes[element], BiomeSearch.Mode.Inner, start, maxBlockDistance))
            for (var distance = 0; distance < InCellSearchDistanceInSections; distance++)
                foreach (Vector3i position in SearchAtDistance(element, cell, distance))
                    yield return position;
    }

    private IEnumerable<Vector3i> SearchAtDistance(StructureGenerator structure, Vector3i anchor, Int32 distance)
    {
        return structure.Definition.Placement switch
        {
            StructureGeneratorDefinition.Kind.Surface => SearchAtDistanceOnSurface(structure, anchor, distance),
            StructureGeneratorDefinition.Kind.Underground => SearchAtDistanceUnderground(structure, anchor, distance),
            _ => throw Exceptions.UnsupportedEnumValue(structure.Definition.Placement)
        };
    }

    private IEnumerable<Vector3i> SearchAtDistanceOnSurface(StructureGenerator structure, Vector3i anchor, Int32 distance)
    {
        SectionPosition center = SectionPosition.From(anchor);

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

                SectionPosition current = center.Offset(dx, y: 0, dz);

                if (!World.IsInLimits(current)) continue;

                current = SectionPosition.From(current.FirstBlock with {Y = Generator.GetWorldHeight(current.FirstBlock)});

                if (SearchInSection(structure, current, out Vector3i found))
                    yield return found;

                dz++;
            }
        }
    }

    private IEnumerable<Vector3i> SearchAtDistanceUnderground(StructureGenerator structure, Vector3i anchor, Int32 distance)
    {
        SectionPosition center = SectionPosition.From(anchor);

        for (Int32 dx = -distance; dx <= distance; dx++)
        for (Int32 dy = -distance; dy <= distance; dy++)
            foreach (Vector3i position in SearchUndergroundRow(structure, center, distance, dx, dy))
                yield return position;
    }

    private IEnumerable<Vector3i> SearchUndergroundRow(StructureGenerator structure, SectionPosition section, Int32 distance, Int32 dx, Int32 dy)
    {
        Int32 dz = -distance;

        while (dz <= distance)
        {
            if (Math.Abs(dx) != distance && Math.Abs(dy) != distance && Math.Abs(dz) != distance)
            {
                dz = distance;

                continue;
            }

            SectionPosition current = section.Offset(dx, dy, dz);

            if (!World.IsInLimits(current)) continue;

            if (SearchInSection(structure, current, out Vector3i found))
                yield return found;

            dz++;
        }
    }

    private Boolean SearchInSection(StructureGenerator structure, SectionPosition current, out Vector3i found)
    {
        found = default;

        return FilterSectionByBiome(current, structure) && structure.CheckPlacement(current, Generator, out found);
    }

    private Boolean FilterSectionByBiome(SectionPosition section, StructureGenerator structure)
    {
        ICollection<Biome> sectionBiomes = Generator.GetSectionBiomes(section, columns: null);

        if (sectionBiomes.Count != 1) return false;

        return sectionBiomes.First().SubBiome.Structure == structure;
    }
}
