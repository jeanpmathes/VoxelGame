// <copyright file="SinusGenerator.cs" company="VoxelGame">
//     All rights reserved.
// </copyright>
// <author>pershingthesecond</author>
using System;

using VoxelGame.Logic;

namespace VoxelGame.WorldGeneration
{
    public class SinusGenerator : IWorldGenerator
    {
        int amplitude;
        int mid;
        float a;
        float b;

        public SinusGenerator(int amplitude, int mid, float a = 1f, float b = 1f)
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
                return Game.AIR;
            }
            else if (y == height)
            {
                return Game.GRASS;
            }
            else if (y > height - 5)
            {
                return Game.DIRT;
            }
            else
            {
                return Game.STONE;
            }
        }
    }
}
