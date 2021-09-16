// <copyright file="IPotentiallySolid.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;

namespace VoxelGame.Core.Logic.Interfaces
{
    /// <summary>
    ///     Mark a non-solid block as able to become solid.
    /// </summary>
    public interface IPotentiallySolid : IBlockBase
    {
        void BecomeSolid(World world, Vector3i position);
    }
}