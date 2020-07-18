// <copyright file="FireBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Visuals;
using VoxelGame.Physics;
using VoxelGame.Entities;
using System;

namespace VoxelGame.Logic.Blocks
{
    /// <summary>
    /// An animated block that attaches to sides.
    /// Data bit usage: <c>neswt</c>
    /// </summary>
    // n = north
    // e = east
    // s = south
    // w = west
    // t = top
    public class FireBlock : Block
    {
        private protected float[] completeVertices = null!;
        private protected uint[] completeIndices = null!;
        private protected int[] completeTexIndices = null!;

        private protected float[][] attachedVertices = null!;
        private protected int texIndex;

        private protected string texture;

        public FireBlock(string name, string namedId, string texture) :
            base(
                name,
                namedId,
                isFull: false,
                isOpaque: true,
                renderFaceAtNonOpaques: true,
                isSolid: false,
                recieveCollisions: false,
                isTrigger: false,
                isReplaceable: true,
                isInteractable: false,
                BoundingBox.Block,
                TargetBuffer.Complex)
        {
            this.texture = texture;
        }

        protected override void Setup()
        {
            texIndex = Game.BlockTextureArray.GetTextureIndex(texture);

            completeVertices = new float[]
            {
                // North:
                1f, 0f, 0.001f, 0f, 0f, 0f, 0f, 0f,
                1f, 1f, 0.001f, 0f, 1f, 0f, 0f, 0f,
                0f, 1f, 0.001f, 1f, 1f, 0f, 0f, 0f,
                0f, 0f, 0.001f, 1f, 0f, 0f, 0f, 0f,

                // East:
                0.999f, 0f, 1f, 0f, 0f, 0f, 0f, 0f,
                0.999f, 1f, 1f, 0f, 1f, 0f, 0f, 0f,
                0.999f, 1f, 0f, 1f, 1f, 0f, 0f, 0f,
                0.999f, 0f, 0f, 1f, 0f, 0f, 0f, 0f,

                // South:
                0f, 0f, 0.999f, 0f, 0f, 0f, 0f, 0f,
                0f, 1f, 0.999f, 0f, 1f, 0f, 0f, 0f,
                1f, 1f, 0.999f, 1f, 1f, 0f, 0f, 0f,
                1f, 0f, 0.999f, 1f, 0f, 0f, 0f, 0f,

                // West:
                0.001f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
                0.001f, 1f, 0f, 0f, 1f, 0f, 0f, 0f,
                0.001f, 1f, 1f, 1f, 1f, 0f, 0f, 0f,
                0.001f, 0f, 1f, 1f, 0f, 0f, 0f, 0f,

                // Two sides: /
                0.001f, 0f, 0.999f, 0f, 0f, 0f, 0f, 0f,
                0.001f, 1f, 0.999f, 0f, 1f, 0f, 0f, 0f,
                0.999f, 1f, 0.001f, 1f, 1f, 0f, 0f, 0f,
                0.999f, 0f, 0.001f, 1f, 0f, 0f, 0f, 0f,

                // Two sides: \
                0.001f, 0f, 0.001f, 0f, 0f, 0f, 0f, 0f,
                0.001f, 1f, 0.001f, 0f, 1f, 0f, 0f, 0f,
                0.999f, 1f, 0.999f, 1f, 1f, 0f, 0f, 0f,
                0.999f, 0f, 0.999f, 1f, 0f, 0f, 0f, 0f
            };

            completeTexIndices = new int[24];
            for (int i = 0; i < completeTexIndices.Length; i++) completeTexIndices[i] = texIndex;

            completeIndices = new uint[]
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
                12, 14, 15,

                16, 18, 17,
                16, 19, 18,
                16, 17, 18,
                16, 18, 19,

                20, 22, 21,
                20, 23, 22,
                20, 21, 22,
                20, 22, 23
            };

            attachedVertices = new float[][]
            {
                // North:
                new float[]
                {
                    1f, 0f, 0.001f, 0f, 0f, 0f, 0f, 0f,
                    1f, 1f, 0.1f, 0f, 1f, 0f, 0f, 0f,
                    0f, 1f, 0.1f, 1f, 1f, 0f, 0f, 0f,
                    0f, 0f, 0.001f, 1f, 0f, 0f, 0f, 0f
                },
                // East:
                new float[]
                {
                    0.999f, 0f, 1f, 0f, 0f, 0f, 0f, 0f,
                    0.9f, 1f, 1f, 0f, 1f, 0f, 0f, 0f,
                    0.9f, 1f, 0f, 1f, 1f, 0f, 0f, 0f,
                    0.999f, 0f, 0f, 1f, 0f, 0f, 0f, 0f
                },
                // South:
                new float[]
                {
                    0f, 0f, 0.999f, 0f, 0f, 0f, 0f, 0f,
                    0f, 1f, 0.9f, 0f, 1f, 0f, 0f, 0f,
                    1f, 1f, 0.9f, 1f, 1f, 0f, 0f, 0f,
                    1f, 0f, 0.999f, 1f, 0f, 0f, 0f, 0f
                },
                // West:
                new float[]
                {
                    0.001f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
                    0.1f, 1f, 0f, 0f, 1f, 0f, 0f, 0f,
                    0.1f, 1f, 1f, 1f, 1f, 0f, 0f, 0f,
                    0.001f, 0f, 1f, 1f, 0f, 0f, 0f, 0f
                },
                // Top:
                new float[]
                {
                    0f, 0.999f, 1f, 0f, 0f, 0f, 0f, 0f,
                    0f, 0.8f, 0f, 0f, 1f, 0f, 0f, 0f,
                    1f, 0.8f, 0f, 1f, 1f, 0f, 0f, 0f,
                    1f, 0.999f, 1f, 1f, 0f, 0f, 0f, 0f,

                    0f, 0.8f, 1f, 0f, 1f, 0f, 0f, 0f,
                    0f, 0.999f, 0f, 0f, 0f, 0f, 0f, 0f,
                    1f, 0.999f, 0f, 1f, 0f, 0f, 0f, 0f,
                    1f, 0.8f, 1f, 1f, 1f, 0f, 0f, 0f
                }
            };
        }

