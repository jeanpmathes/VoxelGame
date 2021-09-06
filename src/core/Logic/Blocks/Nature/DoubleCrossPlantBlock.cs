// <copyright file="DoubleCrossPlantBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// Similar to <see cref="CrossPlantBlock"/>, but is two blocks high.
    /// Data bit usage: <c>----lh</c>
    /// </summary>
    // l = lowered
    // h = height
    public class DoubleCrossPlantBlock : Block, IFlammable, IFillable
    {
        private readonly string bottomTexture;
        private readonly int topTexOffset;

        private int bottomTextureIndex;
        private int topTextureIndex;

        internal DoubleCrossPlantBlock(string name, string namedId, string bottomTexture, int topTexOffset,
            BoundingBox boundingBox) :
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
                TargetBuffer.CrossPlant)
        {
            this.bottomTexture = bottomTexture;
            this.topTexOffset = topTexOffset;
        }

        protected override void Setup(ITextureIndexProvider indexProvider)
        {
            bottomTextureIndex = indexProvider.GetTextureIndex(bottomTexture);
            topTextureIndex = bottomTextureIndex + topTexOffset;
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            bool isUpper = (info.Data & 0b01) != 0;
            bool isLowered = (info.Data & 0b10) != 0;

            return BlockMeshData.CrossPlant(
                isUpper ? topTextureIndex : bottomTextureIndex,
                TintColor.Neutral,
                true,
                isLowered,
                isUpper);
        }

        internal override bool CanPlace(World world, int x, int y, int z, PhysicsEntity? entity)
        {
            return world.GetBlock(x, y + 1, z, out _)?.IsReplaceable == true &&
                   (world.GetBlock(x, y - 1, z, out _) ?? Block.Air) is IPlantable;
        }

        protected override void DoPlace(World world, int x, int y, int z, PhysicsEntity? entity)
        {
            bool isLowered = world.IsLowered(x, y, z);

            uint data = (isLowered ? 1u : 0u) << 1;

            world.SetBlock(this, data, x, y, z);
            world.SetBlock(this, data | 1, x, y + 1, z);
        }

        internal override void DoDestroy(World world, int x, int y, int z, uint data, PhysicsEntity? entity)
        {
            bool isBase = (data & 0b1) == 0;

            world.SetDefaultBlock(x, y, z);
            world.SetDefaultBlock(x, y + (isBase ? 1 : -1), z);
        }

        internal override void BlockUpdate(World world, int x, int y, int z, uint data, BlockSide side)
        {
            // Check if this block is the lower part and if the ground supports plant growth.
            if (side == BlockSide.Bottom && (data & 0b1) == 0 &&
                !((world.GetBlock(x, y - 1, z, out _) ?? Block.Air) is IPlantable))
            {
                Destroy(world, x, y, z);
            }
        }

        public void LiquidChange(World world, int x, int y, int z, Liquid liquid, LiquidLevel level)
        {
            if (liquid.Direction > 0 && level > LiquidLevel.Five) ScheduleDestroy(world, x, y, z);
        }
    }
}