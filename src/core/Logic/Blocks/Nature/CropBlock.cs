// <copyright file="CropBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenToolkit.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    ///     A block which grows on farmland and has multiple growth stages.
    ///     Data bit usage: <c>--lsss</c>
    /// </summary>
    // l: lowered
    // s: stage
    public class CropBlock : Block, IFlammable, IFillable
    {
        private readonly string texture;
        private (int second, int third, int fourth, int fifth, int sixth, int final, int dead) stages;
        private int[] stageTextureIndices = null!;

        internal CropBlock(string name, string namedId, string texture, int second, int third, int fourth, int fifth,
            int sixth, int final, int dead) :
            base(
                name,
                namedId,
                new BlockFlags(),
                BoundingBox.Block,
                TargetBuffer.CropPlant)
        {
            this.texture = texture;

            stages = (second, third, fourth, fifth, sixth, final, dead);
        }

        /// <inheritdoc />
        public void LiquidChange(World world, Vector3i position, Liquid liquid, LiquidLevel level)
        {
            if (liquid.IsLiquid && level > LiquidLevel.Three) ScheduleDestroy(world, position);
        }

        /// <inheritdoc />
        protected override void Setup(ITextureIndexProvider indexProvider)
        {
            int baseIndex = indexProvider.GetTextureIndex(texture);

            if (baseIndex == 0) stages = (0, 0, 0, 0, 0, 0, 0);

            stageTextureIndices = new[]
            {
                baseIndex,
                baseIndex + stages.second,
                baseIndex + stages.third,
                baseIndex + stages.fourth,
                baseIndex + stages.fifth,
                baseIndex + stages.sixth,
                baseIndex + stages.final,
                baseIndex + stages.dead
            };
        }

        /// <inheritdoc />
        protected override BoundingBox GetBoundingBox(uint data)
        {
            switch ((GrowthStage) (data & 0b00_0111))
            {
                case GrowthStage.Initial:
                case GrowthStage.Dead:
                    return BoundingBox.BlockWithHeight(height: 3);

                case GrowthStage.Second:
                    return BoundingBox.BlockWithHeight(height: 5);

                case GrowthStage.Third:
                    return BoundingBox.BlockWithHeight(height: 7);

                case GrowthStage.Fourth:
                    return BoundingBox.BlockWithHeight(height: 9);

                case GrowthStage.Fifth:
                    return BoundingBox.BlockWithHeight(height: 11);

                case GrowthStage.Sixth:
                    return BoundingBox.BlockWithHeight(height: 13);

                case GrowthStage.Final:
                    return BoundingBox.BlockWithHeight(height: 15);

                default: throw new InvalidOperationException();
            }
        }

        /// <inheritdoc />
        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            int textureIndex = stageTextureIndices[info.Data & 0b00_0111];
            bool isLowered = (info.Data & 0b00_1000) != 0;

            return BlockMeshData.CropPlant(textureIndex, TintColor.None, isLowered, isUpper: false);
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
            if (isLowered) data |= 0b00_1000;

            world.SetBlock(this.AsInstance(data), position);
        }

        /// <inheritdoc />
        public override void BlockUpdate(World world, Vector3i position, uint data, BlockSide side)
        {
            if (side == BlockSide.Bottom && world.GetBlock(position.Below())?.Block is not IPlantable)
                Destroy(world, position);
        }

        /// <inheritdoc />
        public override void RandomUpdate(World world, Vector3i position, uint data)
        {
            var stage = (GrowthStage) (data & 0b00_0111);
            uint lowered = data & 0b00_1000;

            if (stage != GrowthStage.Final && stage != GrowthStage.Dead &&
                world.GetBlock(position.Below())?.Block is IPlantable plantable)
            {
                if ((int) stage > 2)
                {
                    if (!plantable.SupportsFullGrowth) return;

                    if (!plantable.TryGrow(world, position.Below(), Liquid.Water, LiquidLevel.One))
                    {
                        world.SetBlock(this.AsInstance(lowered | (uint) GrowthStage.Dead), position);

                        return;
                    }
                }

                world.SetBlock(this.AsInstance(lowered | (uint) (stage + 1)), position);
            }
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
