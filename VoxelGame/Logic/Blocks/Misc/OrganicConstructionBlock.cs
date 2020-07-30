// <copyright file="OrganicConstructionBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Logic.Interfaces;

namespace VoxelGame.Logic.Blocks.Misc
{
    /// <summary>
    /// A <see cref="ConstructionBlock"/> made out of organic, flammable materials.
    /// Data bit usage: <c>------</c>
    /// </summary>
    public class OrganicConstructionBlock : ConstructionBlock, IFlammable
    {
        public OrganicConstructionBlock(string name, string namedId, TextureLayout layout) :
            base(
                name,
                namedId,
                layout)
        {
        }
    }
}