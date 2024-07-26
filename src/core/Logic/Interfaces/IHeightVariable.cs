// <copyright file="IHeightVariable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Elements;

namespace VoxelGame.Core.Logic.Interfaces;

/// <summary>
///     Allows blocks to have variable height.
///     The height varies in steps of 1/16th of a block.
///     Height is a number from 0 to <see cref="MaximumHeight" /> inclusive.
///     Note that the number 0 indicates a block with a height of 1/16th of a block, not a block with no height.
/// </summary>
public interface IHeightVariable : IBlockBase
{
    /// <summary>
    ///     The maximum height. A block with this height completely fills a position.
    /// </summary>
    public static Int32 MaximumHeight => 15;

    /// <summary>
    ///     Special constant to indicate that a block has no height.
    ///     This is only allowed in certain cases.
    /// </summary>
    public static Int32 NoHeight => -1;

    /// <summary>
    ///     The half height. A block with this height fills half of a position.
    /// </summary>
    public static Int32 HalfHeight => MaximumHeight / 2;

    /// <inheritdoc />
    Boolean IBlockBase.IsSideFull(BlockSide side, UInt32 data)
    {
        if (side == BlockSide.Bottom) return true;

        Int32 height = GetHeight(data);

        return height == MaximumHeight;
    }

    /// <summary>
    ///     Convert a fluid height to a block height.
    /// </summary>
    /// <param name="fluidHeight">The fluid height, in the range [-1, 7].</param>
    /// <returns>The block height, in the range [-1, 15].</returns>
    public static Int32 GetBlockHeightFromFluidHeight(Int32 fluidHeight)
    {
        return fluidHeight * 2 + 1;
    }

    /// <summary>
    ///     Get the size of a face with a given height, in world units.
    /// </summary>
    /// <param name="height">The height of the face.</param>
    /// <returns>The size of the face.</returns>
    public static Single GetSize(Int32 height)
    {
        return (height + 1) / (Single) (MaximumHeight + 1);
    }

    /// <summary>
    ///     Get the gap of a face, which is the space between the end of the face and the end of the block, in world units.
    /// </summary>
    /// <param name="height">The height of the face.</param>
    /// <returns>The gap of the face.</returns>
    public static Single GetGap(Int32 height)
    {
        return 1 - GetSize(height);
    }

    /// <summary>
    ///     Get the bounds of a face with a given height.
    ///     The bounds can be used as texture coordinates.
    /// </summary>
    /// <param name="height">The height of the face.</param>
    /// <returns>The bounds of the face.</returns>
    public static (Vector2 min, Vector2 max) GetBounds(Int32 height)
    {
        Single size = GetSize(height);

        return (new Vector2(x: 0, y: 0), new Vector2(x: 1, size));
    }

    /// <summary>
    ///     Get the height of a block, given the block data.
    /// </summary>
    /// <param name="data">The data from which to extract the height.</param>
    /// <returns>The block height.</returns>
    Int32 GetHeight(UInt32 data);
}
