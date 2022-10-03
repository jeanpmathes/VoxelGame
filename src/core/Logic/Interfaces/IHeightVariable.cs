// <copyright file="IHeightVariable.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

namespace VoxelGame.Core.Logic.Interfaces;

/// <summary>
///     Allows blocks to have variable height.
///     The height varies in steps of 1/16th of a block.
///     Height is a number from 0 to <see cref="MaximumHeight" /> inclusive.
/// </summary>
public interface IHeightVariable : IBlockBase
{
    /// <summary>
    ///     The maximum height. A block with this height completely fills a position.
    /// </summary>
    public static int MaximumHeight => 15;

    /// <inheritdoc />
    bool IBlockBase.IsSideFull(BlockSide side, uint data)
    {
        if (side == BlockSide.Bottom) return true;

        int height = GetHeight(data);

        return height == MaximumHeight;
    }

    /// <summary>
    ///     Get the height of a block, given the block data.
    /// </summary>
    /// <param name="data">The data from which to extract the height.</param>
    /// <returns>The block height.</returns>
    int GetHeight(uint data);
}
