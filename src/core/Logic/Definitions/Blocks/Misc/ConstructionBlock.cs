// <copyright file="ConstructionBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     Blocks that are used in constructing structures.
///     Data bit usage: <c>------</c>
/// </summary>
public class ConstructionBlock : BasicBlock, IWideConnectable, IThinConnectable
{
    internal ConstructionBlock(string name, string namedId, TextureLayout layout) :
        base(
            name,
            namedId,
            BlockFlags.Basic,
            layout) {}
}

