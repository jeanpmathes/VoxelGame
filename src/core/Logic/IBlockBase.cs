// <copyright file="IBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Entities;

namespace VoxelGame.Core.Logic
{
    /// <summary>
    /// Defines the basic <see cref="Block"/> methods required for a lot of block functionality.
    /// </summary>
    public interface IBlockBase
    {
        /// <summary>
        /// Gets whether this block completely fills a 1x1x1 volume or not. If a block is not full, it cannot be opaque.
        /// </summary>
        public bool IsFull { get; }

        /// <summary>
        /// Gets whether it is possible to see through this block. This will affect the rendering of this block and the blocks around it.
        /// </summary>
        public bool IsOpaque { get; }

        /// <summary>
        /// Gets whether this block hinders movement.
        /// </summary>
        public bool IsSolid { get; }

        /// <summary>
        /// Gets whether this block is solid and full.
        /// </summary>
        public bool IsSolidAndFull { get; }

        /// <summary>
        /// Tries to place a block in the world.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="x">The x position where a block should be placed.</param>
        /// <param name="y">The y position where a block should be placed.</param>
        /// <param name="z">The z position where a block should be placed.</param>
        /// <param name="entity">The entity that tries to place the block. May be null.</param>
        /// <returns>Returns true if placing the block was successful.</returns>
        public bool Place(World world, int x, int y, int z, PhysicsEntity? entity = null);

        /// <summary>
        /// Destroys a block in the world if it is the same type as this block.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="x">The x position of the block to destroy.</param>
        /// <param name="y">The y position of the block to destroy.</param>
        /// <param name="z">The z position of the block to destroy.</param>
        /// <param name="entity">The entity which caused the destruction, or null if no entity caused it.</param>
        /// <returns>Returns true if the block has been destroyed.</returns>
        public bool Destroy(World world, int x, int y, int z, PhysicsEntity? entity = null);
    }
}