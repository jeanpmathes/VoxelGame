// <copyright file="Palette.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Generation.Default;

/// <summary>
///     A palette of blocks and fluids to use for world generation.
/// </summary>
public class Palette
{
    private readonly Content granite = new(Blocks.Instance.Granite);

    private readonly Content gravel = new(Blocks.Instance.Gravel);
    private readonly Content gravelGroundwater = new(Blocks.Instance.Gravel, Fluids.Instance.FreshWater);
    private readonly Content limestone = new(Blocks.Instance.Limestone);
    private readonly Content marble = new(Blocks.Instance.Marble);

    private readonly Content sand = new(Blocks.Instance.Sand);
    private readonly Content sandGroundwater = new(Blocks.Instance.Sand, Fluids.Instance.FreshWater);
    private readonly Content sandstone = new(Blocks.Instance.Sandstone);

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

    internal Content GetLoose(Map.StoneType type)
    {
        return type == Map.StoneType.Sandstone ? sand : gravel;
    }

    internal Content GetGroundwater(Map.StoneType type)
    {
        return type == Map.StoneType.Sandstone ? sandGroundwater : gravelGroundwater;
    }
}
