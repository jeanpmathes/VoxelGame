// <copyright file="FlatGenerator.cs" company="VoxelGame">
//     All rights reserved.
// </copyright>
// <author>pershingthesecond</author>
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
    }
}