// <copyright file="DoubeCropBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// A block which grows on farmland and has multiple growth stages, of which some are two blocks tall.
    /// Data bit usage: <c>--hsss</c>
    /// </summary>
    // s = stage
    // h = height
    public class DoubeCropBlock : Block, IFlammable, IFillable
    {
        private protected float[] vertices = null!;

        private protected int[] stageTexIndicesLow = null!;
        private protected int[] stageTexIndicesTop = null!;

        private protected uint[] indices = null!;

        private protected string texture;
        private protected int dead, first, second, third;
        private protected (int low, int top) fourth, fifth, sixth, final;

        public DoubeCropBlock(string name, string namedId, string texture, int dead, int first, int second, int third, (int low, int top) fourth, (int low, int top) fifth, (int low, int top) sixth, (int low, int top) final) :
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
                TargetBuffer.Complex)
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

        protected override void Setup()
        {
            vertices = new float[]
            {
                //X----Y---Z---U---V---N---O---P
                0.25f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
                0.25f, 1f, 0f, 0f, 1f, 0f, 0f, 0f,
                0.25f, 1f, 1f, 1f, 1f, 0f, 0f, 0f,
                0.25f, 0f, 1f, 1f, 0f, 0f, 0f, 0f,

                0.75f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
                0.75f, 1f, 0f, 0f, 1f, 0f, 0f, 0f,
                0.75f, 1f, 1f, 1f, 1f, 0f, 0f, 0f,
                0.75f, 0f, 1f, 1f, 0f, 0f, 0f, 0f,

                0f, 0f, 0.25f, 0f, 0f, 0f, 0f, 0f,
                0f, 1f, 0.25f, 0f, 1f, 0f, 0f, 0f,
                1f, 1f, 0.25f, 1f, 1f, 0f, 0f, 0f,
                1f, 0f, 0.25f, 1f, 0f, 0f, 0f, 0f,

                0f, 0f, 0.75f, 0f, 0f, 0f, 0f, 0f,
                0f, 1f, 0.75f, 0f, 1f, 0f, 0f, 0f,
                1f, 1f, 0.75f, 1f, 1f, 0f, 0f, 0f,
                1f, 0f, 0.75f, 1f, 0f, 0f, 0f, 0f
            };

            int baseIndex = Game.BlockTextures.GetTextureIndex(texture);

            if (baseIndex == 0)
            {
                dead = first = second = third = 0;
                fourth = fifth = sixth = final = (0, 0);
            }

            stageTexIndicesLow = new int[]
            {
                baseIndex + dead,
                baseIndex + first,
                baseIndex + second,
                baseIndex + third,
                baseIndex + fourth.low,
                baseIndex + fifth.low,
                baseIndex + sixth.low,
                baseIndex + final.low,
            };

            stageTexIndicesTop = new int[]
            {
                0,
                0,
                0,
                0,
                baseIndex + fourth.top,
                baseIndex + fifth.top,
                baseIndex + sixth.top,
                baseIndex + final.top,
            };

            indices = new uint[]
            {
                0, 2, 1,
                0, 3, 2,
                0, 1, 2,
                0, 2, 3,

                4, 6, 5,
                4, 7, 6,
                4, 5, 6,
                4, 6, 7,

                8, 10, 9,
                8, 11, 10,
                8, 9, 10,
                8, 10, 11,

                12, 14, 13,
                12, 15, 14,
                12, 13, 14,
                12, 14, 15
            };
        }

        protected override BoundingBox GetBoundingBox(int x, int y, int z, uint data)
        {
            GrowthStage stage = (GrowthStage)(data & 0b00_0111);

            if (((data & 0b00_1000) == 0 && stage == GrowthStage.Initial) ||
                ((data & 0b00_1000) != 0 && (stage == GrowthStage.Fourth || stage == GrowthStage.Fifth)))
            {
                return new BoundingBox(new Vector3(0.5f, 0.25f, 0.5f) + new Vector3(x, y, z), new Vector3(0.5f, 0.25f, 0.5f));
            }
            else
            {
                return new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f) + new Vector3(x, y, z), new Vector3(0.5f, 0.5f, 0.5f));
            }
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            int[] textureIndices = new int[24];

            int tex = (int)(info.Data & 0b00_0111);

            if ((info.Data & 0b00_1000) == 0)
            {
                for (int i = 0; i < 24; i++)
                {
                    textureIndices[i] = stageTexIndicesLow[tex];
                }
            }
            else
            {
                for (int i = 0; i < 24; i++)
                {
                    textureIndices[i] = stageTexIndicesTop[tex];
                }
            }

            return new BlockMeshData(16, vertices, textureIndices, indices);
        }

        protected override bool Place(PhysicsEntity? entity, int x, int y, int z)
        {
            if (!(Game.World.GetBlock(x, y - 1, z, out _) is IPlantable))
            {
                return false;
            }

            Game.World.SetBlock(this, (uint)GrowthStage.Initial, x, y, z);

            return true;
        }

        protected override bool Destroy(PhysicsEntity? entity, int x, int y, int z, uint data)
        {
            Game.World.SetBlock(Block.Air, 0, x, y, z);

            if ((data & 0b00_0111) >= (int)GrowthStage.Fourth)
            {
                Game.World.SetBlock(Block.Air, 0, x, y + ((data & 0b00_1000) == 0 ? 1 : -1), z);
            }

            return true;
        }

        internal override void BlockUpdate(int x, int y, int z, uint data, BlockSide side)
        {
            // Check if this block is the lower part and if the ground supports plant growth.
            if (side == BlockSide.Bottom && (data & 0b00_1000) == 0 && !((Game.World.GetBlock(x, y - 1, z, out _) ?? Block.Air) is IPlantable))
            {
                Destroy(x, y, z);
            }
        }

        internal override void RandomUpdate(int x, int y, int z, uint data)
        {
            GrowthStage stage = (GrowthStage)(data & 0b00_0111);

            // If this block is the upper part, the random update is ignored.
            if ((data & 0b00_1000) != 0) return;

            if (Game.World.GetBlock(x, y - 1, z, out _) is IPlantable plantable)
            {
                if ((int)stage > 2 && !plantable.SupportsFullGrowth) return;

                if (stage != GrowthStage.Final && stage != GrowthStage.Dead)
                {
                    if (stage >= GrowthStage.Third)
                    {
                        Block? above = Game.World.GetBlock(x, y + 1, z, out _);

                        if (plantable.TryGrow(x, y - 1, z, Liquid.Water, LiquidLevel.One) && ((above?.IsReplaceable ?? false) || above == this))
                        {
                            Game.World.SetBlock(this, (uint)(stage + 1), x, y, z);
                            Game.World.SetBlock(this, (uint)(0b00_1000 | (int)stage + 1), x, y + 1, z);
                        }
                        else
                        {
                            Game.World.SetBlock(this, (uint)GrowthStage.Dead, x, y, z);
                            if (stage != GrowthStage.Third) Game.World.SetBlock(Block.Air, 0, x, y + 1, z);
                        }
                    }
                    else
                    {
                        Game.World.SetBlock(this, (uint)(stage + 1), x, y, z);
                    }
                }
            }
        }

        public void LiquidChange(int x, int y, int z, Liquid liquid, LiquidLevel level)
        {
            if (liquid.Direction > 0 && level > LiquidLevel.Four) Destroy(x, y, z);
        }

        protected enum GrowthStage
        {
            /// <summary>
            /// One Block tall.
            /// </summary>
            Dead,

            /// <summary>
            /// One Block tall.
            /// </summary>
            Initial,

            /// <summary>
            /// One Block tall.
            /// </summary>
            Second,

            /// <summary>
            /// One Block tall.
            /// </summary>
            Third,

            /// <summary>
            /// Two blocks tall.
            /// </summary>
            Fourth,

            /// <summary>
            /// Two blocks tall.
            /// </summary>
            Fifth,

            /// <summary>
            /// Two blocks tall.
            /// </summary>
            Sixth,

            /// <summary>
            /// Two blocks tall.
            /// </summary>
            Final,
        }
    }
}