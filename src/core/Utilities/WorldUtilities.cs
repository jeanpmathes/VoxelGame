// <copyright file="WorldUtilities.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     Utility methods for block behavior.
/// </summary>
public static class BlockUtilities // todo: find a better way, maybe put this into MathTools
{
    /// <summary>
    ///     Get a position dependant number.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <param name="mod">The value range. The resulting number will be in [0, mod).</param>
    /// <returns>The position dependant number.</returns>
    public static Int32 GetPositionDependentNumber(Vector3i position, Int32 mod)
    {
        return Math.Abs(HashCode.Combine(position.X, position.Y, position.Z)) % mod;
    }

    /// <summary>
    ///     Get a position dependant number.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <param name="mod">The value range. The resulting number will be in [0, mod).</param>
    /// <returns>The position dependant number.</returns>
    public static UInt32 GetPositionDependentNumber(Vector3i position, UInt32 mod)
    {
        return (UInt32) Math.Abs(HashCode.Combine(position.X, position.Y, position.Z)) % mod;
    }
}
