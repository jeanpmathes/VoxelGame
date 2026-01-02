// <copyright file="NumberGenerator.cs" company="VoxelGame">
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
