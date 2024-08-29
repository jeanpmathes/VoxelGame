// <copyright file="Layer.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using VoxelGame.Core.Logic.Elements;
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
    public Int32 Width { get; private init; }

    /// <summary>
    ///     Whether this layer is a dampen layer that requires special handling.
    /// </summary>
    public Boolean IsDampen { get; private init; }

    /// <summary>
    ///     Get whether this layer is solid and does not allow water to pass through.
    /// </summary>
    public Boolean IsSolid { get; private init; }

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
    public static Layer CreatePermeableDampen(Block block, Int32 maxWidth)
    {
        return new PermeableDampen(block, maxWidth);
    }

    /// <summary>
    ///     Create a dampening layer that absorbs some of the offset. This is a meta layer and uses stone blocks.
    /// </summary>
    public static Layer CreateStonyDampen(Int32 maxWidth)
    {
        return new StonyDampen(maxWidth);
    }

    /// <summary>
    ///     Create a stony top layer that simulates erosion.
    /// </summary>
    public static Layer CreateStonyTop(Int32 width, Int32 amplitude)
    {
        return new StonyTop(width, amplitude);
    }

    /// <summary>
    ///     Create a top layer, which selects an alternative when filled. The alternative block is also filled with water if
    ///     possible.
    /// </summary>
    public static Layer CreateTop(Block top, Block filled, Int32 width)
    {
        return new Top(top, filled, width);
    }

    /// <summary>
    ///     Create a simple layer. It can be declared as solid, which is only valid when not fillable.
    /// </summary>
    public static Layer CreateSimple(Block block, Int32 width, Boolean isSolid)
    {
        if (isSolid) Debug.Assert(block is not IFillable);

        return new Simple(block, width, isSolid);
    }

    /// <summary>
    ///     Create a layer with ground water that uses a loose block depending on stone type.
    /// </summary>
    public static Layer CreateGroundwater(Int32 width)
    {
        return new Groundwater(width);
    }

    /// <summary>
    ///     Create a layer with loose material, depending on stone type.
    /// </summary>
    public static Layer CreateLoose(Int32 width)
    {
        return new Loose(width);
    }

    /// <summary>
    ///     Create a snow layer. Snow will not generated when filled.
    /// </summary>
    public static Layer CreateSnow(Int32 width)
    {
        return new Snow(width);
    }

    /// <summary>
    ///     Create a stone layer.
    /// </summary>
    public static Layer CreateStone(Int32 width)
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
    public abstract Content GetContent(Int32 depth, Int32 offset, Map.StoneType stoneType, Boolean isFilled);

    private sealed class Top : Layer
    {
        private readonly Content filledData;
        private readonly Content normalData;

        public Top(Block top, Block filled, Int32 width)
        {
            Width = width;

            normalData = new Content(top);
            filledData = new Content(filled);
        }

        public override Content GetContent(Int32 depth, Int32 offset, Map.StoneType stoneType, Boolean isFilled)
        {
            return isFilled ? filledData : normalData;
        }
    }

    private sealed class Simple : Layer
    {
        private readonly Content data;

        public Simple(Block block, Int32 width, Boolean isSolid)
        {
            Width = width;
            IsSolid = isSolid;

            data = new Content(block);
        }

        public override Content GetContent(Int32 depth, Int32 offset, Map.StoneType stoneType, Boolean isFilled)
        {
            return data;
        }
    }

    private sealed class Groundwater : Layer
    {
        private readonly Int32 groundWaterDepth;

        public Groundwater(Int32 width)
        {
            Width = width;

            groundWaterDepth = width / 2;
        }

        public override Content GetContent(Int32 depth, Int32 offset, Map.StoneType stoneType, Boolean isFilled)
        {
            if (isFilled) return Palette!.GetLoose(stoneType);

            Int32 actualDepth = depth - offset;

            return actualDepth >= groundWaterDepth ? Palette!.GetGroundwater(stoneType) : Palette!.GetLoose(stoneType);
        }
    }

    private sealed class Snow : Layer
    {
        private readonly Content filled;
        private readonly Content snow;

        public Snow(Int32 width)
        {
            Width = width;

            snow = new Content(Blocks.Instance.Specials.Snow.FullHeightInstance, FluidInstance.Default);
            filled = Content.Default;
        }

        public override Content GetContent(Int32 depth, Int32 offset, Map.StoneType stoneType, Boolean isFilled)
        {
            return isFilled ? filled : snow;
        }
    }

    private sealed class Loose : Layer
    {
        public Loose(Int32 width)
        {
            Width = width;
        }

        public override Content GetContent(Int32 depth, Int32 offset, Map.StoneType stoneType, Boolean isFilled)
        {
            return Palette!.GetLoose(stoneType);
        }
    }

    private sealed class PermeableDampen : Layer
    {
        private readonly Content data;

        public PermeableDampen(Block block, Int32 maxWidth)
        {
            Width = maxWidth;
            IsDampen = true;

            data = new Content(block);
        }

        public override Content GetContent(Int32 depth, Int32 offset, Map.StoneType stoneType, Boolean isFilled)
        {
            return data;
        }
    }

    private sealed class StonyDampen : Layer
    {
        public StonyDampen(Int32 maxWidth)
        {
            Width = maxWidth;
            IsDampen = true;
        }

        public override Content GetContent(Int32 depth, Int32 offset, Map.StoneType stoneType, Boolean isFilled)
        {
            return Palette!.GetStone(stoneType);
        }
    }

    private sealed class Stone : Layer
    {
        public Stone(Int32 width)
        {
            Width = width;
            IsSolid = true;
        }

        public override Content GetContent(Int32 depth, Int32 offset, Map.StoneType stoneType, Boolean isFilled)
        {
            return Palette!.GetStone(stoneType);
        }
    }

    private sealed class StonyTop : Layer
    {
        private readonly Int32 amplitude;
        private readonly Content dirt;
        private readonly Content grass;

        public StonyTop(Int32 width, Int32 amplitude)
        {
            Width = width;

            dirt = new Content(Blocks.Instance.Dirt);

            grass = new Content(Blocks.Instance.Grass);

            this.amplitude = amplitude;
        }

        public override Content GetContent(Int32 depth, Int32 offset, Map.StoneType stoneType, Boolean isFilled)
        {
            if (offset > amplitude)
            {
                if (depth == 0) return isFilled ? dirt : grass;

                return dirt;
            }

            if (offset < -amplitude) return Palette!.GetLoose(stoneType);

            return Palette!.GetStone(stoneType);
        }
    }
}
