// <copyright file="OrganicDoorBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Core.Logic.Interfaces;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// A <see cref="DoorBlock"/> that is flammable.
    /// Data bit usage: <c>-csboo</c>
    /// </summary>
    public class OrganicDoorBlock : DoorBlock, IFlammable
    {
        public OrganicDoorBlock(string name, string namedId, string closed, string open) :
            base(
                name,
                namedId,
                closed,
                open)
        {
        }
    }
}