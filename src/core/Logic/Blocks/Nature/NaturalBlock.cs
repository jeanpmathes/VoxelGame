// <copyright file="NaturalBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    ///     A natural block that can burn.
    ///     Data bit usage: <c>------</c>
    /// </summary>
    public class NaturalBlock : BasicBlock, IFlammable
    {
        internal NaturalBlock(string name, string namedId, BlockFlags flags, TextureLayout layout) :
            base(
                name,
                namedId,
                flags,
                layout) {}
    }
}
