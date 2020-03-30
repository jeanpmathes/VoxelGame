// <copyright file="IWorldGenerator.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
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