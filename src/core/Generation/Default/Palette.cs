﻿// <copyright file="Palette.cs" company="VoxelGame">
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
    private readonly Content granite = new(Block.Granite);

    private readonly Content gravel = new(Block.Gravel);
    private readonly Content gravelFilled = new(Block.Gravel, Fluid.Water);
    private readonly Content gravelGroundwater = new(Block.Gravel, Fluid.Water);
    private readonly Content limestone = new(Block.Limestone);
    private readonly Content marble = new(Block.Marble);

    private readonly Content sand = new(Block.Sand);
    private readonly Content sandFilled = new(Block.Sand, Fluid.Water);
    private readonly Content sandGroundwater = new(Block.Sand, Fluid.Water);
    private readonly Content sandstone = new(Block.Sandstone);

    internal Content Empty { get; init; } = Content.Default;

    internal Content Water { get; init; } = new(fluid: Fluid.Water);

    internal Content Core { get; init; } = new(Block.Core);

    internal Content Ice { get; init; } = new(Block.Specials.Ice.FullHeightInstance, FluidInstance.Default);

    internal Content GetStone(Map.StoneType type)
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

    internal Content GetLoose(Map.StoneType type, bool isFilled)
    {
        Content currentSand = isFilled ? sandFilled : sand;
        Content currentGravel = isFilled ? gravelFilled : gravel;

        return type == Map.StoneType.Sandstone ? currentSand : currentGravel;
    }

    internal Content GetGroundwater(Map.StoneType type)
    {
        return type == Map.StoneType.Sandstone ? sandGroundwater : gravelGroundwater;
    }
}
