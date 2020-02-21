// <copyright file="IWorldGenerator.cs" company="VoxelGame">
//     All rights reserved.
// </copyright>
// <author>pershingthesecond</author>
using System.Collections.Generic;
using VoxelGame.Logic;

namespace VoxelGame.WorldGeneration
{
    public interface IWorldGenerator
    {
        Block GenerateBlock(int x, int y, int z);

        IEnumerable<Block> GenerateColumn(int x, int z);
    }
}