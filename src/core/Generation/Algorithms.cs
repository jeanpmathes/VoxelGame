// <copyright file="Algorithms.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
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

    /// <summary>
    ///     Merge nodes of a graph represented by an adjacency list.
    /// </summary>
    /// <param name="original">The original adjacency list.</param>
    /// <param name="merge">A function giving the containing node id for a node id of the original.</param>
    /// <returns>The merged adjacency list.</returns>
    public static (List<short>, Dictionary<short, List<short>>) MergeAdjacencyList(List<List<short>> original, Func<short, short> merge)
    {
        Dictionary<short, HashSet<short>> adjacencyHashed = new();

        for (short current = 0; current < original.Count; current++)
        {
            short mergedCurrent = merge(current);

            if (!adjacencyHashed.ContainsKey(mergedCurrent)) adjacencyHashed.Add(mergedCurrent, new HashSet<short>());

            HashSet<short> currentSet = adjacencyHashed[mergedCurrent];

            foreach (short neighbor in original[current])
            {
                short mergedNeighbor = merge(neighbor);

                if (mergedCurrent != mergedNeighbor) currentSet.Add(mergedNeighbor);
            }
        }

        List<short> nodes = adjacencyHashed.Keys.OrderBy(key => key).ToList();
        Dictionary<short, List<short>> adjacency = adjacencyHashed.ToDictionary(pair => pair.Key, pair => new List<short>(pair.Value));

        return (nodes, adjacency);
    }

    /// <summary>
    ///     Create a new list where each element is appended some data from a dictionary.
    /// </summary>
    /// <param name="list">The list to append data to.</param>
    /// <param name="dictionary">The dictionary to get data from.</param>
    /// <returns>The new list.</returns>
    public static List<(TA, TB)> AppendData<TA, TB>(IEnumerable<TA> list, IDictionary<TA, TB> dictionary) where TA : notnull
    {

        return list.Select(element => (element, dictionary[element])).ToList();
    }
}
