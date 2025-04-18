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
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Generation.Worlds.Default;

/// <summary>
///     Cover is generated on top of the terrain. It can be used for one-block sized elements.
/// </summary>
public sealed class Cover
{
    /// <summary>
    ///     What vegetation to generate as part of the cover.
    /// </summary>
    public enum Vegetation
    {
        /// <summary>
        ///     No vegetation.
        /// </summary>
        None,

        /// <summary>
        ///     Normal vegetation, meaning grass and flowers.
        /// </summary>
        Normal,

        /// <summary>
        ///     Lichen.
        /// </summary>
        Lichen
    }

    private const Double FlowerFactor = 0.05;

    private readonly Vegetation vegetation;
    private readonly Boolean isSnowLoose;

    /// <summary>
    ///     Create a new cover generator.
    /// </summary>
    /// <param name="vegetation">The type of vegetation to generate.</param>
    /// <param name="isSnowLoose">Whether snow is placed as normal or loose snow.</param>
    public Cover(Vegetation vegetation, Boolean isSnowLoose = false)
    {
        this.vegetation = vegetation;
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

        switch (vegetation)
        {
            case Vegetation.None:
                return Content.Default;

            case Vegetation.Normal:
                Int32 value = BlockUtilities.GetPositionDependentNumber(position, mod: 100);
                Int32 humidity = MathTools.RoundedToInt(sample.Humidity * 100);

                if (value >= humidity)
                    return Content.Default;

                if (value < humidity * FlowerFactor)
                    return new Content(Blocks.Instance.Flower);

                return value % 2 == 0 ? new Content(Blocks.Instance.TallGrass) : new Content(Blocks.Instance.TallerGrass);

            case Vegetation.Lichen:
                return BlockUtilities.GetPositionDependentNumber(position, mod: 3) != 0
                    ? new Content(Blocks.Instance.Lichen)
                    : Content.Default;

            default:
                throw Exceptions.UnsupportedEnumValue(vegetation);
        }
    }
}
