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
        public static int GetPositionDependantNumber(int x, int z, int mod)
        {
            return HashCode.Combine(x, z) % mod;
        }

        public static int GetPositionDependantNumber(int x, int y, int z, int mod)
        {
            return HashCode.Combine(x, y, z) % mod;
        }
    }
}