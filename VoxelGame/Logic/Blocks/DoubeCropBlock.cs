// <copyright file="DoubeCropBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Rendering;
using VoxelGame.Physics;
using System;
using VoxelGame.Entities;
using VoxelGame.Logic.Interfaces;
using OpenToolkit.Mathematics;

namespace VoxelGame.Logic.Blocks
{
    /// <summary>
    /// A block which grows on farmland and has multiple growth stages, of which some are two blocks tall.
    /// Data bit usage: <c>-hsss</c>
    /// </summary>
    // s = stage
    // h = height
    public class DoubeCropBlock : Block
    {
        private protected int[] stageTexIndicesLow = null!;
        private protected int[] stageTexIndicesTop = null!;

        private protected float[] vertices = new float[]
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

        private protected uint[] indices =
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

        public DoubeCropBlock(string name, string texture, int dead, int first, int second, int third, (int low, int top) fourth, (int low, int top) fifth, (int low, int top) sixth, (int low, int top) final) :
            base(
                name,
                isFull: false,
                isOpaque: false,
                renderFaceAtNonOpaques: true,
                isSolid: false,
                recieveCollisions: false,
                isTrigger: false,
                isReplaceable: false,
                BoundingBox.Block,
                TargetBuffer.Complex)
        {
#pragma warning disable CA2214 // Do not call overridable methods in constructors
            this.Setup(texture, dead, first, second, third, fourth, fifth, sixth, final);
#pragma warning restore CA2214 // Do not call overridable methods in constructors
        }

        protected virtual void Setup(string texture, int dead, int first, int second, int third, (int low, int top) fourth, (int low, int top) fifth, (int low, int top) sixth, (int low, int top) final)
        {
            int baseIndex = Game.BlockTextureArray.GetTextureIndex(texture);

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
        }

        public override BoundingBox GetBoundingBox(int x, int y, int z)
        {
            if (Game.World.GetBlock(x, y, z, out byte data) != this)
            {
                return new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f) + new Vector3(x, y, z), new Vector3(0.5f, 0.5f, 0.5f));
            }

            GrowthStage stage = (GrowthStage)(data & 0b0_0111);

            if (((data & 0b0_1000) == 0 && stage == GrowthStage.Initial) ||
                ((data & 0b0_1000) != 0 && (stage == GrowthStage.Fourth || stage == GrowthStage.Fifth)))
            {
                return new BoundingBox(new Vector3(0.5f, 0.25f, 0.5f) + new Vector3(x, y, z), new Vector3(0.5f, 0.25f, 0.5f));
            }
            else
            {
                return new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f) + new Vector3(x, y, z), new Vector3(0.5f, 0.5f, 0.5f));
            }
        }

        public override bool Place(int x, int y, int z, PhysicsEntity? entity)
        {
            if (Game.World.GetBlock(x, y, z, out _)?.IsReplaceable != true || !(Game.World.GetBlock(x, y - 1, z, out _) is IPlantable))
            {
                return false;
            }

            Game.World.SetBlock(this, (byte)GrowthStage.Initial, x, y, z);

            return true;
        }

        public override bool Destroy(int x, int y, int z, PhysicsEntity? entity)
        {
            if (Game.World.GetBlock(x, y, z, out byte data) != this)
            {
                return false;
            }

            Game.World.SetBlock(Block.AIR, 0, x, y, z);

            if ((data & 0b0_0111) >= (int)GrowthStage.Fourth)
            {
                Game.World.SetBlock(Block.AIR, 0, x, y + ((data & 0b0_1000) == 0 ? 1 : -1), z);
            }

            return true;
        }

        public override uint GetMesh(BlockSide side, byte data, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint)
        {
            vertices = this.vertices;
            textureIndices = new int[24];

            int tex = data & 0b0_0111;

            if ((data & 0b0_1000) == 0)
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

            indices = this.indices;
            tint = TintColor.None;

            return 16;
        }

        public override void BlockUpdate(int x, int y, int z, byte data)
        {
            // Check if this block is the lower part and if the ground supports plant growth.
            if ((data & 0b0_1000) == 0 && !((Game.World.GetBlock(x, y - 1, z, out _) ?? Block.AIR) is IPlantable))
            {
                Destroy(x, y, z, null);
            }
        }

        public override void RandomUpdate(int x, int y, int z, byte data)
        {
            GrowthStage stage = (GrowthStage)(data & 0b0_0111);

            // If this block is the upper part or the block cannot grow more on this type of ground, the random update is ignored.
            if ((data & 0b0_1000) != 0 || ((int)stage > 2 && Game.World.GetBlock(x, y - 1, z, out _) != Block.FARMLAND))
            {
                return;
            }

            if (stage != GrowthStage.Final && stage != GrowthStage.Dead)
            {
                if (stage >= GrowthStage.Third)
                {
                    Block? above = Game.World.GetBlock(x, y + 1, z, out _);

                    if ((above?.IsReplaceable ?? false) || above == this)
                    {
                        Game.World.SetBlock(this, (byte)(stage + 1), x, y, z);
                        Game.World.SetBlock(this, (byte)(0b0_1000 | (int)stage + 1), x, y + 1, z);
                    }
                }
                else
                {
                    Game.World.SetBlock(this, (byte)(stage + 1), x, y, z);
                }
            }
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
