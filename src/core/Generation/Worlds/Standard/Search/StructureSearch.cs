// <copyright file="StructureSearch.cs" company="VoxelGame">
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
using VoxelGame.Core.Generation.Worlds.Standard.Structures;
using VoxelGame.Core.Generation.Worlds.Standard.SubBiomes;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Generation.Worlds.Standard.Search;

/// <summary>
///     Searches for structures in the world.
/// </summary>
public class StructureSearch(Dictionary<String, StructureGenerator> structures, Searcher searcher, ICollection<SubBiome> subBiomes, SubBiomeSearch subBiomeSearch)
    : SearchCategory<StructureGenerator>(structures, [], searcher)
{
    private const Int32 InSubBiomeGridCellSearchDistanceInSections = Map.SubBiomeGridSize / Section.Size + 1;

    private readonly Dictionary<StructureGenerator, IReadOnlySet<SubBiome>> structureToSubBiomes = subBiomes
        .Where(subBiome => subBiome.Structure != null)
        .GroupBy(subBiome => subBiome.Structure!)
        .ToDictionary(group => group.Key, Set.Of<SubBiome>);

    /// <inheritdoc />
    protected override IEnumerable<Vector3i> SearchElement(StructureGenerator element, String? modifier, Vector3i start, UInt32 maxBlockDistance)
    {
        foreach (Vector3i cell in subBiomeSearch.SearchSubBiomes(structureToSubBiomes[element], start, maxBlockDistance))
            for (var distance = 0; distance < InSubBiomeGridCellSearchDistanceInSections; distance++)
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

                current = SectionPosition.From(current.FirstBlock with {Y = Generator.GetGroundHeight(current.FirstBlock)});

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

        return Generator.IsStructurePlacementAllowed(current, out StructureGenerator? sectionStructure, store: null)
               && sectionStructure == structure
               && structure.CheckPlacement(current, Generator, out found);
    }
}
