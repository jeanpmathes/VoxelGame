﻿// <copyright file="DoubleCropBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenTK.Mathematics;
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
    // l: lowered
    // s: stage
    // h: height
    public class DoubleCropBlock : Block, IFlammable, IFillable
    {
        private readonly string texture;

        private (
            int dead, int first, int second, int third,
            (int low, int top) fourth, (int low, int top) fifth, (int low, int top) sixth, (int low, int top) final
            ) stages;

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

            stages = (dead, first, second, third, fourth, fifth, sixth, final);
        }

        /// <inheritdoc />
        public void LiquidChange(World world, Vector3i position, Liquid liquid, LiquidLevel level)
        {
            if (liquid.IsLiquid && level > LiquidLevel.Four) ScheduleDestroy(world, position);
        }

        /// <inheritdoc />
        protected override void Setup(ITextureIndexProvider indexProvider)
        {
            int baseIndex = indexProvider.GetTextureIndex(texture);

            if (baseIndex == 0)
            {
                stages = (0, 0, 0, 0, (0, 0), (0, 0), (0, 0), (0, 0));
            }

            stageTextureIndicesLow = new[]
            {
                baseIndex + stages.dead,
                baseIndex + stages.first,
                baseIndex + stages.second,
                baseIndex + stages.third,
                baseIndex + stages.fourth.low,
                baseIndex + stages.fifth.low,
                baseIndex + stages.sixth.low,
                baseIndex + stages.final.low
            };

            stageTextureIndicesTop = new[]
            {
                0,
                0,
                0,
                0,
                baseIndex + stages.fourth.top,
                baseIndex + stages.fifth.top,
                baseIndex + stages.sixth.top,
                baseIndex + stages.final.top
            };
        }

        /// <inheritdoc />
        protected override BoundingBox GetBoundingBox(uint data)
        {
            var stage = (GrowthStage) (data & 0b00_0111);

            bool isLowerAndStillGrowing = (data & 0b00_1000) == 0 && stage == GrowthStage.Initial;
            bool isUpperAndStillGrowing = (data & 0b00_1000) != 0 && stage is GrowthStage.Fourth or GrowthStage.Fifth;

            if (isLowerAndStillGrowing || isUpperAndStillGrowing)
                return BoundingBox.BlockWithHeight(height: 7);

            return BoundingBox.BlockWithHeight(height: 15);
        }

        /// <inheritdoc />
        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            var stageData = (int) (info.Data & 0b00_0111);

            bool isUpper = (info.Data & 0b00_1000) != 0;
            bool isLowered = (info.Data & 0b01_0000) != 0;
            bool hasUpper = (GrowthStage) stageData >= GrowthStage.Fourth;

            int textureIndex = !isUpper ? stageTextureIndicesLow[stageData] : stageTextureIndicesTop[stageData];

            return BlockMeshData.DoubleCropPlant(textureIndex, TintColor.None, hasUpper, isLowered, isUpper);
        }

        /// <inheritdoc />
        public override bool CanPlace(World world, Vector3i position, PhysicsEntity? entity)
        {
            return world.GetBlock(position.Below())?.Block is IPlantable;
        }

        /// <inheritdoc />
        protected override void DoPlace(World world, Vector3i position, PhysicsEntity? entity)
        {
            bool isLowered = world.IsLowered(position);

            var data = (uint) GrowthStage.Initial;
            if (isLowered) data |= 0b01_0000;

            world.SetBlock(this.AsInstance(data), position);
        }

        /// <inheritdoc />
        protected override void DoDestroy(World world, Vector3i position, uint data, PhysicsEntity? entity)
        {
            world.SetDefaultBlock(position);

            bool isBase = (data & 0b00_1000) == 0;

            if ((data & 0b00_0111) >= (int) GrowthStage.Fourth)
                world.SetDefaultBlock(isBase ? position.Above() : position.Below());
        }

        /// <inheritdoc />
        public override void BlockUpdate(World world, Vector3i position, uint data, BlockSide side)
        {
            // Check if this block is the lower part and if the ground supports plant growth.
            if (side == BlockSide.Bottom && (data & 0b00_1000) == 0 &&
                (world.GetBlock(position.Below())?.Block ?? Air) is not IPlantable) Destroy(world, position);
        }

        /// <inheritdoc />
        public override void RandomUpdate(World world, Vector3i position, uint data)
        {
            var stage = (GrowthStage) (data & 0b00_0111);
            uint lowered = data & 0b01_0000;

            // If this block is the upper part, the random update is ignored.
            if ((data & 0b00_1000) != 0) return;

            if (world.GetBlock(position.Below())?.Block is not IPlantable plantable) return;
            if ((int) stage > 2 && !plantable.SupportsFullGrowth) return;
            if (stage is GrowthStage.Final or GrowthStage.Dead) return;

            if (stage >= GrowthStage.Third) GrowBothParts(world, position, plantable, lowered, stage);
            else world.SetBlock(this.AsInstance(lowered | (uint) (stage + 1)), position);
        }

        private void GrowBothParts(World world, Vector3i position, IPlantable plantable, uint lowered,
            GrowthStage stage)
        {
            BlockInstance? above = world.GetBlock(position.Above());

            if (plantable.TryGrow(world, position.Below(), Liquid.Water, LiquidLevel.One) &&
                ((above?.Block.IsReplaceable ?? false) || above?.Block == this))
            {
                world.SetBlock(this.AsInstance(lowered | (uint) (stage + 1)), position);

                world.SetBlock(
                    this.AsInstance(lowered | (uint) (0b00_1000 | ((int) stage + 1))),
                    position.Above());
            }
            else
            {
                world.SetBlock(this.AsInstance(lowered | (uint) GrowthStage.Dead), position);
                if (stage != GrowthStage.Third) world.SetDefaultBlock(position.Above());
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