        public override uint GetMesh(BlockSide side, byte data, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint, out bool isAnimated)
        {
            if (data == 0)
            {
                vertices = completeVertices;
                textureIndices = completeTexIndices;
                indices = completeIndices;

                tint = TintColor.None;
                isAnimated = true;

                return 24;
            }
            else
            {
                int faceCount = 0;

                if ((data & 0b1_0000) != 0)
                {
                    faceCount++;
                }

                if ((data & 0b0_1000) != 0)
                {
                    faceCount++;
                }

                if ((data & 0b0_0100) != 0)
                {
                    faceCount++;
                }

                if ((data & 0b0_0010) != 0)
                {
                    faceCount++;
                }

                if ((data & 0b0_0001) != 0)
                {
                    faceCount += 2;
                }

                vertices = new float[faceCount * 32];

                int vi = 0;

                for (int i = 0; i < 5; i++)
                {
                    if ((data & (0b1_0000 >> i)) != 0)
                    {
                        Array.Copy(attachedVertices[i], 0, vertices, vi, attachedVertices[i].Length);
                        vi += attachedVertices[i].Length;
                    }
                }

                textureIndices = new int[faceCount * 4];

                for (int i = 0; i < textureIndices.Length; i++)
                {
                    textureIndices[i] = texIndex;
                }

                indices = new uint[faceCount * 12];

                Array.Copy(completeIndices, indices, indices.Length);

                tint = TintColor.None;
                isAnimated = true;

                return (uint)(faceCount * 4);
            }
        }

        protected override bool Place(PhysicsEntity? entity, int x, int y, int z)
        {
            if (Game.World.GetBlock(x, y - 1, z, out _)?.IsSolidAndFull == true)
            {
                Game.World.SetBlock(this, 0, x, y, z);

                return true;
            }
            else
            {
                byte data = 0;

                if (Game.World.GetBlock(x, y, z - 1, out _)?.IsSolidAndFull == true) data |= 0b1_0000; // North.
                if (Game.World.GetBlock(x + 1, y, z, out _)?.IsSolidAndFull == true) data |= 0b0_1000; // East.
                if (Game.World.GetBlock(x, y, z + 1, out _)?.IsSolidAndFull == true) data |= 0b0_0100; // South.
                if (Game.World.GetBlock(x - 1, y, z, out _)?.IsSolidAndFull == true) data |= 0b0_0010; // West.
                if (Game.World.GetBlock(x, y + 1, z, out _)?.IsSolidAndFull == true) data |= 0b0_0001; // Top.

                if (data != 0)
                {
                    Game.World.SetBlock(this, data, x, y, z);

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        internal override void BlockUpdate(int x, int y, int z, byte data, BlockSide side)
        {
            switch (side)
            {
                case BlockSide.Back:

                    if ((data & 0b1_0000) != 0 && Game.World.GetBlock(x, y, z - 1, out _)?.IsSolidAndFull != true)
                    {
                        data ^= 0b1_0000;
                        SetData();
                    }

                    break;

                case BlockSide.Right:

                    if ((data & 0b0_1000) != 0 && Game.World.GetBlock(x + 1, y, z, out _)?.IsSolidAndFull != true)
                    {
                        data ^= 0b0_1000;
                        SetData();
                    }

                    break;

                case BlockSide.Front:

                    if ((data & 0b0_0100) != 0 && Game.World.GetBlock(x, y, z + 1, out _)?.IsSolidAndFull != true)
                    {
                        data ^= 0b0_0100;
                        SetData();
                    }

                    break;

                case BlockSide.Left:

                    if ((data & 0b0_0010) != 0 && Game.World.GetBlock(x - 1, y, z, out _)?.IsSolidAndFull != true)
                    {
                        data ^= 0b0_0010;
                        SetData();
                    }

                    break;

                case BlockSide.Top:

                    if ((data & 0b0_0001) != 0 && Game.World.GetBlock(x, y + 1, z, out _)?.IsSolidAndFull != true)
                    {
                        data ^= 0b0_0001;
                        SetData();
                    }

                    break;

                case BlockSide.Bottom:

                    if (data == 0)
                    {
                        if (Game.World.GetBlock(x, y, z - 1, out _)?.IsSolidAndFull == true) data |= 0b1_0000; // North.
                        if (Game.World.GetBlock(x + 1, y, z, out _)?.IsSolidAndFull == true) data |= 0b0_1000; // East.
                        if (Game.World.GetBlock(x, y, z + 1, out _)?.IsSolidAndFull == true) data |= 0b0_0100; // South.
                        if (Game.World.GetBlock(x - 1, y, z, out _)?.IsSolidAndFull == true) data |= 0b0_0010; // West.
                        if (Game.World.GetBlock(x, y + 1, z, out _)?.IsSolidAndFull == true) data |= 0b0_0001; // Top.

                        SetData();
                    }

                    break;
            }

            void SetData()
            {
                if (data != 0)
                {
                    Game.World.SetBlock(this, data, x, y, z);
                }
                else
                {
                    Destroy(x, y, z, null);
                }
            }
        }
    }
}