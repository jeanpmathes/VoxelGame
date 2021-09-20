// <copyright file="FireBlock.cs" company="VoxelGame">
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
    ///     An animated block that attaches to sides.
    ///     Data bit usage: <c>-fblrt</c>
    /// </summary>
    // f = front
    // b = back
    // l = left
    // r = right
    // t = top
    public class FireBlock : Block, IFillable
    {
        private const int TickOffset = 150;
        private const int TickVariation = 25;

        private readonly string texture;

        private float[][] attachedVertices = null!;
        private uint[] completeIndices = null!;
        private int[] completeTexIndices = null!;

        private float[] completeVertices = null!;
        private int texIndex;

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

        public void LiquidChange(World world, Vector3i position, Liquid liquid, LiquidLevel level)
        {
            if (liquid != Liquid.None) Destroy(world, position);
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
                // Front:
                new[]
                {
                    0f, 0f, 0.999f, 0f, 0f, 0f, 0f, 0f,
                    0f, 1f, 0.9f, 0f, 1f, 0f, 0f, 0f,
                    1f, 1f, 0.9f, 1f, 1f, 0f, 0f, 0f,
                    1f, 0f, 0.999f, 1f, 0f, 0f, 0f, 0f
                },
                // Back:
                new[]
                {
                    1f, 0f, 0.001f, 0f, 0f, 0f, 0f, 0f,
                    1f, 1f, 0.1f, 0f, 1f, 0f, 0f, 0f,
                    0f, 1f, 0.1f, 1f, 1f, 0f, 0f, 0f,
                    0f, 0f, 0.001f, 1f, 0f, 0f, 0f, 0f
                },
                // Left:
                new[]
                {
                    0.001f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
                    0.1f, 1f, 0f, 0f, 1f, 0f, 0f, 0f,
                    0.1f, 1f, 1f, 1f, 1f, 0f, 0f, 0f,
                    0.001f, 0f, 1f, 1f, 0f, 0f, 0f, 0f
                },
                // Right:
                new[]
                {
                    0.999f, 0f, 1f, 0f, 0f, 0f, 0f, 0f,
                    0.9f, 1f, 1f, 0f, 1f, 0f, 0f, 0f,
                    0.9f, 1f, 0f, 1f, 1f, 0f, 0f, 0f,
                    0.999f, 0f, 0f, 1f, 0f, 0f, 0f, 0f
                },

                // Bottom - dummy array.
                Array.Empty<float>(),

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
            if (data == 0) return BoundingBox.Block;

            int count = BitHelper.CountSetBits(data);

            var parent = new BoundingBox();
            var children = new BoundingBox[count - 1];

            for (var side = BlockSide.Front; side <= BlockSide.Top; side++)
            {
                if (side == BlockSide.Bottom) continue;

                if (IsFlagSet(data, side))
                {
                    Vector3 offset = side.Direction().ToVector3() * 0.4f;

                    var child = new BoundingBox(
                        new Vector3(x: 0.5f, y: 0.5f, z: 0.5f) + offset,
                        new Vector3(x: 0.5f, y: 0.5f, z: 0.5f) - offset.Absolute());

                    IncludeChild(child);
                }
            }

            return children.Length == 0 ? parent : new BoundingBox(parent.Center, parent.Extents, children);

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
                return BlockMeshData.Complex(
                    vertexCount: 24,
                    completeVertices,
                    completeTexIndices,
                    completeIndices,
                    isAnimated: true);

            int faceCount = BitHelper.CountSetBits(info.Data & 0b1_1111);

            if ((info.Data & 0b00_0001) != 0) faceCount++;

            float[] vertices = new float[faceCount * 32];

            var vi = 0;

            for (var side = BlockSide.Front; side <= BlockSide.Top; side++)
            {
                if (side == BlockSide.Bottom) continue;

                if (IsFlagSet(info.Data, side))
                {
                    var i = (int) side;

                    Array.Copy(attachedVertices[i], sourceIndex: 0, vertices, vi, attachedVertices[i].Length);
                    vi += attachedVertices[i].Length;
                }
            }

            int[] textureIndices = new int[faceCount * 4];

            for (var i = 0; i < textureIndices.Length; i++) textureIndices[i] = texIndex;

            uint[] indices = new uint[faceCount * 12];
            Array.Copy(completeIndices, indices, indices.Length);

            return BlockMeshData.Complex((uint) (faceCount * 4), vertices, textureIndices, indices, isAnimated: true);
        }

        internal override bool CanPlace(World world, Vector3i position, PhysicsEntity? entity)
        {
            if (world.HasSolidGround(position)) return true;

            return GetData(world, position) != 0;
        }

        protected override void DoPlace(World world, Vector3i position, PhysicsEntity? entity)
        {
            world.SetBlock(this, world.HasSolidGround(position) ? 0 : GetData(world, position), position);
            ScheduleTick(world, position, GetDelay(position));
        }

        private static uint GetData(World world, Vector3i position)
        {
            uint data = 0;

            for (var side = BlockSide.Front; side <= BlockSide.Top; side++)
            {
                if (side == BlockSide.Bottom) continue;

                if (world.IsSolid(side.Offset(position))) data |= GetFlag(side);
            }

            return data;
        }

        internal override void BlockUpdate(World world, Vector3i position, uint data, BlockSide side)
        {
            if (side == BlockSide.Bottom)
            {
                if (data != 0) return;

                for (var sideToCheck = BlockSide.Front; sideToCheck <= BlockSide.Top; sideToCheck++)
                {
                    if (sideToCheck == BlockSide.Bottom) continue;

                    if (world.IsSolid(sideToCheck.Offset(position))) data |= GetFlag(sideToCheck);
                }

                SetData(data);
            }
            else
            {
                if (!IsFlagSet(data, side) || world.IsSolid(side.Offset(position))) return;

                data ^= GetFlag(side);
                SetData(data);
            }

            void SetData(uint dataToSet)
            {
                if (dataToSet != 0) world.SetBlock(this, dataToSet, position);
                else Destroy(world, position);
            }
        }

        protected override void ScheduledUpdate(World world, Vector3i position, uint data)
        {
            var canBurn = false;

            if (data == 0)
            {
                canBurn |= BurnAt(position - Vector3i.UnitY); // Bottom.
                data = 0b01_1111;
            }

            for (var side = BlockSide.Front; side <= BlockSide.Top; side++)
            {
                if (side == BlockSide.Bottom) continue;

                if (IsFlagSet(data, side)) canBurn |= BurnAt(side.Offset(position));
            }

            if (!canBurn) Destroy(world, position);

            ScheduleTick(world, position, GetDelay(position));

            bool BurnAt(Vector3i burnPosition)
            {
                if (world.GetBlock(burnPosition, out _) is IFlammable block)
                {
                    if (block.Burn(world, burnPosition, this))
                    {
                        if (world.GetBlock(burnPosition - Vector3i.UnitY, out _) is IAshCoverable coverable)
                            coverable.CoverWithAsh(world, burnPosition - Vector3i.UnitY);

                        Place(world, burnPosition);
                    }

                    return true;
                }

                return false;
            }
        }

        private static int GetDelay(Vector3i position)
        {
            return TickOffset +
                   (BlockUtilities.GetPositionDependentNumber(position, TickVariation * 2) - TickVariation);
        }

        private static uint GetFlag(BlockSide side)
        {
            return side switch
            {
                BlockSide.Front => 0b01_0000,
                BlockSide.Back => 0b00_1000,
                BlockSide.Left => 0b00_0100,
                BlockSide.Right => 0b00_0010,
                BlockSide.Top => 0b00_0001,
                _ => 0b00_0000
            };
        }

        private static bool IsFlagSet(uint data, BlockSide side)
        {
            return (data & GetFlag(side)) != 0;
        }
    }
}