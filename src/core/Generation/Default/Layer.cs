﻿// <copyright file="Layer.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Interfaces;

namespace VoxelGame.Core.Generation.Default;

/// <summary>
///     A layer of the world.
/// </summary>
public abstract class Layer
{
    /// <summary>
    ///     The width of the layer, in number of blocks.
    /// </summary>
    public int Width { get; protected init; }

    /// <summary>
    ///     Whether this layer is a dampen layer that requires special handling.
    /// </summary>
    public bool IsDampen { get; protected init; }

    /// <summary>
    ///     Get whether this layer is solid and does not allow water to pass through.
    /// </summary>
    public bool IsSolid { get; protected init; }

    /// <summary>
    ///     Get the current palette, if there is any.
    ///     There will always be a palette when <see cref="GetContent" /> is called.
    /// </summary>
    protected Palette? Palette { get; private set; }

    /// <summary>
    ///     Set the currently used Palette.
    /// </summary>
    public void SetPalette(Palette newPalette)
    {
        Palette = newPalette;
    }

    /// <summary>
    ///     Create a dampening layer that absorbs some of the offset. This is a meta layer and is assumed to be fillable.
    /// </summary>
    public static Layer CreatePermeableDampen(Block block, int maxWidth)
    {
        return new PermeableDampen(block, maxWidth);
    }

    /// <summary>
    ///     Create a dampening layer that absorbs some of the offset. This is a meta layer and uses stone blocks.
    /// </summary>
    public static Layer CreateStonyDampen(int maxWidth)
    {
        return new StonyDampen(maxWidth);
    }

    /// <summary>
    ///     Create a stony cover layer that simulates erosion.
    /// </summary>
    public static Layer CreateStonyCover(int width, int amplitude)
    {
        return new StonyCover(width, amplitude);
    }

    /// <summary>
    /// Create a cover layer, which selects an alternative when filled. The alternative block is also filled with water if possible.
    /// </summary>
    public static Layer CreateCover(Block cover, Block filled, int width)
    {
        return new Cover(cover, filled, width);
    }

    /// <summary>
    ///     Create a layer with a permeable material that will be filled with water.
    /// </summary>
    public static Layer CreatePermeable(Block block, int width)
    {
        return new Permeable(block, width);
    }

    /// <summary>
    ///     Create a solid layer, which always has the same block.
    /// </summary>
    public static Layer CreateSolid(Block block, int width)
    {
        return new Solid(block, width);
    }

    /// <summary>
    ///     Create a layer with ground water that uses a loose block depending on stone type.
    /// </summary>
    public static Layer CreateGroundwater(int width)
    {
        return new Groundwater(width);
    }

    /// <summary>
    ///     Create a layer with loose material, depending on stone type.
    /// </summary>
    public static Layer CreateLoose(int width)
    {
        return new Loose(width);
    }

    /// <summary>
    ///     Create a snow layer.
    /// </summary>
    public static Layer CreateSnow(int width)
    {
        return new Snow(width);
    }

    /// <summary>
    ///     Create a stone layer.
    /// </summary>
    public static Layer CreateStone(int width)
    {
        return new Stone(width);
    }

    /// <summary>
    ///     Returns the data for the layer content.
    /// </summary>
    /// <param name="depth">The depth in the layer.</param>
    /// <param name="offset">The random offset from normal world height.</param>
    /// <param name="stoneType">The stone type of the column.</param>
    /// <param name="isFilled">Whether the column is filled with fluid or not.</param>
    /// <returns>The data for the layer content.</returns>
    public abstract Content GetContent(int depth, int offset, Map.StoneType stoneType, bool isFilled);

    private sealed class Cover : Layer
    {
        private readonly Content filledData;
        private readonly Content normalData;

        public Cover(Block cover, Block filled, int width)
        {
            Width = width;

            normalData = new Content(cover);
            filledData = filled is IFillable ? new Content(filled, Fluid.Water) : new Content(filled);
        }

        public override Content GetContent(int depth, int offset, Map.StoneType stoneType, bool isFilled)
        {
            return isFilled ? filledData : normalData;
        }
    }

