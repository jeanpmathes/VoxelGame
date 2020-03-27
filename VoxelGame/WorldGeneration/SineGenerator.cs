// <copyright file="SinusGenerator.cs" company="VoxelGame">
//     All rights reserved.
// </copyright>
// <author>pershingthesecond</author>
using System;
using System.Collections.Generic;
using VoxelGame.Logic;

namespace VoxelGame.WorldGeneration
{
    public class SineGenerator : IWorldGenerator
    {
        private int amplitude;
        private int mid;
        private float a;
        private float b;

        public SineGenerator(int amplitude, int mid, float a = 1f, float b = 1f)
        {
            this.amplitude = amplitude;
            this.mid = mid;
            this.a = a;
            this.b = b;
        }

        public Block GenerateBlock(int x, int y, int z)
        {
            int height = (int)(amplitude * (Math.Sin(a * x) - Math.Sin(b * z))) + mid;

            if (y > height)
            {
                return Block.AIR;
            }
            else if (y == height)
            {
                return Block.GRASS;
            }
            else if (y > height - 5)
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
            int height = (int)(amplitude * (Math.Sin(a * x) - Math.Sin(b * z))) + mid;

            for (int y = 0; y < Section.SectionSize * Chunk.ChunkHeight; y++)
            {
                if (y > height)
                {
                    yield return Block.AIR;
                }
                else if (y == height)
                {
                    yield return Block.GRASS;
                }
                else if (y > height - 5)
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