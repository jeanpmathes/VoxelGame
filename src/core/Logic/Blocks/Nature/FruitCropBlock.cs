// <copyright file="FruitCropBlock.cs" company="VoxelGame">
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
    ///     A block that places a fruit block when reaching the final growth stage.
    ///     Data bit usage: <c>--sssl</c>
    /// </summary>
    // l = lowered
    // s = stage
    public class FruitCropBlock : Block, IFlammable, IFillable
    {
        private readonly Block fruit;
        private readonly string texture;

        private (int dead, int initial, int last) textureIndex;

        internal FruitCropBlock(string name, string namedId, string texture, Block fruit) :
            base(
                name,
                namedId,
                new BlockFlags(),
                new BoundingBox(new Vector3(x: 0.5f, y: 0.5f, z: 0.5f), new Vector3(x: 0.175f, y: 0.5f, z: 0.175f)),
                TargetBuffer.CrossPlant)
        {
            this.texture = texture;
            this.fruit = fruit;
        }

        public void LiquidChange(World world, Vector3i position, Liquid liquid, LiquidLevel level)
        {
            if (liquid.IsLiquid && level > LiquidLevel.Three) ScheduleDestroy(world, position);
        }

        protected override void Setup(ITextureIndexProvider indexProvider)
        {
            int baseTextureIndex = indexProvider.GetTextureIndex(texture);

            textureIndex =
            (
                baseTextureIndex + 0,
                baseTextureIndex + 1,
                baseTextureIndex + 2
            );
        }

        protected override BoundingBox GetBoundingBox(uint data)
        {
            var stage = (GrowthStage) ((data >> 2) & 0b111);

            return stage < GrowthStage.First
                ? new BoundingBox(new Vector3(x: 0.5f, y: 0.25f, z: 0.5f), new Vector3(x: 0.175f, y: 0.25f, z: 0.175f))
                : new BoundingBox(new Vector3(x: 0.5f, y: 0.5f, z: 0.5f), new Vector3(x: 0.175f, y: 0.5f, z: 0.175f));
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            var stage = (GrowthStage) ((info.Data >> 2) & 0b111);

            int index = stage switch
            {
                GrowthStage.Initial => textureIndex.initial,
                GrowthStage.First => textureIndex.initial,
                GrowthStage.Second => textureIndex.last,
                GrowthStage.Third => textureIndex.last,
                GrowthStage.Fourth => textureIndex.last,
                GrowthStage.Fifth => textureIndex.last,
                GrowthStage.Ready => textureIndex.last,
                GrowthStage.Dead => textureIndex.dead,
                _ => 0
            };

            return BlockMeshData.CrossPlant(
                index,
                TintColor.None,
                hasUpper: false,
                (info.Data & 0b1) == 1,
                isUpper: false);
        }

        internal override bool CanPlace(World world, Vector3i position, PhysicsEntity? entity)
        {
            Block ground = world.GetBlock(position.Below(), out _) ?? Air;

            return ground is IPlantable;
        }

        protected override void DoPlace(World world, Vector3i position, PhysicsEntity? entity)
        {
            bool isLowered = world.IsLowered(position);
            world.SetBlock(this, isLowered ? 1u : 0u, position);
        }

        internal override void BlockUpdate(World world, Vector3i position, uint data, BlockSide side)
        {
            if (side == BlockSide.Bottom && (world.GetBlock(position.Below(), out _) ?? Air) is not IPlantable)
                Destroy(world, position);
        }

        internal override void RandomUpdate(World world, Vector3i position, uint data)
        {
            if (world.GetBlock(position.Below(), out _) is not IPlantable ground) return;

            var stage = (GrowthStage) ((data >> 1) & 0b111);

            switch (stage)
            {
                case < GrowthStage.Ready:
                    world.SetBlock(this, (uint) ((int) (stage + 1) << 1), position);

                    break;
                case GrowthStage.Ready when ground.SupportsFullGrowth && ground.TryGrow(
                    world,
                    position.Below(),
                    Liquid.Water,
                    LiquidLevel.Two):
                {
                    foreach (Orientation orientation in Orientations.ShuffledStart(position))
                    {
                        if (!fruit.Place(world, orientation.Offset(position))) continue;
                        world.SetBlock(this, (uint) GrowthStage.Second << 1, position);

                        break;
                    }

                    break;
                }
                case GrowthStage.Ready when ground.SupportsFullGrowth:
                    world.SetBlock(this, (uint) GrowthStage.Dead << 1, position);

                    break;
            }
        }

        private enum GrowthStage
        {
            Initial,
            First,
            Second,
            Third,
            Fourth,
            Fifth,
            Ready,
            Dead
        }
    }
}