// <copyright file="BitHelper.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Linq;
using System.Numerics;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     A utility class for bit manipulation.
/// </summary>
public static class BitHelper
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
        return (UInt32) (1 << size) - 1;
    }
}
