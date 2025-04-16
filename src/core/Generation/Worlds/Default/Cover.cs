// <copyright file="Cover.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Definitions.Blocks;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Units;

namespace VoxelGame.Core.Generation.Worlds.Default;

/// <summary>
///     Cover is generated on top of the terrain. It can be used for one-block sized elements.
/// </summary>
public sealed class Cover
{
    private const Double FlowerFactor = 0.05;

    private readonly Boolean hasPlants;
    private readonly Boolean isSnowLoose;

    /// <summary>
    ///     Create a new cover generator.
    /// </summary>
    /// <param name="hasPlants">Whether the cover should generate plants.</param>
    /// <param name="isSnowLoose">Whether snow is placed as normal or loose snow.</param>
    public Cover(Boolean hasPlants, Boolean isSnowLoose = false)
    {
        this.hasPlants = hasPlants;
        this.isSnowLoose = isSnowLoose;
    }

    /// <summary>
    ///     Get the cover for a given block.
    /// </summary>
    public Content GetContent(Vector3i position, Boolean isFilled, Double heightFraction, in Map.Sample sample)
    {
        if (isFilled) return Content.Default;

        Temperature temperature = sample.EstimateTemperature(position.Y);

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

            SnowBlock snow = isSnowLoose
                ? Blocks.Instance.Specials.LooseSnow
                : Blocks.Instance.Specials.Snow;

            return new Content(snow.GetInstance(height), FluidInstance.Default);
        }

        if (!hasPlants) return Content.Default;

        Int32 value = BlockUtilities.GetPositionDependentNumber(position, mod: 100);
        Int32 humidity = MathTools.RoundedToInt(sample.Humidity * 100);

        if (value >= humidity)
            return Content.Default;

        if (value < humidity * FlowerFactor)
            return new Content(Blocks.Instance.Flower);

        return value % 2 == 0 ? new Content(Blocks.Instance.TallGrass) : new Content(Blocks.Instance.TallerGrass);
    }
}
