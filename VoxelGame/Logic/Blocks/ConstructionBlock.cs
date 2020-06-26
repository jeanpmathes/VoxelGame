// <copyright file="ConstructionBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Logic.Interfaces;

namespace VoxelGame.Logic.Blocks
{
    /// <summary>
    ///Blocks that are used in constructing structures.
    /// </summary>
    public class ConstructionBlock : BasicBlock, IConnectable
    {
        public ConstructionBlock(string name, TextureLayout layout) :
            base(
                name,
                layout,
                isOpaque: true,
                renderFaceAtNonOpaques: true,
                isSolid: true)
        {
        }
    }
}