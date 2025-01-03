﻿// <copyright file="GlassTiled.cs" company="VoxelGame">
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
///     A glass block.
///     Data bit usage: <c>------</c>
/// </summary>
public class GlassBlock : BasicBlock, IThinConnectable
{
    internal GlassBlock(String name, String namedID, TextureLayout layout) :
        base(
            name,
            namedID,
            BlockFlags.Basic with {IsOpaque = false},
            layout) {}
}
