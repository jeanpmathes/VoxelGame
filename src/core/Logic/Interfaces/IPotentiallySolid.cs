// <copyright file="IPotentiallySolid.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

namespace VoxelGame.Core.Logic.Interfaces
{
    /// <summary>
    /// Mark a non-solid block as able to become solid.
    /// </summary>
    public interface IPotentiallySolid : IBlockBase
    {
        void BecomeSolid(World world, int x, int y, int z);
    }
}