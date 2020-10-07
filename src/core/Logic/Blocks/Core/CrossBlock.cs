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
    /// A block with two crossed quads.
    /// Data bit usage: <c>------</c>
    /// </summary>
    public class CrossBlock : Block, IFillable
    {
        private protected float[] vertices = null!;
        private protected int[] textureIndices = null!;
        private protected uint[] indices = null!;

        private protected string texture;

        /// <summary>
        /// Initializes a new instance of a cross block; a block made out of two intersecting planes.
        /// </summary>
        /// <param name="name">The name of this block and the texture file.</param>
        /// <param name="isReplaceable">Indicates whether this block will be replaceable.</param>
        /// <param name="boundingBox">The bounding box of this block.</param>
        public CrossBlock(string name, string namedId, string texture, bool recieveCollisions, bool isTrigger, bool isReplaceable, BoundingBox boundingBox) :
            base(
                name,
                namedId,
                isFull: false,
                isOpaque: false,
                renderFaceAtNonOpaques: false,
                isSolid: false,
                recieveCollisions,
                isTrigger,
                isReplaceable,
                isInteractable: false,
                boundingBox,
                TargetBuffer.Complex)
        {
            this.texture = texture;
        }

        protected override void Setup()
        {
            vertices = new float[]
            {
                // Two sides: /
                0.145f, 0f, 0.855f, 0f, 0f, 0f, 0f, 0f,
                0.145f, 1f, 0.855f, 0f, 1f, 0f, 0f, 0f,
                0.855f, 1f, 0.145f, 1f, 1f, 0f, 0f, 0f,
                0.855f, 0f, 0.145f, 1f, 0f, 0f, 0f, 0f,

                // Two sides: \
                0.145f, 0f, 0.145f, 0f, 0f, 0f, 0f, 0f,
                0.145f, 1f, 0.145f, 0f, 1f, 0f, 0f, 0f,
                0.855f, 1f, 0.855f, 1f, 1f, 0f, 0f, 0f,
                0.855f, 0f, 0.855f, 1f, 0f, 0f, 0f, 0f
            };

            indices = new uint[]
            {
                // Direction: /
                0, 2, 1,
                0, 3, 2,

                0, 1, 2,
                0, 2, 3,

                // Direction: \
                4, 6, 5,
                4, 7, 6,

                4, 5, 6,
                4, 6, 7
            };

            int tex = Game.BlockTextures.GetTextureIndex(texture);
            textureIndices = new int[] { tex, tex, tex, tex, tex, tex, tex, tex };
        }

        public override uint GetMesh(BlockSide side, uint data, Liquid liquid, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint, out bool isAnimated)
        {
            vertices = this.vertices;
            textureIndices = this.textureIndices;
            indices = this.indices;

            tint = TintColor.None;
            isAnimated = false;

            return 8;
        }
    }
}