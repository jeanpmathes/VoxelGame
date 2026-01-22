// <copyright file="BitTools.cs" company="VoxelGame">
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
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using VoxelGame.Toolkit;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     A utility class for bit manipulation.
/// </summary>
public static class BitTools
{
    /// <summary>
    ///     Counts how many bits are set in an unsigned integer.
    /// </summary>
    /// <param name="n">The unsigned integer in which to count the set bits.</param>
    /// <returns>The number of set bits.</returns>
    public static Int32 CountSetBits(UInt32 n)
    {
        return BitOperations.PopCount(n);
    }

    /// <summary>
    ///     Counts how many bits are set in an unsigned long integer.
    /// </summary>
    /// <param name="n">The unsigned long integer in which to count the set bits.</param>
    /// <returns>The number of set bits.</returns>
    public static Int32 CountSetBits(UInt64 n)
    {
        return BitOperations.PopCount(n);
    }

    /// <summary>
    ///     Get the zero-based index of the most significant set bit.
    ///     Invalid if <paramref name="n" /> is zero.
    /// </summary>
    /// <param name="n">The unsigned integer in which to find the most significant bit.</param>
    /// <returns>The index of the most significant bit.</returns>
    public static Int32 MostSignificantBit(UInt32 n)
    {
        Debug.Assert(n != 0);

        return 32 - 1 - BitOperations.LeadingZeroCount(n);
    }

    /// <summary>
    ///     Get the zero-based index of the most significant set bit.
    ///     Invalid if <paramref name="n" /> is zero.
    /// </summary>
    /// <param name="n">The unsigned long integer in which to find the most significant bit.</param>
    /// <returns>The index of the most significant bit.</returns>
    public static Int32 MostSignificantBit(UInt64 n)
    {
        Debug.Assert(n != 0);

        return 64 - 1 - BitOperations.LeadingZeroCount(n);
    }

    /// <summary>
    ///     Get the zero-based index of the least significant set bit.
    ///     Invalid if <paramref name="n" /> is zero.
    /// </summary>
    /// <param name="n">The unsigned integer in which to find the least significant bit.</param>
    /// <returns>The index of the least significant bit.</returns>
    public static Int32 LeastSignificantBit(UInt32 n)
    {
        Debug.Assert(n != 0);

        return BitOperations.TrailingZeroCount(n);
    }

    /// <summary>
    ///     Count the number of set boolean values in an array.
    /// </summary>
    /// <param name="booleans">The array or parameter to count the true values in.</param>
    /// <returns>Return the count.</returns>
    public static Int32 CountSetBooleans(params Boolean[] booleans)
    {
        return booleans.Sum(b => b.ToInt());
    }

    /// <summary>
    ///     Get a bit-mask of a given size.
    /// </summary>
    public static UInt32 GetMask(Int32 size)
    {
        Debug.Assert(size >= 0);

        if (size >= 32) return UInt32.MaxValue;

        return (1u << size) - 1u;
    }

    /// <summary>
    ///     Signifies an implication. If a is true, b must be true as well.
    /// </summary>
    public static Boolean Implies(this Boolean a, Boolean b)
    {
        return !a || b;
    }
}
