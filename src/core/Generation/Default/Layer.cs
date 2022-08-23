// <copyright file="Layer.cs" company="VoxelGame">
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
    ///     Set the currently used palette.
    /// </summary>
    public virtual void SetPalette(Palette newPalette) {}

    /// <summary>
    ///     Create a dampening layer that absorbs some of the offset. This is a meta layer and is assumed to be fillable.
    /// </summary>
    public static Layer CreatePermeableDampen(IBlockBase block, int maxWidth)
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
    public static Layer CreateCover(IBlockBase cover, IBlockBase filled, int width)
    {
        return new Cover(cover, filled, width);
    }

    /// <summary>
    ///     Create a layer with a permeable material that will be filled with water.
    /// </summary>
    public static Layer CreatePermeable(IBlockBase block, int width)
    {
        return new Permeable(block, width);
    }

    /// <summary>
    ///     Create a solid layer, which always has the same block.
    /// </summary>
    public static Layer CreateSolid(IBlockBase block, int width)
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
    public abstract uint GetData(int depth, int offset, Map.StoneType stoneType, bool isFilled);

    private sealed class Cover : Layer
    {
        private readonly uint filledData;
        private readonly uint normalData;

        public Cover(IBlockBase cover, IBlockBase filled, int width)
        {
            Width = width;

            normalData = Section.Encode(cover);
            filledData = filled is IFillable ? Section.Encode(filled, Fluid.Water) : Section.Encode(filled);
        }

        public override uint GetData(int depth, int offset, Map.StoneType stoneType, bool isFilled)
        {
            return isFilled ? filledData : normalData;
        }
    }

    private sealed class Permeable : Layer
    {
        private readonly uint filled;
        private readonly uint normal;

        public Permeable(IBlockBase block, int width)
        {
            Width = width;

            normal = Section.Encode(block);
            filled = Section.Encode(block, Fluid.Water);
        }

        public override uint GetData(int depth, int offset, Map.StoneType stoneType, bool isFilled)
        {
            return isFilled ? filled : normal;
        }
    }

    private sealed class Solid : Layer
    {
        private readonly uint data;

        public Solid(IBlockBase block, int width)
        {
            Width = width;
            IsSolid = true;

            data = Section.Encode(block);
        }

        public override uint GetData(int depth, int offset, Map.StoneType stoneType, bool isFilled)
        {
            return data;
        }
    }

    private sealed class Groundwater : Layer
    {
        private readonly int groundWaterDepth;

        private Palette? palette;

        public Groundwater(int width)
        {
            Width = width;

            groundWaterDepth = width / 2;
        }

        public override void SetPalette(Palette newPalette)
        {
            palette = newPalette;
        }

        public override uint GetData(int depth, int offset, Map.StoneType stoneType, bool isFilled)
        {
            if (isFilled) return palette!.GetLoose(stoneType, isFilled);

            int actualDepth = depth - offset;

            return actualDepth >= groundWaterDepth ? palette!.GetGroundwater(stoneType) : palette!.GetLoose(stoneType, isFilled);
        }
    }

    private sealed class Snow : Layer
    {
        private readonly uint filled;
        private readonly uint snow;

        public Snow(int width)
        {
            Width = width;

            snow = Block.Specials.Snow.FullHeightData;
            filled = Section.Encode(fluid: Fluid.Water);
        }

        public override uint GetData(int depth, int offset, Map.StoneType stoneType, bool isFilled)
        {
            return isFilled ? filled : snow;
        }
    }

    private sealed class Loose : Layer
    {
        private Palette? palette;

        public Loose(int width)
        {
            Width = width;
        }

        public override void SetPalette(Palette newPalette)
        {
            palette = newPalette;
        }

        public override uint GetData(int depth, int offset, Map.StoneType stoneType, bool isFilled)
        {
            return palette!.GetLoose(stoneType, isFilled);
        }
    }

    private sealed class PermeableDampen : Layer
    {
        private readonly uint blockFilled;
        private readonly uint blockNormal;

        public PermeableDampen(IBlockBase block, int maxWidth)
        {
            Width = maxWidth;
            IsDampen = true;

            blockNormal = Section.Encode(block);
            blockFilled = Section.Encode(block, Fluid.Water);
        }

        public override uint GetData(int depth, int offset, Map.StoneType stoneType, bool isFilled)
        {
            return isFilled ? blockFilled : blockNormal;
        }
    }

    private sealed class StonyDampen : Layer
    {
        private Palette? palette;

        public StonyDampen(int maxWidth)
        {
            Width = maxWidth;
            IsDampen = true;
        }

        public override void SetPalette(Palette newPalette)
        {
            palette = newPalette;
        }

        public override uint GetData(int depth, int offset, Map.StoneType stoneType, bool isFilled)
        {
            return palette!.GetStone(stoneType);
        }
    }

    private sealed class Stone : Layer
    {
        private Palette? palette;

        public Stone(int width)
        {
            Width = width;
            IsSolid = true;
        }

        public override void SetPalette(Palette newPalette)
        {
            palette = newPalette;
        }

        public override uint GetData(int depth, int offset, Map.StoneType stoneType, bool isFilled)
        {
            return palette!.GetStone(stoneType);
        }
    }

    private sealed class StonyCover : Layer
    {
        private readonly int amplitude;
        private readonly uint dirt;
        private readonly uint dirtFilled;

        private readonly uint grass;
        private readonly uint grassFilled;
        private Palette? palette;

        public StonyCover(int width, int amplitude)
        {
            Width = width;

            dirt = Section.Encode(Block.Dirt);
            dirtFilled = Section.Encode(Block.Dirt, Fluid.Water);

            grass = Section.Encode(Block.Grass);
            grassFilled = Section.Encode(Block.Grass, Fluid.Water);

            this.amplitude = amplitude;
        }

        public override void SetPalette(Palette newPalette)
        {
            palette = newPalette;
        }

        public override uint GetData(int depth, int offset, Map.StoneType stoneType, bool isFilled)
        {
            if (offset > amplitude)
            {
                if (depth == 0) return isFilled ? grassFilled : grass;

                return isFilled ? dirtFilled : dirt;
            }

            if (offset < -amplitude) return palette!.GetLoose(stoneType, isFilled);

            return palette!.GetStone(stoneType);
        }
    }
}
