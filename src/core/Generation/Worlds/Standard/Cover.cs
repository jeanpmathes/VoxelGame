// <copyright file="Cover.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Logic.Voxels.Behaviors;
using VoxelGame.Core.Logic.Voxels.Behaviors.Nature.Plants;
using VoxelGame.Core.Utilities;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Generation.Worlds.Standard;

/// <summary>
///     Cover is generated on top of the terrain. It can be used for one-block sized elements.
/// </summary>
public abstract class Cover
{
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

    private static Content GetTallGrassContent(TallGrass.StageState stageState)
    {
        return new Content(TallGrass.GetState(Blocks.Instance.Environment.TallGrass.States.GenerationDefault, stageState), FluidInstance.Default);
    }

    /// <summary>
    ///     How the snow is generated.
    /// </summary>
    protected enum Snow
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
                return value % 2 == 0
                    ? GetTallGrassContent(TallGrass.StageState.Short)
                    : GetTallGrassContent(TallGrass.StageState.Tall);

            if (mushrooms)
                return (value % 3) switch
                {
                    0 => Content.CreateGenerated(Blocks.Instance.Flowers.FlowerRed.Short),
                    1 => Content.CreateGenerated(Blocks.Instance.Flowers.FlowerYellow.Short),
                    _ => Content.CreateGenerated(Blocks.Instance.Organic.Chanterelle)
                };

            return Content.CreateGenerated(value % 2 == 0 ? Blocks.Instance.Flowers.FlowerRed.Short : Blocks.Instance.Flowers.FlowerYellow.Short);
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
                    0 => Content.CreateGenerated(Blocks.Instance.Organic.AloeVera),
                    1 => GetTallGrassContent(TallGrass.StageState.Short),
                    _ => GetTallGrassContent(TallGrass.StageState.Tall)
                };

            return value % 2 == 0
                ? GetTallGrassContent(TallGrass.StageState.Short)
                : GetTallGrassContent(TallGrass.StageState.Tall);
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
                ? Content.CreateGenerated(Blocks.Instance.Organic.Lichen)
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
                0 => Content.CreateGenerated(Blocks.Instance.Organic.Lichen),
                < 7 => Content.CreateGenerated(Blocks.Instance.Organic.Moss),
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

            return value < 6 ? Content.CreateGenerated(Blocks.Instance.Environment.Salt) : Content.Default;
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
                < 2 => Content.CreateGenerated(Blocks.Instance.Organic.Moss),
                < 7 => Content.CreateGenerated(Blocks.Instance.Organic.Fern),
                _ => Content.GenerationDefault
            };
        }
    }
}
