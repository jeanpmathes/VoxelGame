// <copyright file="FlatGenerator.cs" company="VoxelGame">
//     All rights reserved.
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
                return Game.AIR;
            }
            else if (y == heightAir)
            {
                return Game.GRASS;
            }
            else if (y > heightDirt)
            {
                return Game.DIRT;
            }
            else
            {
                return Game.STONE;
            }
        }

        public IEnumerable<Block> GenerateColumn(int x, int z)
        {
            for (int y = 0; y < Section.SectionSize * Chunk.ChunkHeight; y++)
            {
                if (y > heightAir)
                {
                    yield return Game.AIR;
                }
                else if (y == heightAir)
                {
                    yield return Game.GRASS;
                }
                else if (y > heightDirt)
                {
                    yield return Game.DIRT;
                }
                else
                {
                    yield return Game.STONE;
                }
            }
        }
    }
}