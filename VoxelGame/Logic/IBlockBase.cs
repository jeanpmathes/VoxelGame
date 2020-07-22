// <copyright file="IBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
namespace VoxelGame.Logic
{
    /// <summary>
    /// Defines the basic <see cref="Block"/> methods required for a lot of block functionality.
    /// </summary>
    public interface IBlockBase
    {
        /// <summary>
        /// Tries to place a block in the world.
        /// </summary>
        /// <param name="x">The x position where a block should be placed.</param>
        /// <param name="y">The y position where a block should be placed.</param>
        /// <param name="z">The z position where a block should be placed.</param>
        /// <param name="entity">The entity that tries to place the block. May be null.</param>
        /// <returns>Returns true if placing the block was successful.</returns>
        public bool Place(int x, int y, int z, Entities.PhysicsEntity? entity = null);

        /// <summary>
        /// Destroys a block in the world if it is the same type as this block.
        /// </summary>
        /// <param name="x">The x position of the block to destroy.</param>
        /// <param name="y">The y position of the block to destroy.</param>
        /// <param name="z">The z position of the block to destroy.</param>
        /// <param name="entity">The entity which caused the destruction, or null if no entity caused it.</param>
        /// <returns>Returns true if the block has been destroyed.</returns>
        public bool Destroy(int x, int y, int z, Entities.PhysicsEntity? entity = null);
    }
}