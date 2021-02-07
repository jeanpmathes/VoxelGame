namespace VoxelGame.Core.Logic.Interfaces
{
    internal interface IPipeConnectable : IBlockBase
    {
        /// <summary>
        /// Checks if this block supports connection at a specific side.
        /// </summary>
        /// <param name="side">The side to check for connect-ability.</param>
        /// <param name="x">The x position of the block to check.</param>
        /// <param name="y">The y position of the block to check.</param>
        /// <param name="z">The z position of the block to check.</param>
        /// <returns>True if connection is supported; false if not.</returns>
        public bool IsConnectable(BlockSide side, int x, int y, int z)
        {
            return true;
        }
    }
}