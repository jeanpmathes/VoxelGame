// <copyright file="WorldUtilities.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
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
    /// <returns></returns>
    public static int GetPositionDependentNumber(Vector3i position, int mod)
    {
        return Math.Abs(HashCode.Combine(position.X, position.Y, position.Z)) % mod;
    }
}

/// <summary>
///     Extension methods for <see cref="World" />.
/// </summary>
public static class WorldExtensions
{
    /// <summary>
    ///     Check if a given position has a solid block.
    /// </summary>
    public static bool IsSolid(this World world, Vector3i position)
    {
        return IsSolid(world, position, out _);
    }

    /// <summary>
    ///     Check if a given position has a solid block and get the block.
    /// </summary>
    private static bool IsSolid(this World world, Vector3i position, out BlockInstance block)
    {
        block = world.GetBlock(position) ?? BlockInstance.Default;

        return block.Block.IsSolidAndFull
               || block.Block is IHeightVariable varHeight &&
               varHeight.GetHeight(block.Data) == IHeightVariable.MaximumHeight;
    }

    /// <summary>
    ///     Check if a given position has solid ground below it.
    /// </summary>
    public static bool HasSolidGround(this World world, Vector3i position, bool solidify = false)
    {
        Vector3i groundPosition = position.Below();

        bool isSolid = world.IsSolid(groundPosition, out BlockInstance ground);

        if (!solidify || isSolid || ground.Block is not IPotentiallySolid solidifiable) return isSolid;

        solidifiable.BecomeSolid(world, groundPosition);

        return true;
    }

    /// <summary>
    ///     Check if a given position has a solid top above it.
    /// </summary>
    public static bool HasSolidTop(this World world, Vector3i position)
    {
        return world.IsSolid(position.Above());
    }

    /// <summary>
    ///     Check if a given position has an opaque block above it.
    /// </summary>
    public static bool HasOpaqueTop(this World world, Vector3i position)
    {
        Block top = world.GetBlock(position.Above())?.Block ?? Block.Air;

        return top.IsSolidAndFull && top.IsOpaque
               || top is IHeightVariable;
    }

    /// <summary>
    ///     Check if a given block is lowered exactly one height step.
    /// </summary>
    public static bool IsLowered(this World world, Vector3i position)
    {
        BlockInstance? below = world.GetBlock(position.Below());

        return below?.Block is IHeightVariable block
               && block.GetHeight(below.Value.Data) == IHeightVariable.MaximumHeight - 1;
    }
}
