// <copyright file="WorldUtilities.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Interfaces;

namespace VoxelGame.Core.Utilities
{
    public static class WorldExtensions
    {
        public static bool IsSolid(this World world, Vector3i position)
        {
            return IsSolid(world, position, out _);
        }

        private static bool IsSolid(this World world, Vector3i position, out Block block)
        {
            block = world.GetBlock(position, out uint data) ?? Block.Air;

            return block.IsSolidAndFull
                   || block is IHeightVariable varHeight &&
                   varHeight.GetHeight(data) == IHeightVariable.MaximumHeight;
        }

        public static bool HasSolidGround(this World world, Vector3i position, bool solidify = false)
        {
            Vector3i groundPosition = position.Below();

            bool isSolid = world.IsSolid(groundPosition, out Block ground);

            if (!solidify || isSolid || ground is not IPotentiallySolid solidifiable) return isSolid;

            solidifiable.BecomeSolid(world, groundPosition);

            return true;
        }

        public static bool HasSolidTop(this World world, Vector3i position)
        {
            return world.IsSolid(position.Above());
        }

        public static bool HasOpaqueTop(this World world, Vector3i position)
        {
            Block top = world.GetBlock(position.Above(), out _) ?? Block.Air;

            return top.IsSolidAndFull && top.IsOpaque
                   || top is IHeightVariable;
        }

        public static bool IsLowered(this World world, Vector3i position)
        {
            return world.GetBlock(position.Below(), out uint data) is IHeightVariable block
                   && block.GetHeight(data) == IHeightVariable.MaximumHeight - 1;
        }
    }
}
