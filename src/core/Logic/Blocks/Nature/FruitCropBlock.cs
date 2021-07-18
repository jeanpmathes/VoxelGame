// <copyright file="FruitCropBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Physics;
using OpenToolkit.Mathematics;
using System;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// A block that places a fruit block when reaching the final growth stage.
    /// Data bit usage: <c>-ssscc</c>
    /// </summary>
    // s = stage
    // c = connection (orientation)
    public class FruitCropBlock : Block, IFlammable, IFillable
    {
        private readonly string texture;
        private readonly Block fruit;

        private readonly BlockModel crossBase;
        private readonly BlockModel[] extensions;

        private (int dead, int initial, int noFruit, int withFruit, int connector) baseTextures;

        internal FruitCropBlock(string name, string namedId, string texture, string baseModel, string extensionModel, Block fruit) :
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
                new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.175f, 0.5f, 0.175f)),
                TargetBuffer.Complex)
        {
            this.texture = texture;
            this.fruit = fruit;

            crossBase = BlockModel.Load(baseModel);

            BlockModel extension = BlockModel.Load(extensionModel);
            (BlockModel north, BlockModel east, BlockModel south, BlockModel west) directions = extension.CreateAllDirections(false);

            crossBase.Lock();
            directions.Lock();

            extensions = new[] { directions.north, directions.east, directions.south, directions.west };
        }

        protected override void Setup(ITextureIndexProvider indexProvider)
        {
            int baseTextureIndex = indexProvider.GetTextureIndex(texture);

            baseTextures = (
                baseTextureIndex + 0,
                baseTextureIndex + 1,
                baseTextureIndex + 2,
                baseTextureIndex + 3,
                baseTextureIndex + 4);
        }

        protected override BoundingBox GetBoundingBox(uint data)
        {
            var stage = (GrowthStage)((data >> 2) & 0b111);

            return stage < GrowthStage.First
                ? new BoundingBox(new Vector3(0.5f, 0.25f, 0.5f), new Vector3(0.175f, 0.25f, 0.175f))
                : new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.175f, 0.5f, 0.175f));
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            var stage = (GrowthStage)((info.Data >> 2) & 0b111);

            int textureIndex = stage switch
            {
                GrowthStage.Dead => baseTextures.dead,
                GrowthStage.Young => baseTextures.initial,
                GrowthStage.First => baseTextures.initial,
                GrowthStage.Second => baseTextures.noFruit,
                GrowthStage.Third => baseTextures.noFruit,
                GrowthStage.Fourth => baseTextures.noFruit,
                GrowthStage.BeforeFruit => baseTextures.noFruit,
                GrowthStage.WithFruit => baseTextures.withFruit,
                _ => 0,
            };

            if (stage != GrowthStage.WithFruit)
            {
                crossBase.ToData(out float[] vertices, out int[] textureIndices, out uint[] indices);
                Array.Fill(textureIndices, textureIndex);
                return new BlockMeshData((uint)crossBase.VertexCount, vertices, textureIndices, indices);
            }
            else
            {
                var orientation = (Orientation)(info.Data & 0b11);
                var (vertices, textureIndices, indices) = BlockModel.CombineData(out uint vertexCount, crossBase, extensions[(int)orientation]);

                Array.Fill(textureIndices, textureIndex, 0, crossBase.VertexCount);
                Array.Fill(textureIndices, baseTextures.connector, crossBase.VertexCount, extensions[0].VertexCount);

                return new BlockMeshData(vertexCount, vertices, textureIndices, indices);
            }
        }

        internal override bool CanPlace(World world, int x, int y, int z, PhysicsEntity? entity)
        {
            return world.GetBlock(x, y - 1, z, out _) is IPlantable;
        }

        protected override void DoPlace(World world, int x, int y, int z, PhysicsEntity? entity)
        {
            world.SetBlock(this, (int)GrowthStage.First << 2, x, y, z);
        }

        internal override void BlockUpdate(World world, int x, int y, int z, uint data, BlockSide side)
        {
            var orientation = (Orientation)(data & 0b11);

            if (side == BlockSide.Bottom)
            {
                if (!(world.GetBlock(x, y - 1, z, out _) is IPlantable))
                {
                    Destroy(world, x, y, z);
                }
            }
            else if (side == orientation.ToBlockSide() && (GrowthStage)((data >> 2) & 0b111) == GrowthStage.WithFruit)
            {
                switch (orientation)
                {
                    case Orientation.North:

                        CheckFruit(x, y, z - 1);
                        break;

                    case Orientation.East:

                        CheckFruit(x + 1, y, z);
                        break;

                    case Orientation.South:

                        CheckFruit(x, y, z + 1);
                        break;

                    case Orientation.West:

                        CheckFruit(x - 1, y, z);
                        break;
                }
            }

            void CheckFruit(int fx, int fy, int fz)
            {
                if (world.GetBlock(fx, fy, fz, out _) != fruit)
                {
                    world.SetBlock(this, (int)GrowthStage.First << 2, x, y, z);
                }
            }
        }

        internal override void RandomUpdate(World world, int x, int y, int z, uint data)
        {
            if (!(world.GetBlock(x, y - 1, z, out _) is IPlantable ground)) return;

            var stage = (GrowthStage)((data >> 2) & 0b111);

            if (stage != GrowthStage.Dead && stage < GrowthStage.BeforeFruit)
            {
                world.SetBlock(this, (uint)((int)(stage + 1) << 2), x, y, z);
            }
            else if (stage == GrowthStage.BeforeFruit && ground.SupportsFullGrowth && ground.TryGrow(world, x, y - 1, z, Liquid.Water, LiquidLevel.Two))
            {
                if (fruit.Place(world, x, y, z - 1))
                {
                    world.SetBlock(this, (uint)GrowthStage.WithFruit << 2 | (uint)Orientation.North, x, y, z);
                }
                else if (fruit.Place(world, x + 1, y, z))
                {
                    world.SetBlock(this, (uint)GrowthStage.WithFruit << 2 | (uint)Orientation.East, x, y, z);
                }
                else if (fruit.Place(world, x, y, z + 1))
                {
                    world.SetBlock(this, (uint)GrowthStage.WithFruit << 2 | (uint)Orientation.South, x, y, z);
                }
                else if (fruit.Place(world, x - 1, y, z))
                {
                    world.SetBlock(this, (uint)GrowthStage.WithFruit << 2 | (uint)Orientation.West, x, y, z);
                }
            }
            else if (stage == GrowthStage.BeforeFruit && ground.SupportsFullGrowth)
            {
                world.SetBlock(this, (uint)GrowthStage.Dead << 2, x, y, z);
            }
        }

        public void LiquidChange(World world, int x, int y, int z, Liquid liquid, LiquidLevel level)
        {
            if (liquid.Direction > 0 && level > LiquidLevel.Three) ScheduleDestroy(world, x, y, z);
        }

        private enum GrowthStage
        {
            Dead,
            Young,
            First,
            Second,
            Third,
            Fourth,
            BeforeFruit,
            WithFruit
        }
    }
}