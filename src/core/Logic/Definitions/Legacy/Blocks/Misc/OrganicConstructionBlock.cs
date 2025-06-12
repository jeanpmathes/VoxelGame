// <copyright file="OrganicConstructionBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Definitions.Legacy.Blocks;

/// <summary>
///     A <see cref="ConstructionBlock" /> made out of organic, flammable materials.
///     Data bit usage: <c>------</c>
/// </summary>
public class OrganicConstructionBlock : ConstructionBlock, ICombustible
{
    internal OrganicConstructionBlock(String name, String namedID, TextureLayout layout) :
        base(
            name,
            namedID,
            layout) {}
}
