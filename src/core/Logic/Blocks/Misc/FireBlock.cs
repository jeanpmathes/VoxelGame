// <copyright file="FireBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.Linq;
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
    // f: front
    // b: back
    // l: left
    // r: right
    // t: top
    public class FireBlock : Block, IFillable
    {
        private const int TickOffset = 150;
        private const int TickVariation = 25;

        private readonly List<BlockMesh> meshes = new(capacity: 32);

        internal FireBlock(string name, string namedId, string completeModel, string sideModel, string topModel) :
            base(
                name,
                namedId,
                BlockFlags.Replaceable,
                BoundingBox.Block,
                TargetBuffer.Complex)
        {
            BlockModel complete = BlockModel.Load(completeModel);

            BlockModel side = BlockModel.Load(sideModel);
            BlockModel top = BlockModel.Load(topModel);

            PrepareMeshes(complete, side, top);
        }

        /// <inheritdoc />
        public void LiquidChange(World world, Vector3i position, Liquid liquid, LiquidLevel level)
        {
            if (liquid != Liquid.None) Destroy(world, position);
        }

        private void PrepareMeshes(BlockModel complete, BlockModel side, BlockModel top)
        {
            (BlockModel north, BlockModel east, BlockModel south, BlockModel west) =
                side.CreateAllOrientations(rotateTopAndBottomTexture: true);

            for (uint data = 0b00_0000; data <= 0b01_1111; data++)
                if (data == 0)
                {
                    meshes.Add(complete.GetMesh());
                }
                else
                {
                    List<BlockModel> requiredModels = new(capacity: 5);

                    requiredModels.AddRange(
                        from blockSide in BlockSide.All.Sides()
                        where blockSide != BlockSide.Bottom && IsFlagSet(data, blockSide)
                        select GetSideModel(blockSide));

                    BlockMesh combinedMesh = BlockModel.GetCombinedMesh(requiredModels.ToArray());
                    meshes.Add(combinedMesh);
                }

            BlockModel GetSideModel(BlockSide blockSide)
            {
                return blockSide switch
                {
                    BlockSide.Front => south,
                    BlockSide.Back => north,
                    BlockSide.Left => west,
                    BlockSide.Right => east,
                    BlockSide.Top => top,
                    _ => throw new ArgumentOutOfRangeException(nameof(blockSide), blockSide, message: null)
                };
            }
        }

        /// <inheritdoc />
        protected override BoundingBox GetBoundingBox(uint data)
        {
            if (data == 0) return BoundingBox.Block;

            int count = BitHelper.CountSetBits(data);

            var parent = new BoundingBox();
            var children = new BoundingBox[count - 1];

            foreach (BlockSide side in BlockSide.All.Sides())
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

        /// <inheritdoc />
        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            BlockMesh mesh = meshes[(int) info.Data & 0b01_1111];

            return mesh.GetComplexMeshData(isAnimated: true);
        }

        /// <inheritdoc />
        public override bool CanPlace(World world, Vector3i position, PhysicsEntity? entity)
        {
            if (world.HasSolidGround(position)) return true;

            return GetData(world, position) != 0;
        }

        /// <inheritdoc />
        protected override void DoPlace(World world, Vector3i position, PhysicsEntity? entity)
        {
            world.SetBlock(this.AsInstance(world.HasSolidGround(position) ? 0 : GetData(world, position)), position);
            ScheduleTick(world, position, GetDelay(position));
        }

        private static uint GetData(World world, Vector3i position)
        {
            uint data = 0;

            foreach (BlockSide side in BlockSide.All.Sides())
            {
                if (side == BlockSide.Bottom) continue;

                if (world.IsSolid(side.Offset(position))) data |= GetFlag(side);
            }

            return data;
        }

        /// <inheritdoc />
        public override void BlockUpdate(World world, Vector3i position, uint data, BlockSide side)
        {
            if (side == BlockSide.Bottom)
            {
                if (data != 0) return;

                foreach (BlockSide sideToCheck in BlockSide.All.Sides())
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
                if (dataToSet != 0) world.SetBlock(this.AsInstance(dataToSet), position);
                else Destroy(world, position);
            }
        }

        /// <inheritdoc />
        protected override void ScheduledUpdate(World world, Vector3i position, uint data)
        {
            var canBurn = false;

            if (data == 0)
            {
                canBurn |= BurnAt(position.Below()); // Bottom.
                data = 0b01_1111;
            }

            foreach (BlockSide side in BlockSide.All.Sides())
            {
                if (side == BlockSide.Bottom) continue;

                if (IsFlagSet(data, side)) canBurn |= BurnAt(side.Offset(position));
            }

            if (!canBurn) Destroy(world, position);

            ScheduleTick(world, position, GetDelay(position));

            bool BurnAt(Vector3i burnPosition)
            {
                if (world.GetBlock(burnPosition)?.Block is IFlammable block)
                {
                    if (block.Burn(world, burnPosition, this))
                    {
                        if (world.GetBlock(burnPosition.Below())?.Block is IAshCoverable coverable)
                            coverable.CoverWithAsh(world, burnPosition.Below());

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