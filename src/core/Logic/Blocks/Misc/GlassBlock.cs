// <copyright file="GlassTiled.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Logic.Interfaces;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// A glass block.
    /// Data bit usage: <c>------</c>
    /// </summary>
    public class GlassBlock : BasicBlock, IThinConnectable
    {
        internal GlassBlock(string name, string namedId, TextureLayout layout) :
            base(
                name,
                namedId,
                layout,
                isOpaque: false,
                renderFaceAtNonOpaques: false)
        {
        }
    }
}