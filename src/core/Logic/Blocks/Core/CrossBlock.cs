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
        /// Cross blocks are never full, solid, or opaque.
        /// </summary>
        protected CrossBlock(string name, string namedId, string texture, BlockFlags flags, BoundingBox boundingBox) :
            base(
                name,
                namedId,
                flags with { IsFull = false, IsOpaque = false, IsSolid = false },
                boundingBox,
                TargetBuffer.Complex)
        {
            this.texture = texture;
        }

        /// <inheritdoc />
        protected override void Setup(ITextureIndexProvider indexProvider)
        {
            (vertices, indices, textureIndices) = BlockModels.CreateCrossModel(indexProvider.GetTextureIndex(texture));
        }

        /// <inheritdoc />
        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            return BlockMeshData.Complex(vertexCount: 8, vertices, textureIndices, indices);
        }
    }
}
