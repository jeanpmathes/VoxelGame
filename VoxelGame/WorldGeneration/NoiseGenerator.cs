// <copyright file="PerlinGenerator.cs" company="VoxelGame">
//     All rights reserved.
// </copyright>
// <author>pershingthesecond</author>
using System.Collections.Generic;

using VoxelGame.Logic;

namespace VoxelGame.WorldGeneration
{
    public class NoiseGenerator : IWorldGenerator
    {
        private readonly FastNoise noise;
        private readonly int halfHeight = Section.SectionSize * Chunk.ChunkHeight / 2;

        private readonly float amplitude = 0.6f;

        private readonly int snowLevel = 550;
        private readonly int beachLevel = 450;
        private readonly int soilDepth = 5;

        private readonly float caveTreshold = 0.7f;
        private readonly float caveLifter = 3.5f;

        public NoiseGenerator(int seed)
        {
            noise = new FastNoise(seed);

            // Settings for fractal noise
            noise.SetFractalLacunarity(0.5f);
            noise.SetFractalGain(2f);

            // Settings for cellular noise
            noise.SetCellularReturnType(FastNoise.CellularReturnType.Distance2Div);
            noise.SetCellularDistanceFunction(FastNoise.CellularDistanceFunction.Euclidean);
            noise.SetCellularJitter(0.4f);
        }

        public Block GenerateBlock(int x, int y, int z)
        {
            int height = (int)(amplitude * halfHeight * noise.GetPerlinFractal(x, z)) + halfHeight;

            if (y == 0)
            {
                return Game.COBBLESTONE;
            }
            else if (y > height)
            {
                return Game.AIR;
            }
            else
            {
                if (noise.GetCellular(x, z, y) > caveTreshold + (y / (halfHeight * caveLifter)))
                {
                    return Game.AIR;
                }
                else if (y == height)
                {
                    if (y >= snowLevel)
                    {
                        return Game.STONE;
                    }
                    else if (y <= beachLevel)
                    {
                        return Game.SAND;
                    }
                    else
                    {
                        return Game.GRASS;
                    }
                }
                else
                {
                    if (height < snowLevel && height > beachLevel && y + soilDepth > height)
                    {
                        return Game.DIRT;
                    }
                    else if (height <= beachLevel && y + soilDepth > height)
                    {
                        return Game.SAND;
                    }
                    else
                    {
                        return Game.STONE;
                    }
                }
            }
        }

        public IEnumerable<Block> GenerateColumn(int x, int z)
        {
            int height = (int)(amplitude * halfHeight * noise.GetPerlinFractal(x, z)) + halfHeight;

            for (int y = 0; y < Section.SectionSize * Chunk.ChunkHeight; y++)
            {
                if (y == 0)
                {
                    yield return Game.COBBLESTONE;
                }
                else if (y > height)
                {
                    yield return Game.AIR;
                }
                else
                {
                    if (noise.GetCellular(x, z, y) > caveTreshold + (y / (halfHeight * caveLifter)))
                    {
                        yield return Game.AIR;
                    }
                    else if (y == height)
                    {
                        if (y >= snowLevel)
                        {
                            yield return Game.STONE;
                        }
                        else if (y <= beachLevel)
                        {
                            yield return Game.SAND;
                        }
                        else
                        {
                            yield return Game.GRASS;
                        }
                    }
                    else
                    {
                        if (height < snowLevel && height > beachLevel && y + soilDepth > height)
                        {
                            yield return Game.DIRT;
                        }
                        else if (height <= beachLevel && y + soilDepth > height)
                        {
                            yield return Game.SAND;
                        }
                        else
                        {
                            yield return Game.STONE;
                        }
                    }
                }
            }           
        }
    }
}
