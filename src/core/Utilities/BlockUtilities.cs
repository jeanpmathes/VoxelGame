// <copyright file="BlockUtilities.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenToolkit.Mathematics;

namespace VoxelGame.Core.Utilities
{
    public static class BlockUtilities
    {
        public static int GetPositionDependentNumber(int x, int z, int mod)
        {
            return Math.Abs(HashCode.Combine(x, z)) % mod;
        }

        public static int GetPositionDependentNumber(Vector3i position, int mod)
        {
            return Math.Abs(HashCode.Combine(position.X, position.Y, position.Z)) % mod;
        }
    }
}