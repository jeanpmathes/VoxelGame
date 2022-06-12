// <copyright file="NoiseGenerator.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Logic;
using VoxelGame.Logging;

namespace VoxelGame.Core.Generation;

/// <summary>
///     Generate a complex and rich world. The main generator used.
/// </summary>
public class ComplexGenerator : IWorldGenerator
{
    private const int AverageHeight = 900;
    private const int HalfAverageHeight = AverageHeight / 2;

    private const int SnowLevel = 500;
    private const int BeachLevel = 400;
    private const int SoilDepth = 5;

    private const int CaveHeight = -200;
    private const float CaveSpread = 100;

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
    public IEnumerable<Block> GenerateColumn(int x, int z, (int start, int end) heightRange)
    {
        int height = (int) (HalfAverageHeight * noise.GetPerlinFractal(x, z)) + HalfAverageHeight;

        bool HasCave(int cx, int cy, int cz)
        {
            double cave = noise.GetCellular(cx, cz, cy);

            double distance = Math.Abs(cy - CaveHeight);
            double factor = Math.Clamp(distance / CaveSpread, min: 0, max: 1);

            return cave > factor;
        }

        for (int y = heightRange.start; y < heightRange.end; y++)
            if (y > height)
            {
                if (y == height + 1 && y is < SnowLevel and > BeachLevel + 1)
                {
                    if (HasCave(x, y, z) || HasCave(x, y - 1, z))
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
                if (HasCave(x, y, z))
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
