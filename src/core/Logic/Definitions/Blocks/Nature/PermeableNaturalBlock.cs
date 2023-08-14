// <copyright file="PermeableNaturalBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A permeable <see cref="NaturalBlock" />.
///     Data bit usage: <c>------</c>
/// </summary>
public class PermeableNaturalBlock : NaturalBlock, IFillable
{
    /// <summary>
    ///     Creates a new permeable natural block.
    /// </summary>
    public PermeableNaturalBlock(string name, string namedID, bool hasNeutralTint, BlockFlags flags, TextureLayout layout) : base(name, namedID, hasNeutralTint, flags, layout) {}
}
