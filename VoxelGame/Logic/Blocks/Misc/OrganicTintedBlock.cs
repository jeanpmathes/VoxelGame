// <copyright file="OrganicTintedBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Logic.Interfaces;

namespace VoxelGame.Logic.Blocks.Misc
{
    /// <summary>
    /// A <see cref="TintedBlock"/> made out of organic, flammable materials.
    /// Data bit usage: <c>-cccc</c>
    /// </summary>
    public class OrganicTintedBlock : TintedBlock, IFlammable
    {
        public OrganicTintedBlock(string name, string namedId, TextureLayout layout) :
            base(
                name,
                namedId,
                layout)
        {
        }
    }
}