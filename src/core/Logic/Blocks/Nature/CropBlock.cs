// <copyright file="CropBlock.cs" company="VoxelGame">
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
    /// A block which grows on farmland and has multiple growth stages.
    /// Data bit usage: <c>--lsss</c>
    /// </summary>
    // l = lowered
    // s = stage
    public class CropBlock : Block, IFlammable, IFillable
    {
        private int[] stageTextureIndices = null!;

        private readonly string texture;
        private int second, third, fourth, fifth, sixth, final, dead;

        internal CropBlock(string name, string namedId, string texture, int second, int third, int fourth, int fifth, int sixth, int final, int dead) :
            base(
                name,
                namedId,
                isFull: false,
                isOpaque: false,
                renderFaceAtNonOpaques: true,
                isSolid: false,
                receiveCollisions: false,
                isTrigger: false,
                isReplaceable: false,
                isInteractable: false,
                BoundingBox.Block,
                TargetBuffer.CropPlant)
        {
            this.texture = texture;
            this.second = second;
            this.third = third;
            this.fourth = fourth;
            this.fifth = fifth;
            this.sixth = sixth;
            this.final = final;
            this.dead = dead;
        }

        protected override void Setup(ITextureIndexProvider indexProvider)
        {
            int baseIndex = indexProvider.GetTextureIndex(texture);

            if (baseIndex == 0)
            {
                second = third = fourth = fifth = sixth = final = dead = 0;
            }

            stageTextureIndices = new[]
            {
                baseIndex,
                baseIndex + second,
                baseIndex + third,
                baseIndex + fourth,
                baseIndex + fifth,
                baseIndex + sixth,
                baseIndex + final,
                baseIndex + dead
            };
        }

        protected override BoundingBox GetBoundingBox(uint data)
        {
            switch ((GrowthStage) (data & 0b00_0111))
            {
                case GrowthStage.Initial:
                case GrowthStage.Dead:
                    return BoundingBox.BlockWithHeight(3);

                case GrowthStage.Second:
                    return BoundingBox.BlockWithHeight(5);

                case GrowthStage.Third:
                    return BoundingBox.BlockWithHeight(7);

                case GrowthStage.Fourth:
                    return BoundingBox.BlockWithHeight(9);

                case GrowthStage.Fifth:
                    return BoundingBox.BlockWithHeight(11);

                case GrowthStage.Sixth:
                    return BoundingBox.BlockWithHeight(13);

                case GrowthStage.Final:
                    return BoundingBox.BlockWithHeight(15);
            }

            return BoundingBox.Block;
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            int textureIndex = stageTextureIndices[info.Data & 0b00_0111];
            bool isLowered = (info.Data & 0b00_1000) != 0;

            return BlockMeshData.CropPlant(textureIndex, TintColor.None, isLowered, false);
        }

        internal override bool CanPlace(World world, int x, int y, int z, PhysicsEntity? entity)
        {
            return world.GetBlock(x, y - 1, z, out _) is IPlantable;
        }

        protected override void DoPlace(World world, int x, int y, int z, PhysicsEntity? entity)
        {
            bool isLowered = world.IsLowered(x, y, z);

            var data = (uint) GrowthStage.Initial;
            if (isLowered) data |= 0b00_1000;

            world.SetBlock(this, data, x, y, z);
        }

        internal override void BlockUpdate(World world, int x, int y, int z, uint data, BlockSide side)
        {
            if (side == BlockSide.Bottom && world.GetBlock(x, y - 1, z, out _) is not IPlantable)
            {
                Destroy(world, x, y, z);
            }
        }

        internal override void RandomUpdate(World world, int x, int y, int z, uint data)
        {
            var stage = (GrowthStage) (data & 0b00_0111);
            uint lowered = data & 0b00_1000;

            if (stage != GrowthStage.Final && stage != GrowthStage.Dead && world.GetBlock(x, y - 1, z, out _) is IPlantable plantable)
            {
                if ((int) stage > 2)
                {
                    if (!plantable.SupportsFullGrowth) return;

                    if (!plantable.TryGrow(world, x, y - 1, z, Liquid.Water, LiquidLevel.One))
                    {
                        world.SetBlock(this, lowered | (uint) GrowthStage.Dead, x, y, z);

                        return;
                    }
                }

                world.SetBlock(this, lowered | (uint) (stage + 1), x, y, z);
            }
        }

        public void LiquidChange(World world, int x, int y, int z, Liquid liquid, LiquidLevel level)
        {
            if (liquid.Direction > 0 && level > LiquidLevel.Three) ScheduleDestroy(world, x, y, z);
        }

        private enum GrowthStage
        {
            Initial,
            Second,
            Third,
            Fourth,
            Fifth,
            Sixth,
            Final,
            Dead
        }
    }
}