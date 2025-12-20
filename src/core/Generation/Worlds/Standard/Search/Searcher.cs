// <copyright file="Searcher.cs" company="VoxelGame">
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
using OpenTK.Mathematics;
using VoxelGame.Core.Generation.Worlds.Standard.Biomes;
using VoxelGame.Core.Generation.Worlds.Standard.Structures;
using VoxelGame.Core.Generation.Worlds.Standard.SubBiomes;

namespace VoxelGame.Core.Generation.Worlds.Standard.Search;

/// <summary>
///     Starting point for all searches for generated things in the world.
/// </summary>
/// <param name="generator">The generator that owns this searcher.</param>
public class Searcher(Generator generator)
{
    private readonly Dictionary<String, SearchCategory> categories = new();

    /// <summary>
    ///     Get the generator that owns this searcher.
    /// </summary>
    public Generator Generator { get; } = generator;

    internal void InitializeSearch(
        Dictionary<String, StructureGenerator> structures,
        Dictionary<String, SubBiome> subBiomes,
        Dictionary<String, Biome> biomes)
    {
        BiomeSearch biomeSearch = new(biomes, this);
        SubBiomeSearch subBiomeSearch = new(subBiomes, this, biomes.Values, biomeSearch);
        StructureSearch structureSearch = new(structures, this, subBiomes.Values, subBiomeSearch);

        categories["Biome"] = biomeSearch;
        categories["SubBiome"] = subBiomeSearch;
        categories["Structure"] = structureSearch;
    }

    /// <summary>
    ///     Perform a search for a named thing in the world.
    /// </summary>
    /// <param name="start">The starting point of the search.</param>
    /// <param name="name">The name of the thing to search for.</param>
    /// <param name="maxDistance">The maximum distance to search from the starting point.</param>
    /// <returns>The thread-safe lazy enumerable of the found things.</returns>
    public IEnumerable<Vector3i>? Search(Vector3i start, String name, UInt32 maxDistance)
    {
        return ParseName(name) is {} parsed
            ? categories.GetValueOrDefault(parsed.category)?.Search(start, parsed.entity, parsed.modifier, maxDistance)
            : null;
    }

    private static (String category, String entity, String? modifier)? ParseName(String name)
    {
        String? category = null;
        String? entity = null;
        String? modifier = null;

        foreach (Range range in name.AsSpan().Split("/"))
        {
            String part = name[range];

            if (category == null)
                category = part;
            else if (entity == null)
                entity = part;
            else if (modifier == null)
                modifier = part;
            else
                return null;
        }

        if (category == null || entity == null)
            return null;

        return (category, entity, modifier);
    }
}
