using OpenToolkit.Mathematics;

namespace VoxelGame.Core.Logic.Interfaces
{
    internal interface IPipeConnectable : IBlockBase
    {
        /// <summary>
        ///     Checks if this block supports connection at a specific side.
        /// </summary>
        /// <param name="world">The world this block is in.</param>
        /// <param name="side">The side to check for connect-ability.</param>
        /// <param name="position">The position of the block to check.</param>
        /// <returns>True if connection is supported; false if not.</returns>
        public bool IsConnectable(World world, BlockSide side, Vector3i position)
        {
            return true;
        }
    }
}