// <copyright file="Layer.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using VoxelGame.Core.Generation.Worlds.Default.Palettes;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Logic.Voxels.Behaviors;
using VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;
using VoxelGame.Core.Utilities.Units;

namespace VoxelGame.Core.Generation.Worlds.Default;

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
    ///     There should be one dampening layer per sub-biome.
    ///     It is used to dampen the random offset applied according to noise, so that lower layers have uniform height.
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
    ///     Create a dampening layer that absorbs some of the offset. This is a meta layer and uses the passed block.
    /// </summary>
    public static Layer CreateDampen(Block block, Int32 maxWidth)
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
    public static Layer CreateStonyTop(Int32 width, Int32 amplitude = Int32.MaxValue)
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
    ///     Create a variant of the top layer, that also uses the alternative when very close to the ocean height.
    ///     This creates a beach-like effect when used at coastlines.
    /// </summary>
    public static Layer CreateCoastlineTop(Block top, Block lowOrFilled, Int32 width)
    {
        return new CoastlineTop(top, lowOrFilled, width);
    }

    /// <summary>
    ///     Create a simple layer. It can be declared as solid, which is only valid when not fillable.
    /// </summary>
    public static Layer CreateSimple(Block block, Int32 width, Boolean isSolid)
    {
        if (isSolid) Debug.Assert(!block.Is<Fillable>());

        return new Simple(block, width, isSolid);
    }

    /// <summary>
    ///     Create a layer with groundwater that uses a loose block depending on stone type.
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
    ///     Create a snow layer. Snow will not be generated when filled.
    ///     This layer can place either normal or loose snow blocks.
    /// </summary>
    public static Layer CreateSnow(Int32 width, Boolean loose)
    {
        return new Snow(width, loose);
    }

    /// <summary>
    ///     Create a solid stone layer.
    /// </summary>
    public static Layer CreateStone(Int32 width)
    {
        return new Stone(width);
    }

    /// <summary>
    ///     Create a mud layer, which will use permafrost when the temperature is below freezing.
    /// </summary>
    public static Layer CreateMud(Int32 width)
    {
        return new Mud(width);
    }

    /// <summary>
    ///     Special top layer for the oasis sub-biome.
    /// </summary>
    public static Layer CreateOasisTop(Int32 width, Int32 subBiomeOffset)
    {
        return new OasisTop(width, subBiomeOffset);
    }

    /// <summary>
    ///     A layer made out of ice.
    /// </summary>
    public static Layer CreateIce(Int32 width)
    {
        return new Ice(width, isDampen: false);
    }

    /// <summary>
    ///     Create a dampening layer made out of ice.
    /// </summary>
    public static Layer CreateIceDampen(Int32 maxWidth)
    {
        return new Ice(maxWidth, isDampen: true);
    }

    /// <summary>
    ///     Returns the data for the layer content.
    /// </summary>
    /// <param name="depth">The depth within the layer.</param>
    /// <param name="offset">The random offset from normal ground height.</param>
    /// <param name="y">The y coordinate of the current position.</param>
    /// <param name="stoneType">The stone type of the column.</param>
    /// <param name="isFilled">Whether the column is filled with fluid or not.</param>
    /// <param name="temperature">The temperature at the current position.</param>
    /// <returns>The data for the layer content.</returns>
    public abstract Content GetContent(Int32 depth, Int32 offset, Int32 y, Map.StoneType stoneType, Boolean isFilled, Temperature temperature);

    private sealed class Top : Layer
    {
        private readonly Content filledData;
        private readonly Content normalData;

        public Top(Block top, Block filled, Int32 width)
        {
            Width = width;

            normalData = Content.CreateGenerated(top);
            filledData = Content.CreateGenerated(filled);
        }

        public override Content GetContent(Int32 depth, Int32 offset, Int32 y, Map.StoneType stoneType, Boolean isFilled, Temperature temperature)
        {
            return isFilled ? filledData : normalData;
        }
    }

    private sealed class CoastlineTop : Layer
    {
        private readonly Content lowOrFilledData;
        private readonly Content normalData;

        public CoastlineTop(Block top, Block lowOrFilled, Int32 width)
        {
            Width = width;

            normalData = Content.CreateGenerated(top);
            lowOrFilledData = Content.CreateGenerated(lowOrFilled);
        }

        public override Content GetContent(Int32 depth, Int32 offset, Int32 y, Map.StoneType stoneType, Boolean isFilled, Temperature temperature)
        {
            return isFilled || y < 5 ? lowOrFilledData : normalData;
        }
    }

    private sealed class Simple : Layer
    {
        private readonly Content data;

        public Simple(Block block, Int32 width, Boolean isSolid)
        {
            Width = width;
            IsSolid = isSolid;

            data = Content.CreateGenerated(block);
        }

        public override Content GetContent(Int32 depth, Int32 offset, Int32 y, Map.StoneType stoneType, Boolean isFilled, Temperature temperature)
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

        public override Content GetContent(Int32 depth, Int32 offset, Int32 y, Map.StoneType stoneType, Boolean isFilled, Temperature temperature)
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

        public Snow(Int32 width, Boolean loose)
        {
            Width = width;

            Block block = loose
                ? Blocks.Instance.Environment.PulverizedSnow
                : Blocks.Instance.Environment.Snow;
            
            snow = new Content(block.States.GenerationDefault.WithHeight(BlockHeight.Maximum), FluidInstance.Default);
            filled = Content.Default;
        }

        public override Content GetContent(Int32 depth, Int32 offset, Int32 y, Map.StoneType stoneType, Boolean isFilled, Temperature temperature)
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

        public override Content GetContent(Int32 depth, Int32 offset, Int32 y, Map.StoneType stoneType, Boolean isFilled, Temperature temperature)
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

            data = Content.CreateGenerated(block);
        }

        public override Content GetContent(Int32 depth, Int32 offset, Int32 y, Map.StoneType stoneType, Boolean isFilled, Temperature temperature)
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

        public override Content GetContent(Int32 depth, Int32 offset, Int32 y, Map.StoneType stoneType, Boolean isFilled, Temperature temperature)
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

        public override Content GetContent(Int32 depth, Int32 offset, Int32 y, Map.StoneType stoneType, Boolean isFilled, Temperature temperature)
        {
            return Palette!.GetStone(stoneType);
        }
    }

    private sealed class StonyTop : Layer
    {
        private readonly Int32 amplitude;
        private readonly Content grass;

        private readonly Content soil;

        public StonyTop(Int32 width, Int32 amplitude)
        {
            Width = width;

            soil = Content.CreateGenerated(Blocks.Instance.Environment.Soil);
            grass = Content.CreateGenerated(Blocks.Instance.Environment.Grass);

            this.amplitude = amplitude;
        }

        public override Content GetContent(Int32 depth, Int32 offset, Int32 y, Map.StoneType stoneType, Boolean isFilled, Temperature temperature)
        {
            if (offset > amplitude)
            {
                if (depth == 0) return isFilled ? soil : grass;

                return soil;
            }

            if (offset < -amplitude) return Palette!.GetLoose(stoneType);

            return Palette!.GetStone(stoneType);
        }
    }

    private sealed class Mud : Layer
    {
        private readonly Content mud;
        private readonly Content permafrost;

        public Mud(Int32 width)
        {
            Width = width;

            mud = Content.CreateGenerated(Blocks.Instance.Environment.Mud);
            permafrost = Content.CreateGenerated(Blocks.Instance.Environment.Permafrost);
        }

        public override Content GetContent(Int32 depth, Int32 offset, Int32 y, Map.StoneType stoneType, Boolean isFilled, Temperature temperature)
        {
            return temperature.IsFreezing ? permafrost : mud;
        }
    }

    private sealed class OasisTop : Layer
    {
        private readonly Content sand = Content.CreateGenerated(Blocks.Instance.Environment.Sand);
        private readonly Content sandstone = Content.CreateGenerated(Blocks.Instance.Stones.Sandstone.Base);

        private readonly Int32 subBiomeOffset;

        public OasisTop(Int32 width, Int32 subBiomeOffset)
        {
            Width = width;

            this.subBiomeOffset = subBiomeOffset;
        }

        public override Content GetContent(Int32 depth, Int32 offset, Int32 y, Map.StoneType stoneType, Boolean isFilled, Temperature temperature)
        {
            return offset - subBiomeOffset > 0 ? sandstone : sand;
        }
    }

    private sealed class Ice : Layer
    {
        private readonly Content ice;

        public Ice(Int32 width, Boolean isDampen)
        {
            Width = width;
            IsDampen = isDampen;
            IsSolid = true;

            ice = new Content(Blocks.Instance.Environment.Ice.States.GenerationDefault.WithHeight(BlockHeight.Maximum), FluidInstance.Default);
        }

        public override Content GetContent(Int32 depth, Int32 offset, Int32 y, Map.StoneType stoneType, Boolean isFilled, Temperature temperature)
        {
            return ice;
        }
    }
}
