// <copyright file="OrganicDoorBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Logic.Interfaces;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A <see cref="DoorBlock" /> that is flammable.
///     Data bit usage: <c>-csboo</c>
/// </summary>
public class OrganicDoorBlock : DoorBlock, ICombustible
{
    internal OrganicDoorBlock(string name, string namedID, string closed, string open) :
        base(
            name,
            namedID,
            closed,
            open) {}
}
