// <copyright file="FireBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Visuals;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Entities;
using System;
using VoxelGame.Core.Utilities;
using OpenToolkit.Mathematics;
using VoxelGame.Core.Logic.Interfaces;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// An animated block that attaches to sides.
    /// Data bit usage: <c>-neswt</c>
    /// </summary>
    // n = north
    // e = east
    // s = south
    // w = west
    // t = top
    public class FireBlock : Block, IFillable
    {
        private const int TickOffset = 150;
        private const int TickVariation = 25;

        private float[] completeVertices = null!;
        private uint[] completeIndices = null!;
        private int[] completeTexIndices = null!;

        private float[][] attachedVertices = null!;
        private int texIndex;

        private readonly string texture;

        internal FireBlock(string name, string namedId, string texture) :
            base(
                name,
                namedId,
                isFull: false,
                isOpaque: false,
                renderFaceAtNonOpaques: true,
                isSolid: false,
                receiveCollisions: false,
                isTrigger: false,
                isReplaceable: true,
                isInteractable: false,
                BoundingBox.Block,
                TargetBuffer.Complex)
        {
            this.texture = texture;
        }

        protected override void Setup(ITextureIndexProvider indexProvider)
        {
            texIndex = indexProvider.GetTextureIndex(texture);

            completeVertices = new[]
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
                0.145f, 0f, 0.855f, 0f, 0f, 0f, 0f, 0f,
                0.145f, 1f, 0.855f, 0f, 1f, 0f, 0f, 0f,
                0.855f, 1f, 0.145f, 1f, 1f, 0f, 0f, 0f,
                0.855f, 0f, 0.145f, 1f, 0f, 0f, 0f, 0f,

                // Two sides: \
                0.145f, 0f, 0.145f, 0f, 0f, 0f, 0f, 0f,
                0.145f, 1f, 0.145f, 0f, 1f, 0f, 0f, 0f,
                0.855f, 1f, 0.855f, 1f, 1f, 0f, 0f, 0f,
                0.855f, 0f, 0.855f, 1f, 0f, 0f, 0f, 0f
            };

            completeTexIndices = new int[24];
            for (var i = 0; i < completeTexIndices.Length; i++) completeTexIndices[i] = texIndex;

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

            attachedVertices = new[]
            {
                // North:
                new[]
                {
                    1f, 0f, 0.001f, 0f, 0f, 0f, 0f, 0f,
                    1f, 1f, 0.1f, 0f, 1f, 0f, 0f, 0f,
                    0f, 1f, 0.1f, 1f, 1f, 0f, 0f, 0f,
                    0f, 0f, 0.001f, 1f, 0f, 0f, 0f, 0f
                },
                // East:
                new[]
                {
                    0.999f, 0f, 1f, 0f, 0f, 0f, 0f, 0f,
                    0.9f, 1f, 1f, 0f, 1f, 0f, 0f, 0f,
                    0.9f, 1f, 0f, 1f, 1f, 0f, 0f, 0f,
                    0.999f, 0f, 0f, 1f, 0f, 0f, 0f, 0f
                },
                // South:
                new[]
                {
                    0f, 0f, 0.999f, 0f, 0f, 0f, 0f, 0f,
                    0f, 1f, 0.9f, 0f, 1f, 0f, 0f, 0f,
                    1f, 1f, 0.9f, 1f, 1f, 0f, 0f, 0f,
                    1f, 0f, 0.999f, 1f, 0f, 0f, 0f, 0f
                },
                // West:
                new[]
                {
                    0.001f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
                    0.1f, 1f, 0f, 0f, 1f, 0f, 0f, 0f,
                    0.1f, 1f, 1f, 1f, 1f, 0f, 0f, 0f,
                    0.001f, 0f, 1f, 1f, 0f, 0f, 0f, 0f
                },
                // Top:
                new[]
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

        protected override BoundingBox GetBoundingBox(uint data)
        {
            if (data == 0)
            {
                return BoundingBox.Block;
            }

            int count = BitHelper.CountSetBits(data);

            var parent = new BoundingBox();
            var children = new BoundingBox[count - 1];

            if ((data & 0b01_0000) != 0)
            {
                var child = new BoundingBox(new Vector3(0.5f, 0.5f, 0.1f), new Vector3(0.5f, 0.5f, 0.1f));

                IncludeChild(child);
            }

            if ((data & 0b00_1000) != 0)
            {
                var child = new BoundingBox(new Vector3(0.9f, 0.5f, 0.5f), new Vector3(0.1f, 0.5f, 0.5f));

                IncludeChild(child);
            }

            if ((data & 0b00_0100) != 0)
            {
                var child = new BoundingBox(new Vector3(0.5f, 0.5f, 0.9f), new Vector3(0.5f, 0.5f, 0.1f));

                IncludeChild(child);
            }

            if ((data & 0b00_0010) != 0)
            {
                var child = new BoundingBox(new Vector3(0.1f, 0.5f, 0.5f), new Vector3(0.1f, 0.5f, 0.5f));

                IncludeChild(child);
            }

            if ((data & 0b00_0001) != 0)
            {
                var child = new BoundingBox(new Vector3(0.5f, 0.9f, 0.5f), new Vector3(0.5f, 0.1f, 0.5f));

                IncludeChild(child);
            }

            return (children.Length == 0) ? parent : new BoundingBox(parent.Center, parent.Extents, children);

            void IncludeChild(BoundingBox child)
            {
                count--;

                if (count == 0) parent = child;
                else children[count - 1] = child;
            }
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            if (info.Data == 0)
            {
                return new BlockMeshData(24, completeVertices, completeTexIndices, completeIndices, true);
            }

            int faceCount = BitHelper.CountSetBits(info.Data & 0b1_1111);

            if ((info.Data & 0b00_0001) != 0)
            {
                faceCount++;
            }

            float[] vertices = new float[faceCount * 32];

            var vi = 0;

            for (var i = 0; i < 5; i++)
            {
                if ((info.Data & (0b1_0000 >> i)) != 0)
                {
                    Array.Copy(attachedVertices[i], 0, vertices, vi, attachedVertices[i].Length);
                    vi += attachedVertices[i].Length;
                }
            }

            int[] textureIndices = new int[faceCount * 4];

            for (var i = 0; i < textureIndices.Length; i++)
            {
                textureIndices[i] = texIndex;
            }

            uint[] indices = new uint[faceCount * 12];
            Array.Copy(completeIndices, indices, indices.Length);

            return new BlockMeshData((uint)(faceCount * 4), vertices, textureIndices, indices, true);
        }

        internal override bool CanPlace(World world, int x, int y, int z, PhysicsEntity? entity)
        {
            if (world.HasSolidGround(x, y, z))
            {
                return true;
            }
            else
            {
                return GetData(world, x, y, z) != 0;
            }
        }

        protected override void DoPlace(World world, int x, int y, int z, PhysicsEntity? entity)
        {
            if (world.HasSolidGround(x, y, z))
            {
                world.SetBlock(this, 0, x, y, z);
                ScheduleTick(world, x, y, z, GetDelay(x, y, z));
            }
            else
            {
                world.SetBlock(this, GetData(world, x, y, z), x, y, z);
                ScheduleTick(world, x, y, z, GetDelay(x, y, z));
            }
        }

        private static uint GetData(World world, int x, int y, int z)
        {
            uint data = 0;

            if (world.GetBlock(x, y, z - 1, out _)?.IsSolidAndFull == true) data |= 0b01_0000; // North.
            if (world.GetBlock(x + 1, y, z, out _)?.IsSolidAndFull == true) data |= 0b00_1000; // East.
            if (world.GetBlock(x, y, z + 1, out _)?.IsSolidAndFull == true) data |= 0b00_0100; // South.
            if (world.GetBlock(x - 1, y, z, out _)?.IsSolidAndFull == true) data |= 0b00_0010; // West.
            if (world.GetBlock(x, y + 1, z, out _)?.IsSolidAndFull == true) data |= 0b00_0001; // Top.

            return data;
        }

        internal override void BlockUpdate(World world, int x, int y, int z, uint data, BlockSide side)
        {
            switch (side)
            {
                case BlockSide.Back:

                    CheckNeighbor(x, y, z - 1, 0b01_0000);
                    break;

                case BlockSide.Right:

                    CheckNeighbor(x + 1, y, z, 0b00_1000);
                    break;

                case BlockSide.Front:

                    CheckNeighbor(x, y, z + 1, 0b00_0100);
                    break;

                case BlockSide.Left:

                    CheckNeighbor(x - 1, y, z, 0b00_0010);
                    break;

                case BlockSide.Top:

                    CheckNeighbor(x, y + 1, z, 0b00_0001);
                    break;

                case BlockSide.Bottom:

                    if (data != 0)
                    {
                        break;
                    }

                    data |= AddNeighbor(x, y, z - 1, 0b01_0000); // North.
                    data |= AddNeighbor(x + 1, y, z, 0b00_1000); // East.
                    data |= AddNeighbor(x, y, z + 1, 0b00_0100); // South.
                    data |= AddNeighbor(x - 1, y, z, 0b00_0010); // West.
                    data |= AddNeighbor(x, y + 1, z, 0b00_0001); // Top.

                    SetData(data);

                    break;
            }

            void CheckNeighbor(int nx, int ny, int nz, uint mask)
            {
                if ((data & mask) == 0 || world.GetBlock(nx, ny, nz, out _)?.IsSolidAndFull == true) return;

                data ^= mask;
                SetData(data);
            }

            uint AddNeighbor(int nx, int ny, int nz, uint mask)
            {
                return (world.GetBlock(nx, ny, nz, out _)?.IsSolidAndFull == true) ? mask : 0;
            }

            void SetData(uint dataToSet)
            {
                if (dataToSet != 0)
                {
                    world.SetBlock(this, dataToSet, x, y, z);
                }
                else
                {
                    Destroy(world, x, y, z);
                }
            }
        }

        protected override void ScheduledUpdate(World world, int x, int y, int z, uint data)
        {
            var canBurn = false;

            if (data == 0)
            {
                canBurn |= BurnAt(x, y - 1, z); // Bottom.

                data = 0b1_1111;
            }

            if ((data & 0b01_0000) != 0) canBurn |= BurnAt(x, y, z - 1); // North.
            if ((data & 0b00_1000) != 0) canBurn |= BurnAt(x + 1, y, z); // East.
            if ((data & 0b00_0100) != 0) canBurn |= BurnAt(x, y, z + 1); // South.
            if ((data & 0b00_0010) != 0) canBurn |= BurnAt(x - 1, y, z); // West.
            if ((data & 0b00_0001) != 0) canBurn |= BurnAt(x, y + 1, z); // Top.

            if (!canBurn)
            {
                Destroy(world, x, y, z);
            }

            ScheduleTick(world, x, y, z, GetDelay(x, y, z));

            bool BurnAt(int nx, int ny, int nz)
            {
                if (world.GetBlock(nx, ny, nz, out _) is IFlammable block)
                {
                    if (block.Burn(world, nx, ny, nz, this))
                    {
                        if (world.GetBlock(nx, ny - 1, nz, out _) is IAshCoverable coverable)
                        {
                            coverable.CoverWithAsh(world, nx, ny - 1, nz);
                        }

                        Place(world, nx, ny, nz);
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void LiquidChange(World world, int x, int y, int z, Liquid liquid, LiquidLevel level)
        {
            if (liquid != Liquid.None) Destroy(world, x, y, z);
        }

        private static int GetDelay(int x, int y, int z)
        {
            return TickOffset + (BlockUtilities.GetPositionDependentNumber(x, y, z, TickVariation * 2) - TickVariation);
        }
    }
}