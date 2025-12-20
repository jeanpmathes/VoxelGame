// <copyright file="UnionFind.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Collections;

/// <summary>
///     A simple short-based union find structure.
/// </summary>
public class UnionFind
{
    private readonly Int16[] id;
    private readonly Int16[] size;

    /// <summary>
    ///     Creates a new union find structure with the given number of elements.
    /// </summary>
    /// <param name="n">The number of elements.</param>
    public UnionFind(Int16 n)
    {
        Count = n;

        id = new Int16[n];
        size = new Int16[n];

        for (Int16 i = 0; i < n; i++)
        {
            id[i] = i;
            size[i] = 1;
        }
    }

    /// <summary>
    ///     Gets the number of elements in the union find structure.
    /// </summary>
    public Int16 Count { get; private set; }

    /// <summary>
    ///     Finds the root of the given element.
    /// </summary>
    /// <param name="p">The element to find the root of.</param>
    /// <returns>The root of the given element.</returns>
    public Int16 Find(Int16 p)
    {
        Int16 root = p;

        while (root != id[root]) root = id[root];

        while (p != root)
        {
            Int16 next = id[p];
            id[p] = root;
            p = next;
        }

        return root;
    }

    /// <summary>
    ///     Unions the two given elements.
    /// </summary>
    /// <param name="p">The first element.</param>
    /// <param name="q">The second element.</param>
    /// <returns>The root of the new union.</returns>
    public Int16 Union(Int16 p, Int16 q)
    {
        Int16 i = Find(p);
        Int16 j = Find(q);

        if (i == j) return i;

        Int16 root;

        if (size[i] < size[j])
        {
            id[i] = j;
            size[j] += size[i];

            root = j;
        }
        else
        {
            id[j] = i;
            size[i] += size[j];

            root = i;
        }

        Count--;

        return root;
    }

    /// <summary>
    ///     Checks if the two given elements are in the same union.
    /// </summary>
    /// <param name="p">The first element. </param>
    /// <param name="q">The second element. </param>
    /// <returns>True if the two given elements are in the same union.</returns>
    public Boolean Connected(Int16 p, Int16 q)
    {
        return Find(p) == Find(q);
    }

    /// <summary>
    ///     Gets the size of the given element's union.
    /// </summary>
    /// <param name="p">The element to get the size of.</param>
    /// <returns>The size of the given element's union.</returns>
    public Int32 GetSize(Int16 p)
    {
        return size[Find(p)];
    }
}
