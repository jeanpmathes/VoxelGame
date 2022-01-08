// <copyright file="IAshCoverable.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;

namespace VoxelGame.Core.Logic.Interfaces
{
    /// <summary>
    ///     Marks a block as able to be covered with ash.
    /// </summary>
    public interface IAshCoverable : IBlockBase
    {
        /// <summary>
        ///     Cover the block with ash.
        /// </summary>
        /// <param name="world">The world the block is in.</param>
        /// <param name="position">The position of the block.</param>
        public void CoverWithAsh(World world, Vector3i position);
    }
}