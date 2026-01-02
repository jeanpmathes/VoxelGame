// <copyright file="Algorithms.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
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
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Generation.Worlds;

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
    public static List<List<Int16>> BuildAdjacencyList(Dictionary<Int16, HashSet<Int16>> adjacencyHashed)
    {
        List<List<Int16>> adjacency = [];

        for (Int16 id = 0; id < adjacencyHashed.Count; id++)
        {
            List<Int16> neighbors = [..adjacencyHashed[id]];
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
    public static (List<Int16>, Dictionary<Int16, List<Int16>>) MergeAdjacencyList(List<List<Int16>> original, Func<Int16, Int16> merge)
    {
        Dictionary<Int16, HashSet<Int16>> adjacencyHashed = new();

        for (Int16 current = 0; current < original.Count; current++)
        {
            Int16 mergedCurrent = merge(current);

            HashSet<Int16> currentSet = adjacencyHashed.GetOrAdd(mergedCurrent);

            foreach (Int16 neighbor in original[current])
            {
                Int16 mergedNeighbor = merge(neighbor);

                if (mergedCurrent != mergedNeighbor) currentSet.Add(mergedNeighbor);
            }
        }

        List<Int16> nodes = adjacencyHashed.Keys.OrderBy(key => key).ToList();
        Dictionary<Int16, List<Int16>> adjacency = adjacencyHashed.ToDictionary(pair => pair.Key, pair => new List<Int16>(pair.Value));

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
    public static IEnumerable<Vector2i> TraverseCells(Vector2i start, Vector2d direction, Double length)
    {
        Double Frac0(Double value)
        {
            return value - Math.Floor(value);
        }

        Double Frac1(Double value)
        {
            return 1 - value + Math.Floor(value);
        }

        Vector2d origin = (start.X + 0.5, start.Y + 0.5);
        Vector2d ray = direction * length;
        Vector2i current = start;
        Vector2i step = direction.Sign();

        Double tDeltaX = step.X != 0 ? step.X / ray.X : Double.MaxValue;
        Double tDeltaY = step.Y != 0 ? step.Y / ray.Y : Double.MaxValue;

        Double tMaxX = step.X > 0 ? tDeltaX * Frac1(origin.X) : tDeltaX * Frac0(origin.X);
        Double tMaxY = step.Y > 0 ? tDeltaY * Frac1(origin.Y) : tDeltaY * Frac0(origin.Y);

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
