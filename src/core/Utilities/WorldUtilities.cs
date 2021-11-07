// <copyright file="WorldUtilities.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenToolkit.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Interfaces;

namespace VoxelGame.Core.Utilities
{
    public static class BlockUtilities
    {
        public static int GetPositionDependentNumber(Vector3i position, int mod)
        {
            return Math.Abs(HashCode.Combine(position.X, position.Y, position.Z)) % mod;
        }
    }

    public static class WorldExtensions
    {
        public static bool IsSolid(this World world, Vector3i position)
        {
            return IsSolid(world, position, out _);
        }

        private static bool IsSolid(this World world, Vector3i position, out BlockInstance block)
        {
            block = world.GetBlock(position) ?? BlockInstance.Default;

            return block.Block.IsSolidAndFull
                   || block.Block is IHeightVariable varHeight &&
                   varHeight.GetHeight(block.Data) == IHeightVariable.MaximumHeight;
        }

        public static bool HasSolidGround(this World world, Vector3i position, bool solidify = false)
        {
            Vector3i groundPosition = position.Below();

            bool isSolid = world.IsSolid(groundPosition, out BlockInstance ground);

            if (!solidify || isSolid || ground.Block is not IPotentiallySolid solidifiable) return isSolid;

            solidifiable.BecomeSolid(world, groundPosition);

            return true;
        }

        public static bool HasSolidTop(this World world, Vector3i position)
        {
            return world.IsSolid(position.Above());
        }

        public static bool HasOpaqueTop(this World world, Vector3i position)
        {
            Block top = world.GetBlock(position.Above())?.Block ?? Block.Air;

            return top.IsSolidAndFull && top.IsOpaque
                   || top is IHeightVariable;
        }

        public static bool IsLowered(this World world, Vector3i position)
        {
            BlockInstance? below = world.GetBlock(position.Below());

            return below?.Block is IHeightVariable block
                   && block.GetHeight(below.Data) == IHeightVariable.MaximumHeight - 1;
        }
    }
}