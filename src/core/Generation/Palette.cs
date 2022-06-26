// <copyright file="Palette.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Generation;

/// <summary>
///     A palette of blocks and fluids to use for world generation.
/// </summary>
public class Palette
{
    internal uint Empty { get; init; } = Section.Encode();

    internal uint Water { get; init; } = Section.Encode(fluid: Fluid.Water);

    internal uint Core { get; init; } = Section.Encode(Block.Core);
}
