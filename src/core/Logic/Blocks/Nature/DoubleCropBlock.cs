// <copyright file="DoubleCropBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    ///     A block which grows on farmland and has multiple growth stages, of which some are two blocks tall.
    ///     Data bit usage: <c>-lhsss</c>
    /// </summary>
    // l = lowered
    // s = stage
    // h = height
    public class DoubleCropBlock : Block, IFlammable, IFillable
    {
        private readonly string texture;

        private int dead, first, second, third;
        private (int low, int top) fourth, fifth, sixth, final;

        private int[] stageTextureIndicesLow = null!;
        private int[] stageTextureIndicesTop = null!;

        internal DoubleCropBlock(string name, string namedId, string texture, int dead, int first, int second,
            int third, (int low, int top) fourth, (int low, int top) fifth, (int low, int top) sixth,
            (int low, int top) final) :
            base(
                name,
                namedId,
                new BlockFlags(),
                BoundingBox.Block,
                TargetBuffer.CropPlant)
        {
            this.texture = texture;

            this.dead = dead;
            this.first = first;
            this.second = second;
            this.third = third;

            this.fourth = fourth;
            this.fifth = fifth;
            this.sixth = sixth;
            this.final = final;
        }

        public void LiquidChange(World world, Vector3i position, Liquid liquid, LiquidLevel level)
        {
            if (liquid.IsLiquid && level > LiquidLevel.Four) ScheduleDestroy(world, position);
        }

        protected override void Setup(ITextureIndexProvider indexProvider)
        {
            int baseIndex = indexProvider.GetTextureIndex(texture);

            if (baseIndex == 0)
            {
                dead = first = second = third = 0;
                fourth = fifth = sixth = final = (0, 0);
            }

            stageTextureIndicesLow = new[]
            {
                baseIndex + dead,
                baseIndex + first,
                baseIndex + second,
                baseIndex + third,
                baseIndex + fourth.low,
                baseIndex + fifth.low,
                baseIndex + sixth.low,
                baseIndex + final.low
            };

            stageTextureIndicesTop = new[]
            {
                0,
                0,
                0,
                0,
                baseIndex + fourth.top,
                baseIndex + fifth.top,
                baseIndex + sixth.top,
                baseIndex + final.top
            };
        }

        protected override BoundingBox GetBoundingBox(uint data)
        {
            var stage = (GrowthStage) (data & 0b00_0111);

            if ((data & 0b00_1000) == 0 && stage == GrowthStage.Initial ||
                (data & 0b00_1000) != 0 && stage is GrowthStage.Fourth or GrowthStage.Fifth)
                return BoundingBox.BlockWithHeight(height: 7);

            return BoundingBox.BlockWithHeight(height: 15);
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            var stageData = (int) (info.Data & 0b00_0111);

            bool isUpper = (info.Data & 0b00_1000) != 0;
            bool isLowered = (info.Data & 0b01_0000) != 0;
            bool hasUpper = (GrowthStage) stageData >= GrowthStage.Fourth;

            int textureIndex = !isUpper ? stageTextureIndicesLow[stageData] : stageTextureIndicesTop[stageData];

            return BlockMeshData.DoubleCropPlant(textureIndex, TintColor.None, hasUpper, isLowered, isUpper);
        }

        internal override bool CanPlace(World world, Vector3i position, PhysicsEntity? entity)
        {
            return world.GetBlock(position.Below(), out _) is IPlantable;
        }

        protected override void DoPlace(World world, Vector3i position, PhysicsEntity? entity)
        {
            bool isLowered = world.IsLowered(position);

            var data = (uint) GrowthStage.Initial;
            if (isLowered) data |= 0b01_0000;

            world.SetBlock(this, data, position);
        }

        internal override void DoDestroy(World world, Vector3i position, uint data, PhysicsEntity? entity)
        {
            world.SetDefaultBlock(position);

            bool isBase = (data & 0b00_1000) == 0;

            if ((data & 0b00_0111) >= (int) GrowthStage.Fourth)
                world.SetDefaultBlock(isBase ? position.Above() : position.Below());
        }

        internal override void BlockUpdate(World world, Vector3i position, uint data, BlockSide side)
        {
            // Check if this block is the lower part and if the ground supports plant growth.
            if (side == BlockSide.Bottom && (data & 0b00_1000) == 0 &&
                (world.GetBlock(position.Below(), out _) ?? Air) is not IPlantable) Destroy(world, position);
        }

        internal override void RandomUpdate(World world, Vector3i position, uint data)
        {
            var stage = (GrowthStage) (data & 0b00_0111);
            uint lowered = data & 0b01_0000;

            // If this block is the upper part, the random update is ignored.
            if ((data & 0b00_1000) != 0) return;

            if (world.GetBlock(position.Below(), out _) is IPlantable plantable)
            {
                if ((int) stage > 2 && !plantable.SupportsFullGrowth) return;

                if (stage != GrowthStage.Final && stage != GrowthStage.Dead)
                {
                    if (stage >= GrowthStage.Third)
                    {
                        Block? above = world.GetBlock(position.Above(), out _);

                        if (plantable.TryGrow(world, position.Below(), Liquid.Water, LiquidLevel.One) &&
                            ((above?.IsReplaceable ?? false) || above == this))
                        {
                            world.SetBlock(this, lowered | (uint) (stage + 1), position);

                            world.SetBlock(
                                this,
                                lowered | (uint) (0b00_1000 | ((int) stage + 1)),
                                position.Above());
                        }
                        else
                        {
                            world.SetBlock(this, lowered | (uint) GrowthStage.Dead, position);
                            if (stage != GrowthStage.Third) world.SetDefaultBlock(position.Above());
                        }
                    }
                    else
                    {
                        world.SetBlock(this, lowered | (uint) (stage + 1), position);
                    }
                }
            }
        }

        private enum GrowthStage
        {
            /// <summary>
            ///     One Block tall.
            /// </summary>
            Dead = 0,

            /// <summary>
            ///     One Block tall.
            /// </summary>
            Initial = 1,

            // Second

            /// <summary>
            ///     One Block tall.
            /// </summary>
            Third = 3,

            /// <summary>
            ///     Two blocks tall.
            /// </summary>
            Fourth = 4,

            /// <summary>
            ///     Two blocks tall.
            /// </summary>
            Fifth = 5,

            // Sixth

            /// <summary>
            ///     Two blocks tall.
            /// </summary>
            Final = 7
        }
    }
}