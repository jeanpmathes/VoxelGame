// <copyright file="ConstructionBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     Blocks that are used in constructing structures.
///     Data bit usage: <c>------</c>
/// </summary>
public class ConstructionBlock : BasicBlock, IWideConnectable, IThinConnectable
{
    internal ConstructionBlock(String name, String namedID, TextureLayout layout) :
        base(
            name,
            namedID,
            BlockFlags.Basic,
            layout) {}
}
