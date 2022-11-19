// <copyright file="PermeableNaturalBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks;

/// <summary>
///     A permeable <see cref="NaturalBlock" />.
///     Data bit usage: <c>------</c>
/// </summary>
public class PermeableNaturalBlock : NaturalBlock, IFillable
{
    /// <summary>
    ///     Creates a new permeable natural block.
    /// </summary>
    public PermeableNaturalBlock(string name, string namedId, bool hasNeutralTint, BlockFlags flags, TextureLayout layout) : base(name, namedId, hasNeutralTint, flags, layout) {}
}
