﻿// <copyright file="OrganicTintedBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A <see cref="TintedBlock" /> made out of organic, flammable materials.
///     Data bit usage: <c>--cccc</c>
/// </summary>
public class OrganicTintedBlock : TintedBlock, ICombustible
{
    internal OrganicTintedBlock(string name, string namedID, TextureLayout layout) :
        base(
            name,
            namedID,
            BlockFlags.Basic,
            layout) {}
}
