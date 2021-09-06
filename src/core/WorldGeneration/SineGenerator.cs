// <copyright file="SineGenerator.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.WorldGeneration
{
    public class SineGenerator : IWorldGenerator
    {
        private readonly int amplitude;
        private readonly int mid;
        private readonly float a;
        private readonly float b;

        public SineGenerator(int amplitude, int mid, float a = 1f, float b = 1f)
        {
            this.amplitude = amplitude;
            this.mid = mid;
            this.a = a;
            this.b = b;
        }

        public IEnumerable<Block> GenerateColumn(int x, int z)
        {
            int height = (int) (amplitude * (Math.Sin(a * x) - Math.Sin(b * z))) + mid;

            for (int y = 0; y < Section.SectionSize * Chunk.VerticalSectionCount; y++)
            {
                if (y > height)
                {
                    yield return Block.Air;
                }
                else if (y == height)
                {
                    yield return Block.Grass;
                }
                else if (y > height - 5)
                {
                    yield return Block.Dirt;
                }
                else
                {
                    yield return Block.Stone;
                }
            }
        }
    }
}