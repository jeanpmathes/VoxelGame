// <copyright file="Algorithms.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using System.Linq;

namespace VoxelGame.Core.Generation;

#pragma warning disable S4017
#pragma warning disable S3956

/// <summary>
///     Some helpful and simple algorithms that are used in world generation.
/// </summary>
public static class Algorithms
{
    /// <summary>
    ///     Transform a hash-based adjacency list into a normal array based adjacency list.
    /// </summary>
    /// <param name="adjacencyHashed">The hash-based adjacency list.</param>
    /// <returns>The normal array based adjacency list.</returns>
    public static List<List<short>> BuildAdjacencyList(Dictionary<short, HashSet<short>> adjacencyHashed)
    {
        List<List<short>> adjacency = new();

        for (short id = 0; id < adjacencyHashed.Count; id++)
        {
            List<short> neighbors = new(adjacencyHashed[id]);
            neighbors.Sort();
            adjacency.Add(neighbors);
        }

        return adjacency;
    }

    /// <summary>
    ///     Invert a dictionary.
    /// </summary>
    public static Dictionary<TA, TB> InvertDictionary<TA, TB>(IEnumerable<KeyValuePair<TB, TA>> dictionary) where TA : notnull where TB : notnull
    {
        return dictionary.ToDictionary(pair => pair.Value, pair => pair.Key);
    }
}
