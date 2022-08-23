// <copyright file="Palette.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Generation.Default;

/// <summary>
///     A palette of blocks and fluids to use for world generation.
/// </summary>
public class Palette
{
    private readonly uint granite = Section.Encode(Block.Granite);

    private readonly uint gravel = Section.Encode(Block.Gravel);
    private readonly uint gravelFilled = Section.Encode(Block.Gravel, Fluid.Water);
    private readonly uint gravelGroundwater = Section.Encode(Block.Gravel, Fluid.Water);
    private readonly uint limestone = Section.Encode(Block.Limestone);
    private readonly uint marble = Section.Encode(Block.Marble);

    private readonly uint sand = Section.Encode(Block.Sand);
    private readonly uint sandFilled = Section.Encode(Block.Sand, Fluid.Water);
    private readonly uint sandGroundwater = Section.Encode(Block.Sand, Fluid.Water);
    private readonly uint sandstone = Section.Encode(Block.Sandstone);

    internal uint Empty { get; init; } = Section.Encode();

    internal uint Water { get; init; } = Section.Encode(fluid: Fluid.Water);

    internal uint Core { get; init; } = Section.Encode(Block.Core);

    internal uint GetStone(Map.StoneType type)
    {
        return type switch
        {
            Map.StoneType.Sandstone => sandstone,
            Map.StoneType.Granite => granite,
            Map.StoneType.Limestone => limestone,
            Map.StoneType.Marble => marble,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, message: null)
        };
    }

    internal uint GetLoose(Map.StoneType type, bool isFilled)
    {
        uint currentSand = isFilled ? sandFilled : sand;
        uint currentGravel = isFilled ? gravelFilled : gravel;

        return type == Map.StoneType.Sandstone ? currentSand : currentGravel;
    }

    internal uint GetGroundwater(Map.StoneType type)
    {
        return type == Map.StoneType.Sandstone ? sandGroundwater : gravelGroundwater;
    }
}
