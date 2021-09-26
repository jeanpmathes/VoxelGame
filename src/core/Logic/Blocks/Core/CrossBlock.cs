// <copyright file="CrossBlock.cs" company="VoxelGame">
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
    ///     A block with two crossed quads.
    ///     Data bit usage: <c>------</c>
    /// </summary>
    public class CrossBlock : Block, IFillable
    {
        private readonly string texture;

        private uint[] indices = null!;
        private int[] textureIndices = null!;
        private float[] vertices = null!;

        /// <summary>
        ///     Initializes a new instance of a cross block; a block made out of two intersecting planes.
        /// </summary>
        /// <param name="name">The name of this block.</param>
        /// <param name="namedId">The unique and unlocalized name of this block.</param>
        /// <param name="texture">The name of the texture for this block.</param>
        /// <param name="receiveCollisions">Whether this block should receive collisions.</param>
        /// <param name="isTrigger">Whether this block is a trigger.</param>
        /// <param name="isReplaceable">Indicates whether this block will be replaceable.</param>
        /// <param name="boundingBox">The bounding box of this block.</param>
        protected CrossBlock(string name, string namedId, string texture, bool receiveCollisions, bool isTrigger,
            bool isReplaceable, BoundingBox boundingBox) :
            base(
                name,
                namedId,
                isFull: false,
                isOpaque: false,
                renderFaceAtNonOpaques: false,
                isSolid: false,
                receiveCollisions,
                isTrigger,
                isReplaceable,
                isInteractable: false,
                boundingBox,
                TargetBuffer.Complex)
        {
            this.texture = texture;
        }

        protected override void Setup(ITextureIndexProvider indexProvider)
        {
            (vertices, indices, textureIndices) = BlockModels.CreateCrossModel(indexProvider.GetTextureIndex(texture));
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            return BlockMeshData.Complex(vertexCount: 8, vertices, textureIndices, indices);
        }
    }
}
