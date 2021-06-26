// <copyright file="ConstructionBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Core.Logic.Interfaces;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// Blocks that are used in constructing structures.
    /// Data bit usage: <c>------</c>
    /// </summary>
    public class ConstructionBlock : BasicBlock, IWideConnectable, IThinConnectable
    {
        internal ConstructionBlock(string name, string namedId, TextureLayout layout) :
            base(
                name,
                namedId,
                layout,
                isOpaque: true,
                renderFaceAtNonOpaques: true,
                isSolid: true,
                receiveCollisions: false,
                isTrigger: false,
                isInteractable: false)
        {
        }
    }
}