    private sealed class Permeable : Layer
    {
        private readonly Content filled;
        private readonly Content normal;

        public Permeable(Block block, int width)
        {
            Width = width;

            normal = new Content(block);
            filled = new Content(block, Fluid.Water);
        }

        public override Content GetContent(int depth, int offset, Map.StoneType stoneType, bool isFilled)
        {
            return isFilled ? filled : normal;
        }
    }

    private sealed class Solid : Layer
    {
        private readonly Content data;

        public Solid(Block block, int width)
        {
            Width = width;
            IsSolid = true;

            data = new Content(block);
        }

        public override Content GetContent(int depth, int offset, Map.StoneType stoneType, bool isFilled)
        {
            return data;
        }
    }

    private sealed class Groundwater : Layer
    {
        private readonly int groundWaterDepth;

        public Groundwater(int width)
        {
            Width = width;

            groundWaterDepth = width / 2;
        }

        public override Content GetContent(int depth, int offset, Map.StoneType stoneType, bool isFilled)
        {
            if (isFilled) return Palette!.GetLoose(stoneType, isFilled);

            int actualDepth = depth - offset;

            return actualDepth >= groundWaterDepth ? Palette!.GetGroundwater(stoneType) : Palette!.GetLoose(stoneType, isFilled);
        }
    }

    private sealed class Snow : Layer
    {
        private readonly Content filled;
        private readonly Content snow;

        public Snow(int width)
        {
            Width = width;

            snow = new Content(Block.Specials.Snow.FullHeightInstance, FluidInstance.Default);
            filled = new Content(fluid: Fluid.Water);
        }

        public override Content GetContent(int depth, int offset, Map.StoneType stoneType, bool isFilled)
        {
            return isFilled ? filled : snow;
        }
    }

    private sealed class Loose : Layer
    {
        public Loose(int width)
        {
            Width = width;
        }

        public override Content GetContent(int depth, int offset, Map.StoneType stoneType, bool isFilled)
        {
            return Palette!.GetLoose(stoneType, isFilled);
        }
    }

    private sealed class PermeableDampen : Layer
    {
        private readonly Content blockFilled;
        private readonly Content blockNormal;

        public PermeableDampen(Block block, int maxWidth)
        {
            Width = maxWidth;
            IsDampen = true;

            blockNormal = new Content(block);
            blockFilled = new Content(block, Fluid.Water);
        }

        public override Content GetContent(int depth, int offset, Map.StoneType stoneType, bool isFilled)
        {
            return isFilled ? blockFilled : blockNormal;
        }
    }

    private sealed class StonyDampen : Layer
    {
        public StonyDampen(int maxWidth)
        {
            Width = maxWidth;
            IsDampen = true;
        }

        public override Content GetContent(int depth, int offset, Map.StoneType stoneType, bool isFilled)
        {
            return Palette!.GetStone(stoneType);
        }
    }

    private sealed class Stone : Layer
    {
        public Stone(int width)
        {
            Width = width;
            IsSolid = true;
        }

        public override Content GetContent(int depth, int offset, Map.StoneType stoneType, bool isFilled)
        {
            return Palette!.GetStone(stoneType);
        }
    }

    private sealed class StonyCover : Layer
    {
        private readonly int amplitude;
        private readonly Content dirt;
        private readonly Content dirtFilled;

        private readonly Content grass;
        private readonly Content grassFilled;

        public StonyCover(int width, int amplitude)
        {
            Width = width;

            dirt = new Content(Block.Dirt);
            dirtFilled = new Content(Block.Dirt, Fluid.Water);

            grass = new Content(Block.Grass);
            grassFilled = new Content(Block.Grass, Fluid.Water);

            this.amplitude = amplitude;
        }

        public override Content GetContent(int depth, int offset, Map.StoneType stoneType, bool isFilled)
        {
            if (offset > amplitude)
            {
                if (depth == 0) return isFilled ? grassFilled : grass;

                return isFilled ? dirtFilled : dirt;
            }

            if (offset < -amplitude) return Palette!.GetLoose(stoneType, isFilled);

            return Palette!.GetStone(stoneType);
        }
    }
}
