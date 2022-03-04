// <copyright file="NoiseGenerator.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Logic;
using VoxelGame.Logging;

namespace VoxelGame.Core.Generation
{
    /// <summary>
    ///     Generate a complex and rich world. The main generator used.
    /// </summary>
    public class ComplexGenerator : IWorldGenerator
    {
        private const int HalfHeight = Chunk.ChunkHeight / 2;

        private const float Amplitude = 0.6f;

        private const int SnowLevel = 550;
        private const int BeachLevel = 450;
        private const int SoilDepth = 5;

        private const float CaveThreshold = 0.7f;
        private const float CaveLifter = 3.5f;
        private static readonly ILogger logger = LoggingHelper.CreateLogger<ComplexGenerator>();

        private readonly FastNoise noise;

        /// <summary>
        ///     Create a new complex world generator.
        /// </summary>
        /// <param name="seed">The seed to use.</param>
        public ComplexGenerator(int seed)
        {
            noise = new FastNoise(seed);

            // Settings for fractal noise
            noise.SetFractalLacunarity(lacunarity: 0.5f);
            noise.SetFractalGain(gain: 2f);

            // Settings for cellular noise
            noise.SetCellularReturnType(FastNoise.CellularReturnType.Distance2Div);
            noise.SetCellularDistanceFunction(FastNoise.CellularDistanceFunction.Euclidean);
            noise.SetCellularJitter(cellularJitter: 0.4f);

            logger.LogInformation(Events.WorldGeneration, "Created complex world generator");
        }

        /// <inheritdoc />
        public IEnumerable<Block> GenerateColumn(int x, int z)
        {
            int height = (int) (Amplitude * HalfHeight * noise.GetPerlinFractal(x, z)) + HalfHeight;

            for (var y = 0; y < Chunk.ChunkHeight; y++)
                if (y == 0)
                {
                    yield return Block.Rubble;
                }
                else if (y > height)
                {
                    if (y == height + 1 && y is < SnowLevel and > BeachLevel + 1)
                    {
#pragma warning disable S2234 // Parameters should be passed in the correct order
                        if (noise.GetCellular(x, z, y) > CaveThreshold + y / (HalfHeight * CaveLifter) ||
                            noise.GetCellular(x, z, y - 1) > CaveThreshold + (y - 1) / (HalfHeight * CaveLifter))
#pragma warning restore S2234 // Parameters should be passed in the correct order
                            yield return Block.Air;
                        else if (noise.GetWhiteNoise(x, z) > 0) yield return Block.TallGrass;
                        else yield return Block.Flower;
                    }
                    else
                    {
                        yield return Block.Air;
                    }
                }
                else
                {
#pragma warning disable S2234 // Parameters should be passed in the correct order
                    if (noise.GetCellular(x, z, y) > CaveThreshold + y / (HalfHeight * CaveLifter))
#pragma warning restore S2234 // Parameters should be passed in the correct order
                        yield return Block.Air;
                    else if (y == height)
                        yield return y switch
                        {
                            >= SnowLevel => Block.Snow,
                            <= BeachLevel => Block.Sand,
                            _ => Block.Grass
                        };
                    else
                        yield return height switch
                        {
                            < SnowLevel and > BeachLevel when y + SoilDepth > height => Block.Dirt,
                            <= BeachLevel when y + SoilDepth > height => Block.Sand,
                            _ => Block.Stone
                        };
                }
        }
    }
}
