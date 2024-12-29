// <copyright file="OrganicDoorBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A <see cref="DoorBlock" /> that is flammable.
///     Data bit usage: <c>-csboo</c>
/// </summary>
public class OrganicDoorBlock : DoorBlock, ICombustible
{
    internal OrganicDoorBlock(String name, String namedID, RID closed, RID open) :
        base(
            name,
            namedID,
            closed,
            open) {}
}
