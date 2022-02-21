// <copyright file="IHeightVariable.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

namespace VoxelGame.Core.Logic.Interfaces
{
    /// <summary>
    ///     Allows blocks to have variable height.
    ///     The height varies in steps of 1/16th of a block.
    ///     Height is a number from 0 to <see cref="MaximumHeight" /> inclusive.
    /// </summary>
    public interface IHeightVariable
    {
        /// <summary>
        ///     The maximum height. A block with this height completely fills a position.
        /// </summary>
        public const int MaximumHeight = 15;

        /// <summary>
        ///     Get the height of a block, given the block data.
        /// </summary>
        /// <param name="data">The data from which to extract the height.</param>
        /// <returns>The block height.</returns>
        int GetHeight(uint data);
    }
}
