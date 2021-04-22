// <copyright file="NaturalBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Logic.Interfaces;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// A natural block that can burn.
    /// Data bit usage: <c>------</c>
    /// </summary>
    public class NaturalBlock : BasicBlock, IFlammable
    {
        internal NaturalBlock(string name, string namedId, TextureLayout layout, bool isOpaque = true, bool renderFaceAtNonOpaques = true, bool isSolid = true) :
            base(
                name,
                namedId,
                layout,
                isOpaque,
                renderFaceAtNonOpaques,
                isSolid,
                receiveCollisions: false,
                isTrigger: false,
                isInteractable: false)
        {
        }
    }
}