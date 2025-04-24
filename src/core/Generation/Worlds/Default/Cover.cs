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
public abstract class Cover
{
    private readonly Boolean isSnowLoose;

    /// <summary>
    /// Create a new cover generator.
    /// </summary>
    /// <param name="isSnowLoose">Whether generated snow is loose or not.</param>
    protected Cover(Boolean isSnowLoose)
    {
        this.isSnowLoose = isSnowLoose;
    }

    /// <summary>
    ///     Get the cover for a given block.
    /// </summary>
    public Content GetContent(Vector3i position, Boolean isFilled, Double heightFraction, in Map.Sample sample)
    {
        if (isFilled)
            return Content.Default;

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

        return GetCover(position, sample);
    }

    /// <summary>
    ///     Get the cover for a given position.
    /// </summary>
    protected abstract Content GetCover(Vector3i position, in Map.Sample sample);

    /// <summary>
    ///     Cover with no vegetation.
    /// </summary>
    public class NoVegetation(Boolean isSnowLoose = false) : Cover(isSnowLoose)
    {
        /// <inheritdoc />
        protected override Content GetCover(Vector3i position, in Map.Sample sample)
        {
            return Content.Default;
        }
    }

    /// <summary>
    ///     Cover with (tall) grass and flowers.
    /// </summary>
    public class Grass(Boolean isSnowLoose = false, Boolean isBlooming = false) : Cover(isSnowLoose)
    {
        /// <inheritdoc />
        protected override Content GetCover(Vector3i position, in Map.Sample sample)
        {
            Int32 value = BlockUtilities.GetPositionDependentNumber(position, mod: 100);
            Int32 humidity = MathTools.RoundedToInt(sample.Humidity * 100);

            if (value >= humidity)
                return Content.Default;

            Double flowerFactor = isBlooming ? 0.10 : 0.05;

            if (value < humidity * flowerFactor)
                return new Content(value % 2 == 0 ? Blocks.Instance.RedFlower : Blocks.Instance.YellowFlower);

            return value % 2 == 0 ? new Content(Blocks.Instance.TallGrass) : new Content(Blocks.Instance.TallerGrass);
        }
    }

    /// <summary>
    ///     Cover with lichen.
    /// </summary>
    public class Lichen(Lichen.Density density, Boolean isSnowLoose = false) : Cover(isSnowLoose)
    {
        /// <summary>
        ///     How dense the lichen is.
        /// </summary>
        public enum Density
        {
            /// <summary>
            ///     The lichen is dense.
            /// </summary>
            High,

            /// <summary>
            ///     The lichen is not dense.
            /// </summary>
            Low
        }

        private readonly (Int32 mod, Int32 threshold) draw = density switch
        {
            Density.High => (3, 0), // Chance: 66%
            Density.Low => (5, 3), // Chance: 20%
            _ => throw Exceptions.UnsupportedEnumValue(density)
        };

        /// <inheritdoc />
        protected override Content GetCover(Vector3i position, in Map.Sample sample)
        {
            return BlockUtilities.GetPositionDependentNumber(position, draw.mod) > draw.threshold
                ? new Content(Blocks.Instance.Lichen)
                : Content.Default;
        }
    }

    /// <summary>
    ///     Cover with moss and some lichen.
    /// </summary>
    public class Moss(Boolean isSnowLoose = false) : Cover(isSnowLoose)
    {
        /// <inheritdoc />
        protected override Content GetCover(Vector3i position, in Map.Sample sample)
        {
            Int32 value = BlockUtilities.GetPositionDependentNumber(position, mod: 10);

            return value switch
            {
                0 => new Content(Blocks.Instance.Lichen),
                < 7 => new Content(Blocks.Instance.Moss),
                _ => Content.Default
            };
        }
    }

    /// <summary>
    ///     Cover with fern, and some moss.
    /// </summary>
    public class Fern(Boolean isSnowLoose = false) : Cover(isSnowLoose)
    {
        /// <inheritdoc />
        protected override Content GetCover(Vector3i position, in Map.Sample sample)
        {
            Int32 value = BlockUtilities.GetPositionDependentNumber(position, mod: 10);

            return value switch
            {
                < 2 => new Content(Blocks.Instance.Moss),
                < 7 => new Content(Blocks.Instance.Fern),
                _ => Content.Default
            };
        }
    }
}
