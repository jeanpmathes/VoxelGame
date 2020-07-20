// <copyright file="IFlammable.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
namespace VoxelGame.Logic.Interfaces
{
    /// <summary>
    /// Marks a block as able to be burned.
    /// </summary>
    public interface IFlammable : IBlockBase
    {
        /// <summary>
        /// Try to burn a block at a given position.
        /// </summary>
        /// <param name="x">The x position to burn.</param>
        /// <param name="y">The y position to burn.</param>
        /// <param name="z">The z position to burn.</param>
        /// <returns>true if the block was burned, false if not.</returns>
        public bool Burn(int x, int y, int z)
        {
            Destroy(x, y, z, null);

            return true;
        }
    }
}