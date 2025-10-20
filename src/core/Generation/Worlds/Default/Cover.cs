// <copyright file="Cover.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Logic.Voxels.Behaviors;
using VoxelGame.Core.Utilities;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Generation.Worlds.Default;

/// <summary>
///     Cover is generated on top of the terrain. It can be used for one-block sized elements.
/// </summary>
public abstract class Cover
{
    /// <summary>
    ///     How the snow is generated.
    /// </summary>
    public enum Snow
    {
        /// <summary>
        ///     No snow is generated.
        /// </summary>
        None,

        /// <summary>
        ///     Normal snow is generated.
        /// </summary>
        Normal,

        /// <summary>
        ///     Pulverized snow is generated.
        /// </summary>
        Pulverized
    }

    private readonly Snow snowMode;

    /// <summary>
    ///     Create a new cover generator.
    /// </summary>
    protected Cover(Snow snow)
    {
        snowMode = snow;
    }

    /// <summary>
    ///     Get the cover for a given block.
    /// </summary>
    public Content GetContent(Vector3i position, Boolean isFilled, Double heightFraction, in Map.PositionClimate climate)
    {
        if (isFilled)
            return Content.Default;

        if (climate.Temperature.IsFreezing && snowMode != Snow.None)
        {
            var maximumHeight = BlockHeight.Maximum.ToInt32();
            Int32 height = MathTools.RoundedToInt(maximumHeight * heightFraction * 0.75);

            height += NumberGenerator.GetPositionDependentNumber(position, mod: 5) switch
            {
                0 => 1,
                1 => -1,
                _ => 0
            };
            

            Block snow = snowMode == Snow.Pulverized
                ? Blocks.Instance.Environment.PulverizedSnow
                : Blocks.Instance.Environment.Snow;

            return new Content(snow.States.GenerationDefault.WithHeight(BlockHeight.FromInt32(height)), FluidInstance.Default);
        }

        return GetCover(position, climate);
    }

    /// <summary>
    ///     Get the cover for a given position.
    /// </summary>
    protected abstract Content GetCover(Vector3i position, in Map.PositionClimate climate);

    /// <summary>
    ///     Cover with no vegetation and no snow.
    /// </summary>
    public class Nothing() : Cover(Snow.None)
    {
        /// <inheritdoc />
        protected override Content GetCover(Vector3i position, in Map.PositionClimate climate)
        {
            return Content.Default;
        }
    }

    /// <summary>
    ///     Cover with no vegetation.
    /// </summary>
    public class NoVegetation(Boolean isSnowPulverized = false) : Cover(isSnowPulverized ? Snow.Pulverized : Snow.Normal)
    {
        /// <inheritdoc />
        protected override Content GetCover(Vector3i position, in Map.PositionClimate climate)
        {
            return Content.Default;
        }
    }

    /// <summary>
    ///     Cover with (tall) grass and flowers.
    /// </summary>
    public class GrassAndFlowers(Boolean isSnowPulverized = false, Boolean isBlooming = false, Boolean mushrooms = false) : Cover(isSnowPulverized ? Snow.Pulverized : Snow.Normal)
    {
        /// <inheritdoc />
        protected override Content GetCover(Vector3i position, in Map.PositionClimate climate)
        {
            Int32 value = NumberGenerator.GetPositionDependentNumber(position, mod: 100);
            Int32 humidity = MathTools.RoundedToInt(climate.SampledHumidity * 100);

            if (value >= humidity)
                return Content.Default;

            Double flowerFactor = isBlooming ? 0.10 : 0.05;

            if (value >= humidity * flowerFactor)
                return value % 2 == 0 ? new Content(Blocks.Instance.Environment.TallGrass) : new Content(Blocks.Instance.Environment.TallerGrass);

            if (mushrooms)
                return (value % 3) switch
                {
                    0 => new Content(Blocks.Instance.Flowers.FlowerRed.Short),
                    1 => new Content(Blocks.Instance.Flowers.FlowerYellow.Short),
                    _ => new Content(Blocks.Instance.Organic.Chanterelle)
                };

            return new Content(value % 2 == 0 ? Blocks.Instance.Flowers.FlowerRed.Short : Blocks.Instance.Flowers.FlowerYellow.Short);
        }
    }

    /// <summary>
    ///     Cover with (tall) grass.
    /// </summary>
    public class Grass(Boolean isSnowPulverized = false, Boolean hasSucculents = false) : Cover(isSnowPulverized ? Snow.Pulverized : Snow.Normal)
    {
        /// <inheritdoc />
        protected override Content GetCover(Vector3i position, in Map.PositionClimate climate)
        {
            Int32 value = NumberGenerator.GetPositionDependentNumber(position, mod: 100);
            Int32 humidity = MathTools.RoundedToInt(climate.SampledHumidity * 100);

            if (value >= humidity)
                return Content.Default;

            if (hasSucculents)
                return (value % 3) switch
                {
                    0 => new Content(Blocks.Instance.Organic.AloeVera),
                    1 => new Content(Blocks.Instance.Environment.TallGrass),
                    _ => new Content(Blocks.Instance.Environment.TallerGrass)
                };

            return value % 2 == 0 ? new Content(Blocks.Instance.Environment.TallGrass) : new Content(Blocks.Instance.Environment.TallerGrass);
        }
    }

    /// <summary>
    ///     Cover with lichen.
    /// </summary>
    public class Lichen(Lichen.Density density, Boolean isSnowPulverized = false) : Cover(isSnowPulverized ? Snow.Pulverized : Snow.Normal)
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
        protected override Content GetCover(Vector3i position, in Map.PositionClimate climate)
        {
            return NumberGenerator.GetPositionDependentNumber(position, draw.mod) > draw.threshold
                ? new Content(Blocks.Instance.Organic.Lichen)
                : Content.Default;
        }
    }

    /// <summary>
    ///     Cover with moss and some lichen.
    /// </summary>
    public class Moss(Boolean isSnowPulverized = false) : Cover(isSnowPulverized ? Snow.Pulverized : Snow.Normal)
    {
        /// <inheritdoc />
        protected override Content GetCover(Vector3i position, in Map.PositionClimate climate)
        {
            Int32 value = NumberGenerator.GetPositionDependentNumber(position, mod: 10);

            return value switch
            {
                0 => new Content(Blocks.Instance.Organic.Lichen),
                < 7 => new Content(Blocks.Instance.Organic.Moss),
                _ => Content.Default
            };
        }
    }

    /// <summary>
    ///     Cover with salt and no vegetation.
    /// </summary>
    public class Salt(Boolean isSnowPulverized = false) : Cover(isSnowPulverized ? Snow.Pulverized : Snow.Normal)
    {
        /// <inheritdoc />
        protected override Content GetCover(Vector3i position, in Map.PositionClimate climate)
        {
            Int32 value = NumberGenerator.GetPositionDependentNumber(position, mod: 10);

            return value < 6 ? new Content(Blocks.Instance.Environment.Salt) : Content.Default;
        }
    }

    /// <summary>
    ///     Cover with fern, and some moss.
    /// </summary>
    public class Fern(Boolean isSnowPulverized = false) : Cover(isSnowPulverized ? Snow.Pulverized : Snow.Normal)
    {
        /// <inheritdoc />
        protected override Content GetCover(Vector3i position, in Map.PositionClimate climate)
        {
            Int32 value = NumberGenerator.GetPositionDependentNumber(position, mod: 10);

            return value switch
            {
                < 2 => new Content(Blocks.Instance.Organic.Moss),
                < 7 => new Content(Blocks.Instance.Organic.Fern),
                _ => Content.Default
            };
        }
    }
}
