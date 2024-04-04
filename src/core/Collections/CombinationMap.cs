// <copyright file="CombinationMap.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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
