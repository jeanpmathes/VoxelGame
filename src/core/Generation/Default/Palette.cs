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
    private readonly uint limestone = Section.Encode(Block.Limestone);
    private readonly uint marble = Section.Encode(Block.Marble);

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
}
