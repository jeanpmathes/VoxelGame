// <copyright file="CombinationMap.cs" company="VoxelGame">
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
using System.Diagnostics;

namespace VoxelGame.Core.Collections;

/// <summary>
///     Maps unordered pairs to a single value.
/// </summary>
/// <typeparam name="TE">The identifiable element type for the pairs.</typeparam>
/// <typeparam name="TV">The result type of the mapping.</typeparam>
public class CombinationMap<TE, TV> where TE : IIdentifiable<UInt32>
{
    private readonly Boolean[][] flags;
    private readonly TV[][] table;

    /// <summary>
    ///     Create a new combination map.
    /// </summary>
    /// <param name="range">The maximum range of element values.</param>
    public CombinationMap(Int32 range)
    {
        flags = new Boolean[range][];
        table = new TV[range][];

        for (var i = 0; i < range; i++)
        {
            flags[i] = new Boolean[i];
            table[i] = new TV[i];
        }
    }

    private TV this[TE a, TE b]
    {
        get
        {
            Debug.Assert(a.ID != b.ID);

            var i = (Int32) Math.Max(a.ID, b.ID);
            var j = (Int32) Math.Min(a.ID, b.ID);

            return table[i][j];
        }

        set
        {
            Debug.Assert(a.ID != b.ID);

            var i = (Int32) Math.Max(a.ID, b.ID);
            var j = (Int32) Math.Min(a.ID, b.ID);

            Debug.Assert(!flags[i][j], "This combination is already set.");

            table[i][j] = value;
        }
    }

    /// <summary>
    ///     Add a combination between one element and each of the others.
    ///     After a mapping has been set, it cannot be changed.
    /// </summary>
    /// <param name="e">The first element that is part of the mapping.</param>
    /// <param name="v">The value to map to.</param>
    /// <param name="others">The other elements, each combined to a pair with the first element.</param>
    public void AddCombination(TE e, TV v, params TE[] others)
    {
        Debug.Assert(others.Length > 0);

        foreach (TE other in others) this[e, other] = v;
    }

    /// <summary>
    ///     Resolve the mapping for the given elements.
    ///     The order of the elements is not relevant.
    /// </summary>
    /// <param name="a">The first element.</param>
    /// <param name="b">The second element.</param>
    /// <returns></returns>
    public TV Resolve(TE a, TE b)
    {
        return this[a, b];
    }
}
