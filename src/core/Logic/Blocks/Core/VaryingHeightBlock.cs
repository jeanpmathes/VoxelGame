// <copyright file="VaryingHeightBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    ///     A block that can have different heights.
    ///     Data bit usage: <c>--hhhh</c>
    /// </summary>
    // h = height
    public class VaryingHeightBlock : Block, IHeightVariable
    {
        private readonly TextureLayout layout;
        private int[] textureIndices = null!;

        protected VaryingHeightBlock(string name, string namedId, TextureLayout layout, bool isSolid,
            bool receiveCollisions, bool isTrigger, bool isReplaceable, bool isInteractable) :
            base(
                name,
                namedId,
                isFull: false,
                isOpaque: false,
                renderFaceAtNonOpaques: false,
                isSolid,
                receiveCollisions,
                isTrigger,
                isReplaceable,
                isInteractable,
                BoundingBox.Block,
                TargetBuffer.VaryingHeight)
        {
            this.layout = layout;
        }

        public virtual int GetHeight(uint data)
        {
            return (int) (data & 0b00_1111);
        }

        protected override void Setup(ITextureIndexProvider indexProvider)
        {
            textureIndices = layout.GetTexIndexArray();
        }

        protected override BoundingBox GetBoundingBox(uint data)
        {
            return BoundingBox.BlockWithHeight(GetHeight(data));
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            return BlockMeshData.VaryingHeight(textureIndices[(int) info.Side], TintColor.None);
        }
    }
}