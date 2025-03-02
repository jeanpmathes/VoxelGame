// <copyright file="Cover.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Units;
using VoxelGame.Toolkit.Noise;

namespace VoxelGame.Core.Generation.Worlds.Default;

/// <summary>
///     Cover is generated on top of the terrain. It can be used for one-block sized elements.
/// </summary>
public sealed class Cover
{
    private const Double FlowerFactor = 0.05;
    private readonly Boolean hasPlants;

    /// <summary>
    ///     Create a new cover generator.
    /// </summary>
    /// <param name="hasPlants">Whether the cover should generate plants.</param>
    public Cover(Boolean hasPlants)
    {
        this.hasPlants = hasPlants;
    }

    /// <summary>
    ///     Get the cover for a given block.
    /// </summary>
    public Content GetContent(NoiseGenerator noise, Vector3i position, Boolean isFilled, Double heightFraction, in Map.Sample sample)
    {
        if (isFilled) return Content.Default;

        Temperature temperature = sample.GetRealTemperature(position.Y);

        if (temperature.IsFreezing)
        {
            Int32 height = MathTools.RoundedToInt(IHeightVariable.MaximumHeight * heightFraction * 0.75);

            height += BlockUtilities.GetPositionDependentNumber(position, mod: 5) switch
            {
                0 => 1,
                1 => -1,
                _ => 0
            };

            height = Math.Clamp(height, min: 0, IHeightVariable.MaximumHeight);

            return new Content(Blocks.Instance.Specials.Snow.GetInstance(height), FluidInstance.Default);
        }

        if (!hasPlants) return Content.Default;

        // No grid noise is used here because this method is only called for a single block per column.

        Single value = noise.GetNoise(position);
        value = value > 0 ? value : value + 1;

        if (value < sample.Humidity) return value < sample.Humidity * FlowerFactor ? new Content(Blocks.Instance.Flower) : new Content(Blocks.Instance.TallGrass);

        return Content.Default;
    }
}
