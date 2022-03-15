// <copyright file="IBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Entities;

namespace VoxelGame.Core.Logic
{
    /// <summary>
    ///     Defines the basic <see cref="Block" /> methods required for a lot of block functionality.
    /// </summary>
    public interface IBlockBase
    {
        /// <summary>
        ///     Gets whether this block is solid and full.
        /// </summary>
        public bool IsSolidAndFull { get; }

        /// <summary>
        ///     Tries to place a block in the world.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="position"></param>
        /// <param name="entity">The entity that tries to place the block. May be null.</param>
        /// <returns>Returns true if placing the block was successful.</returns>
        public bool Place(World world, Vector3i position, PhysicsEntity? entity = null);

        /// <summary>
        ///     Destroys a block in the world if it is the same type as this block.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="position"></param>
        /// <param name="entity">The entity which caused the destruction, or null if no entity caused it.</param>
        /// <returns>Returns true if the block has been destroyed.</returns>
        public bool Destroy(World world, Vector3i position, PhysicsEntity? entity = null);
    }
}
