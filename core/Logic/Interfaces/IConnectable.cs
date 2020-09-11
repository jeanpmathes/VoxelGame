// <copyright file="IFenceConnectable.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
namespace VoxelGame.Core.Logic.Interfaces
{
    /// <summary>
    /// Marks a block as able to be connected to by other blocks. Currently the bottom and top side do not have to be defined.
    /// </summary>
    public interface IConnectable : IBlockBase
    {
        /// <summary>
        /// Checks if this block supports connection at a specific side.
        /// </summary>
        /// <param name="side">The side to check for connect-ability.</param>
        /// <param name="x">The x position of the block to check.</param>
        /// <param name="y">The y position of the block to check.</param>
        /// <param name="z">The z position of the block to check.</param>
        /// <returns>True if connection is supported; false if not.</returns>
        public bool IsConnetable(BlockSide side, int x, int y, int z)
        {
            return true;
        }
    }
}