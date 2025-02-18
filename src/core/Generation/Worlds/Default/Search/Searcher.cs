// <copyright file="Searcher.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Generation.Worlds.Default.Biomes;
using VoxelGame.Core.Generation.Worlds.Default.Structures;

namespace VoxelGame.Core.Generation.Worlds.Default.Search;

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

    internal void InitializeSearch(Dictionary<String, StructureGenerator> structures, Dictionary<String, Biome> biomes)
    {
        BiomeSearch biomeSearch = new(biomes, this);
        StructureSearch structureSearch = new(structures, this, biomes.Values, biomeSearch);

        categories["Biome"] = biomeSearch;
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
