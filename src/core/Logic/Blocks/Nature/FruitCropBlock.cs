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
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.175f, 0.5f, 0.175f)),
                TargetBuffer.CrossPlant)
        {
            this.texture = texture;
            this.fruit = fruit;
        }

        public void LiquidChange(World world, int x, int y, int z, Liquid liquid, LiquidLevel level)
        {
            if (liquid.Direction > 0 && level > LiquidLevel.Three) ScheduleDestroy(world, x, y, z);
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
                ? new BoundingBox(new Vector3(0.5f, 0.25f, 0.5f), new Vector3(0.175f, 0.25f, 0.175f))
                : new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.175f, 0.5f, 0.175f));
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

            return BlockMeshData.CrossPlant(index, TintColor.None, false, (info.Data & 0b1) == 1, false);
        }

        internal override bool CanPlace(World world, int x, int y, int z, PhysicsEntity? entity)
        {
            Block ground = world.GetBlock(x, y - 1, z, out _) ?? Air;

            return ground is IPlantable;
        }

        protected override void DoPlace(World world, int x, int y, int z, PhysicsEntity? entity)
        {
            bool isLowered = world.IsLowered(x, y, z);
            world.SetBlock(this, isLowered ? 1u : 0u, x, y, z);
        }

        internal override void BlockUpdate(World world, int x, int y, int z, uint data, BlockSide side)
        {
            if (side == BlockSide.Bottom && !((world.GetBlock(x, y - 1, z, out _) ?? Air) is IPlantable))
                Destroy(world, x, y, z);
        }

        internal override void RandomUpdate(World world, int x, int y, int z, uint data)
        {
            if (!(world.GetBlock(x, y - 1, z, out _) is IPlantable ground)) return;

            var stage = (GrowthStage) ((data >> 1) & 0b111);

            if (stage < GrowthStage.Ready)
            {
                world.SetBlock(this, (uint) ((int) (stage + 1) << 1), x, y, z);
            }
            else if (stage == GrowthStage.Ready && ground.SupportsFullGrowth && ground.TryGrow(
                world,
                x,
                y - 1,
                z,
                Liquid.Water,
                LiquidLevel.Two))
            {
                if (fruit.Place(world, x, y, z - 1))
                    world.SetBlock(this, (uint) GrowthStage.Second << 1, x, y, z); // North.
                else if (fruit.Place(world, x + 1, y, z))
                    world.SetBlock(this, (uint) GrowthStage.Second << 1, x, y, z); // East.
                else if (fruit.Place(world, x, y, z + 1))
                    world.SetBlock(this, (uint) GrowthStage.Second << 1, x, y, z); // South.
                else if (fruit.Place(world, x - 1, y, z))
                    world.SetBlock(this, (uint) GrowthStage.Second << 1, x, y, z); // West.
            }
            else if (stage == GrowthStage.Ready && ground.SupportsFullGrowth)
            {
                world.SetBlock(this, (uint) GrowthStage.Dead << 1, x, y, z);
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