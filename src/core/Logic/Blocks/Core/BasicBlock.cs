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
    /// This class represents a simple block that is completely filled. <see cref="BasicBlock"/>s themselves do not have much function, but the class can be extended easily.
    /// Data bit usage: <c>------</c>
    /// </summary>
    public class BasicBlock : Block, IOverlayTextureProvider
    {
        private protected int[] sideTextureIndices = null!;

        private readonly TextureLayout layout;

        public virtual int TextureIdentifier => layout.Bottom;

        internal BasicBlock(string name, string namedId, TextureLayout layout, bool isOpaque = true, bool renderFaceAtNonOpaques = true, bool isSolid = true, bool receiveCollisions = false, bool isTrigger = false, bool isInteractable = false) :
            base(
                name,
                namedId,
                isFull: true,
                isOpaque,
                renderFaceAtNonOpaques,
                isSolid,
                receiveCollisions,
                isTrigger,
                isReplaceable: false,
                isInteractable,
                BoundingBox.Block,
                TargetBuffer.Simple)
        {
            this.layout = layout;
        }

        protected override void Setup(ITextureIndexProvider indexProvider)
        {
            sideTextureIndices = layout.GetTexIndexArray();
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            return BlockMeshData.Basic(sideTextureIndices[(int)info.Side], false);
        }
    }
}