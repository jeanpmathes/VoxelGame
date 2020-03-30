// <copyright file="CrossBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using System;
using VoxelGame.Physics;
using VoxelGame.Rendering;

namespace VoxelGame.Logic
{
    public class CrossBlock : Block
    {
        private float[] vertices;

        private uint[] indices =
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

        protected Block RequiredGround { get; }

        /// <summary>
        /// Initializes a new instance of a cross block; a block made out of two intersecting planes.
        /// </summary>
        /// <param name="name">The name of this block and the texture file.</param>
        /// <param name="isReplaceable">Indicates whether this block will be replaceable.</param>
        /// <param name="requiredGround">The block on which this block can be placed.</param>
        /// <param name="boundingBox">The bounding box of this block.</param>
        public CrossBlock(string name, bool isReplaceable, Block requiredGround, BoundingBox boundingBox) :
            base(
                name,
                isFull: false,
                isOpaque: false,
                renderFaceAtNonOpaques: false,
                isSolid: false,
                recieveCollisions: false,
                isTrigger: false,
                isReplaceable,
                boundingBox)
        {
            RequiredGround = requiredGround;

#pragma warning disable CA2214 // Do not call overridable methods in constructors
            Setup();
#pragma warning restore CA2214 // Do not call overridable methods in constructors
        }

        protected virtual void Setup()
        {
            int textureIndex = Game.Atlas.GetTextureIndex(Name);

            if (textureIndex == -1)
            {
                throw new Exception($"No texture '{Name}' found!");
            }

            AtlasPosition uv = Game.Atlas.GetTextureUV(textureIndex);

            vertices = new float[]
            {
                // Two sides: /
                0f, 0f, 1f, uv.bottomLeftU, uv.bottomLeftV,
                0f, 1f, 1f, uv.bottomLeftU, uv.topRightV,
                1f, 1f, 0f, uv.topRightU, uv.topRightV,
                1f, 0f, 0f, uv.topRightU, uv.bottomLeftV,

                // Two sides: \
                0f, 0f, 0f, uv.bottomLeftU, uv.bottomLeftV,
                0f, 1f, 0f, uv.bottomLeftU, uv.topRightV,
                1f, 1f, 1f, uv.topRightU, uv.topRightV,
                1f, 0f, 1f, uv.topRightU, uv.bottomLeftV
            };
        }

        public override uint GetMesh(BlockSide side, ushort data, out float[] vertices, out uint[] indices)
        {
            vertices = this.vertices;
            indices = this.indices;

            return 8;
        }

        public override void OnCollision(Entities.PhysicsEntity entity, int x, int y, int z)
        {
            throw new NotImplementedException();
        }

        public override bool Place(int x, int y, int z, Entities.PhysicsEntity entity)
        {
            // Check the block under the placement position.

            if ((Game.World.GetBlock(x, y - 1, z) ?? Block.AIR) != RequiredGround)
            {
                return false;
            }

            return base.Place(x, y, z, entity);
        }
    }
}