// <copyright file="DoubleCrossPlantBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Entities;
using VoxelGame.Logic.Interfaces;
using VoxelGame.Physics;
using VoxelGame.Visuals;

namespace VoxelGame.Logic.Blocks
{
    /// <summary>
    /// Similar to <see cref="CrossPlantBlock"/>, but is two blocks high.
    /// Data bit usage: <c>----h</c>
    /// </summary>
    // h = height
    public class DoubleCrossPlantBlock : Block, IFlammable
    {
        private protected float[] vertices = null!;
        private protected int[] bottomTexIndices = null!;
        private protected int[] topTexIndices = null!;

        private protected uint[] indices = null!;

        private protected string bottomTexture;
        private protected int topTexOffset;

        public DoubleCrossPlantBlock(string name, string namedId, string bottomTexture, int topTexOffset, BoundingBox boundingBox) :
            base(
                name,
                namedId,
                isFull: false,
                isOpaque: false,
                renderFaceAtNonOpaques: false,
                isSolid: false,
                recieveCollisions: false,
                isTrigger: false,
                isReplaceable: false,
                isInteractable: false,
                boundingBox,
                TargetBuffer.Complex)
        {
            this.bottomTexture = bottomTexture;
            this.topTexOffset = topTexOffset;
        }

        protected override void Setup()
        {
            vertices = new float[]
            {
                // Two sides: /
                0f, 0f, 1f, 0f, 0f, 0f, 0f, 0f,
                0f, 1f, 1f, 0f, 1f, 0f, 0f, 0f,
                1f, 1f, 0f, 1f, 1f, 0f, 0f, 0f,
                1f, 0f, 0f, 1f, 0f, 0f, 0f, 0f,

                // Two sides: \
                0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
                0f, 1f, 0f, 0f, 1f, 0f, 0f, 0f,
                1f, 1f, 1f, 1f, 1f, 0f, 0f, 0f,
                1f, 0f, 1f, 1f, 0f, 0f, 0f, 0f
            };

            int tex = Game.BlockTextureArray.GetTextureIndex(bottomTexture);
            bottomTexIndices = new int[] { tex, tex, tex, tex, tex, tex, tex, tex };

            tex += topTexOffset;
            topTexIndices = new int[] { tex, tex, tex, tex, tex, tex, tex, tex };

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
        }

        public override uint GetMesh(BlockSide side, byte data, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint, out bool isAnimated)
        {
            vertices = this.vertices;
            textureIndices = ((data & 0b1) == 0) ? bottomTexIndices : topTexIndices;
            indices = this.indices;
            tint = TintColor.Neutral;
            isAnimated = false;

            return 8;
        }

        protected override bool Place(PhysicsEntity? entity, int x, int y, int z)
        {
            if (Game.World.GetBlock(x, y + 1, z, out _)?.IsReplaceable != true || !((Game.World.GetBlock(x, y - 1, z, out _) ?? Block.AIR) is IPlantable))
            {
                return false;
            }

            Game.World.SetBlock(this, 0, x, y, z);
            Game.World.SetBlock(this, 1, x, y + 1, z);

            return true;
        }

        protected override bool Destroy(PhysicsEntity? entity, int x, int y, int z, byte data)
        {
            bool isBase = (data & 0b1) == 0;

            Game.World.SetBlock(Block.AIR, 0, x, y, z);
            Game.World.SetBlock(Block.AIR, 0, x, y + (isBase ? 1 : -1), z);

            return true;
        }

        internal override void BlockUpdate(int x, int y, int z, byte data, BlockSide side)
        {
            // Check if this block is the lower part and if the ground supports plant growth.
            if (side == BlockSide.Bottom && (data & 0b1) == 0 && !((Game.World.GetBlock(x, y - 1, z, out _) ?? Block.AIR) is IPlantable))
            {
                Destroy(x, y, z, null);
            }
        }
    }
}