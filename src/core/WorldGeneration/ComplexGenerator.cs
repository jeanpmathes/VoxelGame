// <copyright file="NoiseGenerator.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using VoxelGame.Core.Logic;
using VoxelGame.Logging;

namespace VoxelGame.Core.WorldGeneration
{
    public class ComplexGenerator : IWorldGenerator
    {
        private static readonly ILogger Logger = LoggingHelper.CreateLogger<ComplexGenerator>();

        private readonly FastNoise noise;
        private readonly int halfHeight = Chunk.ChunkHeight / 2;

        private readonly float amplitude = 0.6f;

        private readonly int snowLevel = 550;
        private readonly int beachLevel = 450;
        private readonly int soilDepth = 5;

        private readonly float caveTreshold = 0.7f;
        private readonly float caveLifter = 3.5f;

        public ComplexGenerator(int seed)
        {
            noise = new FastNoise(seed);

            // Settings for fractal noise
            noise.SetFractalLacunarity(0.5f);
            noise.SetFractalGain(2f);

            // Settings for cellular noise
            noise.SetCellularReturnType(FastNoise.CellularReturnType.Distance2Div);
            noise.SetCellularDistanceFunction(FastNoise.CellularDistanceFunction.Euclidean);
            noise.SetCellularJitter(0.4f);

            Logger.LogInformation("Created an IWorldGenerator of type Complex.");
        }

        public IEnumerable<Block> GenerateColumn(int x, int z)
        {
            int height = (int)(amplitude * halfHeight * noise.GetPerlinFractal(x, z)) + halfHeight;

            for (var y = 0; y < Chunk.ChunkHeight; y++)
            {
                if (y == 0)
                {
                    yield return Block.Rubble;
                }
                else if (y > height)
                {
                    if (y == height + 1 && y < snowLevel && y > beachLevel + 1)
                    {
#pragma warning disable S2234 // Parameters should be passed in the correct order
                        if (noise.GetCellular(x, z, y) > caveTreshold + (y / (halfHeight * caveLifter)) || noise.GetCellular(x, z, y - 1) > caveTreshold + ((y - 1) / (halfHeight * caveLifter)))
#pragma warning restore S2234 // Parameters should be passed in the correct order
                        {
                            yield return Block.Air;
                        }
                        else if (noise.GetWhiteNoise(x, z) > 0)
                        {
                            yield return Block.TallGrass;
                        }
                        else
                        {
                            yield return Block.Flower;
                        }
                    }
                    else
                    {
                        yield return Block.Air;
                    }
                }
                else
                {
#pragma warning disable S2234 // Parameters should be passed in the correct order
                    if (noise.GetCellular(x, z, y) > caveTreshold + (y / (halfHeight * caveLifter)))
#pragma warning restore S2234 // Parameters should be passed in the correct order
                    {
                        yield return Block.Air;
                    }
                    else if (y == height)
                    {
                        if (y >= snowLevel)
                        {
                            yield return Block.Snow;
                        }
                        else if (y <= beachLevel)
                        {
                            yield return Block.Sand;
                        }
                        else
                        {
                            yield return Block.Grass;
                        }
                    }
                    else
                    {
                        if (height < snowLevel && height > beachLevel && y + soilDepth > height)
                        {
                            yield return Block.Dirt;
                        }
                        else if (height <= beachLevel && y + soilDepth > height)
                        {
                            yield return Block.Sand;
                        }
                        else
                        {
                            yield return Block.Stone;
                        }
                    }
                }
            }
        }
    }
}