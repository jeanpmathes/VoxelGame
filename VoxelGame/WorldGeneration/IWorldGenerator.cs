// <copyright file="IWorldGenerator.cs" company="VoxelGame">
//     All rights reserved.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Logic;

namespace VoxelGame.WorldGeneration
{
    public interface IWorldGenerator
    {
        Block GenerateBlock(int x, int y, int z);
    }
}