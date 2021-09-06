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
        public static bool IsSolid(this World world, int x, int y, int z)
        {
            return IsSolid(world, x, y, z, out _);
        }

        private static bool IsSolid(this World world, int x, int y, int z, out Block block)
        {
            block = world.GetBlock(x, y, z, out uint data) ?? Block.Air;

            return block.IsSolidAndFull
                   || (block is IHeightVariable varHeight &&
                       varHeight.GetHeight(data) == IHeightVariable.MaximumHeight);
        }

        public static bool HasSolidGround(this World world, int x, int y, int z, bool solidify = false)
        {
            bool isSolid = world.IsSolid(x, y - 1, z, out Block ground);

            if (!solidify || isSolid || !(ground is IPotentiallySolid solidifiable)) return isSolid;

            solidifiable.BecomeSolid(world, x, y - 1, z);

            return true;
        }

        public static bool HasSolidTop(this World world, int x, int y, int z)
        {
            return world.IsSolid(x, y + 1, z);
        }

        public static bool HasOpaqueTop(this World world, int x, int y, int z)
        {
            Block top = world.GetBlock(x, y + 1, z, out _) ?? Block.Air;

            return (top.IsSolidAndFull && top.IsOpaque)
                   || top is IHeightVariable;
        }

        public static bool IsLowered(this World world, int x, int y, int z)
        {
            return world.GetBlock(x, y - 1, z, out uint data) is IHeightVariable block
                   && block.GetHeight(data) == IHeightVariable.MaximumHeight - 1;
        }
    }
}