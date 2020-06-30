// <copyright file="FlatGenerator.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using System.Collections.Generic;
using VoxelGame.Logic;

namespace VoxelGame.WorldGeneration
{
    public class FlatGenerator : IWorldGenerator
    {
        private readonly int heightAir;
        private readonly int heightDirt;

        public FlatGenerator(int heightAir, int heightDirt)
        {
            this.heightAir = heightAir;
            this.heightDirt = heightDirt;
        }

        public IEnumerable<Block> GenerateColumn(int x, int z)
        {
            for (int y = 0; y < Section.SectionSize * Chunk.ChunkHeight; y++)
            {
                if (y > heightAir)
                {
                    yield return Block.AIR;
                }
                else if (y == heightAir)
                {
                    yield return Block.GRASS;
                }
                else if (y > heightDirt)
                {
                    yield return Block.DIRT;
                }
                else
                {
                    yield return Block.STONE;
                }
            }
        }
    }
}