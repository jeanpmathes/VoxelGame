// <copyright file="Cover.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Utilities.Units;
using VoxelGame.Toolkit.Noise;

namespace VoxelGame.Core.Generation.Default;

/// <summary>
///     Cover is generated on top of the terrain. It can be used for one-block sized elements.
/// </summary>
public class Cover
{
    private const Double FlowerFactor = 0.05;
    private readonly Boolean hasPlants;

    private NoiseGenerator noise = null!;

    /// <summary>
    ///     Create a new cover generator.
    /// </summary>
    /// <param name="hasPlants">Whether the cover should generate plants.</param>
    public Cover(Boolean hasPlants)
    {
        this.hasPlants = hasPlants;
    }

    /// <summary>
    ///     Setup used noise with the generation seed.
    /// </summary>
    public void SetupNoise(NoiseBuilder builder)
    {
        noise = builder
            .WithType(NoiseType.OpenSimplex2)
            .WithFrequency(frequency: 0.5f)
            .Build();
    }

    /// <summary>
    ///     Get the cover for a given block.
    /// </summary>
    public Content GetContent(Vector3i position, Boolean isFilled, in Map.Sample sample)
    {
        if (isFilled) return Content.Default;

        Temperature temperature = sample.GetRealTemperature(position.Y);

        if (temperature.IsFreezing)
            return new Content(Blocks.Instance.Specials.Snow.GetInstance(height: 1), FluidInstance.Default);

        if (!hasPlants) return Content.Default;

        Single value = noise.GetNoise(position);
        value = value > 0 ? value : value + 1;

        if (value < sample.Humidity) return value < sample.Humidity * FlowerFactor ? new Content(Blocks.Instance.Flower) : new Content(Blocks.Instance.TallGrass);

        return Content.Default;
    }
}
