// <copyright file="IWorldGenerator.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.WorldGeneration
{
    public interface IWorldGenerator
    {
        IEnumerable<Block> GenerateColumn(int x, int z);
    }
}