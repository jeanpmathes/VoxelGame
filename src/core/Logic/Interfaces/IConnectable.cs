// <copyright file="IConnectable.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

namespace VoxelGame.Core.Logic.Interfaces
{
    public interface IConnectable : IBlockBase
    {
        /// <summary>
        /// Checks if this block supports connection at a specific side.
        /// </summary>
        /// <param name="world">The world this block is in.</param>
        /// <param name="side">The side to check for connect-ability. Only front, back, left and right are valid.</param>
        /// <param name="x">The x position of the block to check.</param>
        /// <param name="y">The y position of the block to check.</param>
        /// <param name="z">The z position of the block to check.</param>
        /// <returns>True if connection is supported; false if not.</returns>
        public bool IsConnectable(World world, BlockSide side, int x, int y, int z)
        {
            return true;
        }

        /// <summary>
        /// Get the data about connectables around a block. The data is packed in an uint using four bits.
        /// </summary>
        /// <param name="world">The world the block is in.</param>
        /// <param name="x">The x position of the block.</param>
        /// <param name="y">The y position of the block.</param>
        /// <param name="z">The z position of the block.</param>
        /// <returns></returns>
        public static uint GetConnectionData<T>(World world, int x, int y, int z) where T : IConnectable
        {
            uint data = 0;

            if (world.GetBlock(x, y, z - 1, out _) is T north && north.IsConnectable(world, BlockSide.Front, x, y, z - 1))
                data |= 0b00_1000;
            if (world.GetBlock(x + 1, y, z, out _) is T east && east.IsConnectable(world, BlockSide.Left, x + 1, y, z))
                data |= 0b00_0100;
            if (world.GetBlock(x, y, z + 1, out _) is T south && south.IsConnectable(world, BlockSide.Back, x, y, z + 1))
                data |= 0b00_0010;
            if (world.GetBlock(x - 1, y, z, out _) is T west && west.IsConnectable(world, BlockSide.Right, x - 1, y, z))
                data |= 0b00_0001;

            return data;
        }
    }
}