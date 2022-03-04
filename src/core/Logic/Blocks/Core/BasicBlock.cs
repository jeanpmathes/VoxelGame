// <copyright file="BasicBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    ///     This class represents a simple block that is completely filled. <see cref="BasicBlock" />s themselves do not have
    ///     much function, but the class can be extended easily.
    ///     Data bit usage: <c>------</c>
    /// </summary>
    public class BasicBlock : Block, IOverlayTextureProvider
    {
        private readonly TextureLayout layout;
        private protected int[] sideTextureIndices = null!;

        /// <summary>
        ///     Create a new <see cref="BasicBlock" />.
        ///     A <see cref="BasicBlock" /> is a block that is completely filled and cannot be replaced.
        /// </summary>
        /// <param name="name">The name of the block.</param>
        /// <param name="namedId">The named ID.</param>
        /// <param name="flags">The block flags.</param>
        /// <param name="layout">The texture layout.</param>
        internal BasicBlock(string name, string namedId, BlockFlags flags, TextureLayout layout) :
            base(
                name,
                namedId,
                flags with { IsFull = true, IsReplaceable = false },
                BoundingBox.Block,
                TargetBuffer.Simple)
        {
            this.layout = layout;
        }

        /// <inheritdoc />
        public virtual int TextureIdentifier => layout.Bottom;

        /// <inheritdoc />
        protected override void Setup(ITextureIndexProvider indexProvider)
        {
            sideTextureIndices = layout.GetTexIndexArray();
        }

        /// <inheritdoc />
        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            return BlockMeshData.Basic(sideTextureIndices[(int) info.Side], isTextureRotated: false);
        }
    }
}
