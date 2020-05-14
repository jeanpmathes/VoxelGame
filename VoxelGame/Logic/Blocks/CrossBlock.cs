// <copyright file="CrossBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Physics;
using VoxelGame.Rendering;

namespace VoxelGame.Logic.Blocks
{
    public class CrossBlock : Block
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
        protected float[] vertices;
        protected int[] textureIndices;

        protected readonly uint[] indices =
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
#pragma warning disable CA1051 // Do not declare visible instance fields

        /// <summary>
        /// Initializes a new instance of a cross block; a block made out of two intersecting planes.
        /// </summary>
        /// <param name="name">The name of this block and the texture file.</param>
        /// <param name="isReplaceable">Indicates whether this block will be replaceable.</param>
        /// <param name="boundingBox">The bounding box of this block.</param>
        public CrossBlock(string name, string texture, bool isReplaceable, bool recieveCollisions, bool isTrigger, BoundingBox boundingBox) :
            base(
                name,
                isFull: false,
                isOpaque: false,
                renderFaceAtNonOpaques: false,
                isSolid: false,
                recieveCollisions,
                isTrigger,
                isReplaceable,
                boundingBox)
        {
#pragma warning disable CA2214 // Do not call overridable methods in constructors
            Setup(texture);
#pragma warning restore CA2214 // Do not call overridable methods in constructors
        }

        protected virtual void Setup(string texture)
        {
            vertices = new float[]
            {
                // Two sides: /
                0f, 0f, 1f, 0f, 0f,
                0f, 1f, 1f, 0f, 1f,
                1f, 1f, 0f, 1f, 1f,
                1f, 0f, 0f, 1f, 0f,

                // Two sides: \
                0f, 0f, 0f, 0f, 0f,
                0f, 1f, 0f, 0f, 1f,
                1f, 1f, 1f, 1f, 1f,
                1f, 0f, 1f, 1f, 0f
            };

            int tex = Game.BlockTextureArray.GetTextureIndex(texture);
            textureIndices = new int[] { tex, tex, tex, tex, tex, tex, tex, tex };
        }

        public override uint GetMesh(BlockSide side, byte data, out float[] vertices, out int[] textureIndices, out uint[] indices)
        {
            vertices = this.vertices;
            textureIndices = this.textureIndices;
            indices = this.indices;

            return 8;
        }
    }
}