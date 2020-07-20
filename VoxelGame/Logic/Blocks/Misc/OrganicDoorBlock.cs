// <copyright file="OrganicDoorBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Logic.Interfaces;

namespace VoxelGame.Logic.Blocks
{
    /// <summary>
    /// A <see cref="DoorBlock"/> that is flammable.
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