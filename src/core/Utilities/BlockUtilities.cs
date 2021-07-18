// <copyright file="BlockUtilities.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;

namespace VoxelGame.Core.Utilities
{
    public static class BlockUtilities
    {
        public static int GetPositionDependentNumber(int x, int z, int mod)
        {
            return Math.Abs(HashCode.Combine(x, z)) % mod;
        }

        public static int GetPositionDependentNumber(int x, int y, int z, int mod)
        {
            return Math.Abs(HashCode.Combine(x, y, z)) % mod;
        }
    }
}