// <copyright file="OrganicConstructionBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Logic.Interfaces;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// A <see cref="ConstructionBlock"/> made out of organic, flammable materials.
    /// Data bit usage: <c>------</c>
    /// </summary>
    public class OrganicConstructionBlock : ConstructionBlock, IFlammable
    {
        internal OrganicConstructionBlock(string name, string namedId, TextureLayout layout) :
            base(
                name,
                namedId,
                layout) {}
    }
}