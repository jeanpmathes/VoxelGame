// <copyright file="WorldUtilities.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Interfaces;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     Utility methods for block behavior.
/// </summary>
public static class BlockUtilities
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

/// <summary>
///     Extension methods for <see cref="World" />.
///     All of these retrieve blocks relative to a position and can therefore not be part of the block instance.
/// </summary>
public static class WorldExtensions
{
    /// <summary>
    ///     Check if a given position has full and solid ground below it.
    ///     If the position below cannot be found, false is returned.
    /// </summary>
    public static Boolean HasFullAndSolidGround(this World world, Vector3i position, Boolean solidify = false)
    {
        Vector3i groundPosition = position.Below();
        BlockInstance ground = world.GetBlock(groundPosition) ?? BlockInstance.Default;
        Boolean isSolid = ground.IsSolidAndFull;

        if (!solidify || isSolid || ground.Block is not IPotentiallySolid solidifiable) return isSolid;

        solidifiable.BecomeSolid(world, groundPosition);

        return true;
    }

    /// <summary>
    ///     Check if a given position has an opaque block above it.
    ///     If the position above cannot be found, null is returned.
    /// </summary>
    public static Boolean? HasOpaqueTop(this World world, Vector3i position)
    {
        BlockInstance? top = world.GetBlock(position.Above());

        if (top is null) return null;

        return top.Value.Block is {IsSolid: true, IsOpaque: true} && top.Value.IsSideFull(Side.Bottom);
    }

    /// <summary>
    ///     Check if a given block is lowered exactly one height step.
    /// </summary>
    public static Boolean IsLowered(this World world, Vector3i position)
    {
        BlockInstance? below = world.GetBlock(position.Below());

        return below?.Block is IHeightVariable block
               && block.GetHeight(below.Value.Data) == IHeightVariable.MaximumHeight - 1;
    }
}
