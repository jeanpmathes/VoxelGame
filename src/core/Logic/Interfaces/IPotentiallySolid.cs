// <copyright file="IPotentiallySolid.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenTK.Mathematics;

namespace VoxelGame.Core.Logic.Interfaces
{
    /// <summary>
    ///     Mark a non-solid block as able to become solid.
    /// </summary>
    public interface IPotentiallySolid : IBlockBase
    {
        /// <summary>
        ///     Make the block solid.
        ///     This will replace the block with a solid block.
        /// </summary>
        /// <param name="world">The world in which the operation takes place.</param>
        /// <param name="position">The position of the block.</param>
        void BecomeSolid(World world, Vector3i position);
    }
}
