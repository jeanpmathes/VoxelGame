// <copyright file="DoubleCrossPlantBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// Similar to <see cref="CrossPlantBlock"/>, but is two blocks high.
    /// Data bit usage: <c>-----h</c>
    /// </summary>
    // h = height
    public class DoubleCrossPlantBlock : Block, IFlammable, IFillable
    {
        private float[] vertices = null!;
        private int[] bottomTexIndices = null!;
        private int[] topTexIndices = null!;

        private uint[] indices = null!;

        private readonly string bottomTexture;
        private readonly int topTexOffset;

        internal DoubleCrossPlantBlock(string name, string namedId, string bottomTexture, int topTexOffset, BoundingBox boundingBox) :
            base(
                name,
                namedId,
                isFull: false,
                isOpaque: false,
                renderFaceAtNonOpaques: false,
                isSolid: false,
                receiveCollisions: false,
                isTrigger: false,
                isReplaceable: false,
                isInteractable: false,
                boundingBox,
                TargetBuffer.Complex)
        {
            this.bottomTexture = bottomTexture;
            this.topTexOffset = topTexOffset;
        }

        protected override void Setup(ITextureIndexProvider indexProvider)
        {
            vertices = new[]
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

            int tex = indexProvider.GetTextureIndex(bottomTexture);
            bottomTexIndices = new[] { tex, tex, tex, tex, tex, tex, tex, tex };

            tex += topTexOffset;
            topTexIndices = new[] { tex, tex, tex, tex, tex, tex, tex, tex };

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

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            return new BlockMeshData(8, vertices, ((info.Data & 0b1) == 0) ? bottomTexIndices : topTexIndices, indices, TintColor.Neutral);
        }

        internal override bool CanPlace(int x, int y, int z, PhysicsEntity? entity)
        {
            return Game.World.GetBlock(x, y + 1, z, out _)?.IsReplaceable == true && (Game.World.GetBlock(x, y - 1, z, out _) ?? Block.Air) is IPlantable;
        }

        protected override void DoPlace(int x, int y, int z, PhysicsEntity? entity)
        {
            Game.World.SetBlock(this, 0, x, y, z);
            Game.World.SetBlock(this, 1, x, y + 1, z);
        }

        internal override void DoDestroy(int x, int y, int z, uint data, PhysicsEntity? entity)
        {
            bool isBase = (data & 0b1) == 0;

            Game.World.SetDefaultBlock(x, y, z);
            Game.World.SetDefaultBlock(x, y + (isBase ? 1 : -1), z);
        }

        internal override void BlockUpdate(int x, int y, int z, uint data, BlockSide side)
        {
            // Check if this block is the lower part and if the ground supports plant growth.
            if (side == BlockSide.Bottom && (data & 0b1) == 0 && !((Game.World.GetBlock(x, y - 1, z, out _) ?? Block.Air) is IPlantable))
            {
                Destroy(x, y, z);
            }
        }

        public void LiquidChange(int x, int y, int z, Liquid liquid, LiquidLevel level)
        {
            if (liquid.Direction > 0 && level > LiquidLevel.Five) ScheduleDestroy(x, y, z);
        }
    }
}