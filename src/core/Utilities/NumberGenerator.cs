// <copyright file="NumberGenerator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     A utility class for generating random numbers.
/// </summary>
public static class NumberGenerator
{
    /// <summary>
    ///     The random number generator.
    /// </summary>
    public static Random Random => Random.Shared;

    /// <summary>
    ///     Get a position dependent number.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <param name="mod">The value range. The resulting number will be in [0, mod).</param>
    /// <returns>The position dependant number.</returns>
    public static Int32 GetPositionDependentNumber(Vector3i position, Int32 mod)
    {
        return Math.Abs(HashCode.Combine(position.X, position.Y, position.Z)) % mod;
    }

    /// <summary>
    ///     Get a position dependent number.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <param name="mod">The value range. The resulting number will be in [0, mod).</param>
    /// <returns>The position dependant number.</returns>
    public static UInt32 GetPositionDependentNumber(Vector3i position, UInt32 mod)
    {
        return (UInt32) Math.Abs(HashCode.Combine(position.X, position.Y, position.Z)) % mod;
    }

    /// <summary>
    ///     Get a position dependent outcome based on a chance.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <param name="chance">The chance of the outcome being true.</param>
    /// <returns>The position dependent outcome.</returns>
    public static Boolean GetPositionDependentOutcome(Vector3i position, Chance chance)
    {
        Int32 number = GetPositionDependentNumber(position, mod: 100);

        return chance.Passes(number);
    }
}
