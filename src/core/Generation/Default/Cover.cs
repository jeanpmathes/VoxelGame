﻿// <copyright file="Cover.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Generation.Default;

/// <summary>
///     Cover is generated on top of the terrain. It can be used for one-block sized elements.
/// </summary>
public class Cover
{
    private const double FlowerFactor = 0.05;
    private readonly bool hasPlants;

    private FastNoiseLite noise = null!;

    /// <summary>
    ///     Create a new cover generator.
    /// </summary>
    /// <param name="hasPlants">Whether the cover should generate plants.</param>
    public Cover(bool hasPlants)
    {
        this.hasPlants = hasPlants;
    }

    /// <summary>
    /// Setup used noise with the generation seed.
    /// </summary>
    /// <param name="noiseGenerator">The noise generator to use.</param>
    public void SetupNoise(FastNoiseLite noiseGenerator)
    {
        noise = noiseGenerator;

        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetFrequency(frequency: 0.5f);
    }

    /// <summary>
    ///     Get the cover for a given block.
    /// </summary>
    public Content GetContent(Vector3i position, bool isFilled, in Map.Sample sample)
    {
        if (isFilled) return Content.Default;

        double temperature = sample.GetTemperatureInCelsius(position.Y);

        if (temperature < 0) return new Content(Blocks.Instance.Specials.Snow.GetInstance(height: 1), FluidInstance.Default);

        if (hasPlants)
        {
            float value = noise.GetNoise(position.X, position.Y, position.Z);
            value = value > 0 ? value : value + 1;

            if (value < sample.Humidity) return value < sample.Humidity * FlowerFactor ? new Content(Blocks.Instance.Flower) : new Content(Blocks.Instance.TallGrass);
        }

        return Content.Default;
    }
}

