// <copyright file="NaturalBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Logic.Interfaces;

namespace VoxelGame.Logic.Blocks
{
    /// <summary>
    /// A natural block that can burn.
    /// </summary>
    public class NaturalBlock : BasicBlock, IFlammable
    {
        public NaturalBlock(string name, string namedId, TextureLayout layout, bool isOpaque = true, bool renderFaceAtNonOpaques = true, bool isSolid = true) :
            base(
                name,
                namedId,
                layout,
                isOpaque,
                renderFaceAtNonOpaques,
                isSolid,
                isInteractable: false)
        {
        }
    }
}