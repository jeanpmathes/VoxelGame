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
    public class FruitCropBlock : CrossBlock, IFlammable, IFillable
    {
        private protected float[][] verticesConnected = null!;

        private protected int[][] texIndices = null!;

        private protected uint[] indicesConnected = null!;

        private protected readonly Block fruit;

        private protected int dead, initial, noFruit, withFruit, connector;

        public FruitCropBlock(string name, string namedId, string texture, int dead, int initial, int noFruit, int withFruit, int connector, Block fruit) :
            base(
                name,
                namedId,
                texture,
                receiveCollisions: false,
                isTrigger: false,
                isReplaceable: false,
                new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.175f, 0.5f, 0.175f)))
        {
            this.fruit = fruit;

            this.dead = dead;
            this.initial = initial;
            this.noFruit = noFruit;
            this.withFruit = withFruit;
            this.connector = connector;
        }

        protected override void Setup()
        {
            base.Setup();

            verticesConnected = new float[4][];

            verticesConnected[0] = new float[96];
            Array.Copy(vertices, verticesConnected[0], vertices.Length);
            Array.Copy(new float[] {
                0.5f, 0f, 1f, 0f, 0f, 0f, 0f, 0f,
                0.5f, 1f, 1f, 0f, 1f, 0f, 0f, 0f,
                0.5f, 1f, 0f, 1f, 1f, 0f, 0f, 0f,
                0.5f, 0f, 0f, 1f, 0f, 0f, 0f, 0f
            }, 0, verticesConnected[0], 64, 32);

            verticesConnected[1] = new float[96];
            Array.Copy(vertices, verticesConnected[1], vertices.Length);
            Array.Copy(new float[] {
                0f, 0f, 0.5f, 0f, 0f, 0f, 0f, 0f,
                0f, 1f, 0.5f, 0f, 1f, 0f, 0f, 0f,
                1f, 1f, 0.5f, 1f, 1f, 0f, 0f, 0f,
                1f, 0f, 0.5f, 1f, 0f, 0f, 0f, 0f
            }, 0, verticesConnected[1], 64, 32);

            verticesConnected[2] = new float[96];
            Array.Copy(vertices, verticesConnected[2], vertices.Length);
            Array.Copy(new float[] {
                0.5f, 0f, 1f, 1f, 0f, 0f, 0f, 0f,
                0.5f, 1f, 1f, 1f, 1f, 0f, 0f, 0f,
                0.5f, 1f, 0f, 0f, 1f, 0f, 0f, 0f,
                0.5f, 0f, 0f, 0f, 0f, 0f, 0f, 0f
            }, 0, verticesConnected[2], 64, 32);

            verticesConnected[3] = new float[96];
            Array.Copy(vertices, verticesConnected[3], vertices.Length);
            Array.Copy(new float[] {
                0f, 0f, 0.5f, 1f, 0f, 0f, 0f, 0f,
                0f, 1f, 0.5f, 1f, 1f, 0f, 0f, 0f,
                1f, 1f, 0.5f, 0f, 1f, 0f, 0f, 0f,
                1f, 0f, 0.5f, 0f, 0f, 0f, 0f, 0f
            }, 0, verticesConnected[3], 64, 32);

            int tex = Game.BlockTextures.GetTextureIndex(texture);

            if (tex == 0)
            {
                dead = initial = noFruit = withFruit = connector = 0;
            }

            texIndices = new int[][]
            {
                new int[] {tex + dead, tex + dead, tex + dead, tex + dead, tex + dead, tex + dead, tex + dead, tex + dead},
                new int[] {tex + initial, tex + initial, tex + initial, tex + initial, tex + initial, tex + initial, tex + initial, tex + initial},
                new int[] {tex + noFruit, tex + noFruit, tex + noFruit, tex + noFruit, tex + noFruit, tex + noFruit, tex + noFruit, tex + noFruit},
                new int[] {tex + withFruit, tex + withFruit, tex + withFruit, tex + withFruit, tex + withFruit, tex + withFruit, tex + withFruit, tex + withFruit, tex + connector, tex + connector, tex + connector, tex + connector},
            };

            indicesConnected = new uint[36];
            Array.Copy(indices, indicesConnected, indices.Length);
            Array.Copy(new uint[] {
                8, 10, 9,
                8, 11, 10,
                8, 9, 10,
                8, 10, 11
            }, 0, indicesConnected, 24, 12);
        }

        protected override BoundingBox GetBoundingBox(int x, int y, int z, uint data)
        {
            GrowthStage stage = (GrowthStage)((data >> 2) & 0b111);

            if (stage < GrowthStage.First)
            {
                return new BoundingBox(new Vector3(0.5f, 0.25f, 0.5f) + new Vector3(x, y, z), new Vector3(0.175f, 0.25f, 0.175f));
            }
            else
            {
                return new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f) + new Vector3(x, y, z), new Vector3(0.175f, 0.5f, 0.175f));
            }
        }

        public override uint GetMesh(BlockSide side, uint data, Liquid liquid, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint, out bool isAnimated)
        {
            GrowthStage stage = (GrowthStage)((data >> 2) & 0b111);

            if (stage != GrowthStage.WithFruit)
            {
                vertices = this.vertices;
                textureIndices = texIndices[stage < GrowthStage.Second ? (int)stage : 2];
                indices = this.indices;
                tint = TintColor.None;
                isAnimated = false;

                return 8;
            }
            else
            {
                Orientation orientation = (Orientation)(data & 0b11);

                vertices = verticesConnected[(int)orientation];
                textureIndices = texIndices[3];
                indices = indicesConnected;
                tint = TintColor.None;
                isAnimated = false;

                return 12;
            }
        }

        protected override bool Place(PhysicsEntity? entity, int x, int y, int z)
        {
            if (!(Game.World.GetBlock(x, y - 1, z, out _) is IPlantable))
            {
                return false;
            }

            Game.World.SetBlock(this, (int)GrowthStage.Young << 2, x, y, z);

            return true;
        }

        internal override void BlockUpdate(int x, int y, int z, uint data, BlockSide side)
        {
            Orientation orientation = (Orientation)(data & 0b11);

            if (side == BlockSide.Bottom)
            {
                if (!(Game.World.GetBlock(x, y - 1, z, out _) is IPlantable))
                {
                    Destroy(x, y, z);
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
                if (Game.World.GetBlock(fx, fy, fz, out _) != fruit)
                {
                    Game.World.SetBlock(this, (int)GrowthStage.First << 2, x, y, z);
                }
            }
        }

        internal override void RandomUpdate(int x, int y, int z, uint data)
        {
            IPlantable? ground = Game.World.GetBlock(x, y - 1, z, out _) as IPlantable;

            if (ground == null) return;

            GrowthStage stage = (GrowthStage)((data >> 2) & 0b111);

            if (stage != GrowthStage.Dead && stage < GrowthStage.BeforeFruit)
            {
                Game.World.SetBlock(this, (uint)((int)(stage + 1) << 2), x, y, z);
            }
            else if (stage == GrowthStage.BeforeFruit && ground.SupportsFullGrowth && ground.TryGrow(x, y - 1, z, Liquid.Water, LiquidLevel.Two))
            {
                if (fruit.Place(x, y, z - 1))
                {
                    Game.World.SetBlock(this, (uint)GrowthStage.WithFruit << 2 | (uint)Orientation.North, x, y, z);
                }
                else if (fruit.Place(x + 1, y, z))
                {
                    Game.World.SetBlock(this, (uint)GrowthStage.WithFruit << 2 | (uint)Orientation.East, x, y, z);
                }
                else if (fruit.Place(x, y, z + 1))
                {
                    Game.World.SetBlock(this, (uint)GrowthStage.WithFruit << 2 | (uint)Orientation.South, x, y, z);
                }
                else if (fruit.Place(x - 1, y, z))
                {
                    Game.World.SetBlock(this, (uint)GrowthStage.WithFruit << 2 | (uint)Orientation.West, x, y, z);
                }
            }
            else if (stage == GrowthStage.BeforeFruit && ground.SupportsFullGrowth)
            {
                Game.World.SetBlock(this, (uint)GrowthStage.Dead << 2, x, y, z);
            }
        }

        public void LiquidChange(int x, int y, int z, Liquid liquid, LiquidLevel level)
        {
            if (liquid.Direction > 0 && level > LiquidLevel.Three) Destroy(x, y, z);
        }

        protected enum GrowthStage
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