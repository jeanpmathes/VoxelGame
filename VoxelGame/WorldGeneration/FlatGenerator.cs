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
        private int heightAir;
        private int heightDirt;

        public FlatGenerator(int heightAir, int heightDirt)
        {
            this.heightAir = heightAir;
            this.heightDirt = heightDirt;
        }

        public Block GenerateBlock(int x, int y, int z)
        {
            if (y > heightAir)
            {
                return Block.AIR;
            }
            else if (y == heightAir)
            {
                return Block.GRASS;
            }
            else if (y > heightDirt)
            {
                return Block.DIRT;
            }
            else
            {
                return Block.STONE;
            }
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