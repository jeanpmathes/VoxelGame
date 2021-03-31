// <copyright file="WorldUtilities.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Interfaces;

namespace VoxelGame.Core.Utilities
{
    public static class WorldExtensions
    {
        public static bool HasSolidGround(this World world, int x, int y, int z)
        {
            Block ground = world.GetBlock(x, y - 1, z, out uint data) ?? Block.Air;

            return ground.IsSolidAndFull
                   || (ground is IHeightVariable varHeight && varHeight.GetHeight(data) == IHeightVariable.MaximumHeight);
        }
    }
}