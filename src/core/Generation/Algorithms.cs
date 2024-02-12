// <copyright file="Algorithms.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;

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

            HashSet<short> currentSet = adjacencyHashed.GetOrAdd(mergedCurrent);

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

    /// <summary>
    ///     Traverse the cells of a 2D-cell-grid along a ray.
    /// </summary>
    /// <param name="start">The start cell.</param>
    /// <param name="direction">The direction of the ray.</param>
    /// <param name="length">The length of the ray.</param>
    /// <returns>The cells traversed by the ray.</returns>
    public static IEnumerable<Vector2i> TraverseCells(Vector2i start, Vector2d direction, double length)
    {
        double Frac0(double value)
        {
            return value - Math.Floor(value);
        }

        double Frac1(double value)
        {
            return 1 - value + Math.Floor(value);
        }

        Vector2d origin = (start.X + 0.5, start.Y + 0.5);
        Vector2d ray = direction * length;
        Vector2i current = start;
        Vector2i step = direction.Sign();

        double tDeltaX = step.X != 0 ? step.X / ray.X : double.MaxValue;
        double tDeltaY = step.Y != 0 ? step.Y / ray.Y : double.MaxValue;

        double tMaxX = step.X > 0 ? tDeltaX * Frac1(origin.X) : tDeltaX * Frac0(origin.X);
        double tMaxY = step.Y > 0 ? tDeltaY * Frac1(origin.Y) : tDeltaY * Frac0(origin.Y);

        yield return start;

        while (true)
        {
            if (tMaxX < tMaxY)
            {
                current.X += step.X;
                tMaxX += tDeltaX;
            }
            else
            {
                current.Y += step.Y;
                tMaxY += tDeltaY;
            }

            if (tMaxX > 1 && tMaxY > 1) break;

            yield return current;
        }
    }
}
