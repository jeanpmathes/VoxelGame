﻿// <copyright file="SteelPipeValveBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using System.Diagnostics;
using OpenToolkit.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    ///     A block that only connects to steel pipes at specific sides and can be closed.
    ///     Data bit usage: <c>---oaa</c>
    /// </summary>
    // aa = axis
    // o = open
    internal class SteelPipeValveBlock : Block, IFillable, IIndustrialPipeConnectable
    {
        private readonly List<BlockMesh?> meshes = new(capacity: 8);

        internal SteelPipeValveBlock(string name, string namedId, float diameter, string openModel,
            string closedModel) :
            base(
                name,
                namedId,
                isFull: false,
                isOpaque: false,
                renderFaceAtNonOpaques: true,
                isSolid: true,
                receiveCollisions: false,
                isTrigger: false,
                isReplaceable: false,
                isInteractable: true,
                new BoundingBox(new Vector3(x: 0.5f, y: 0.5f, z: 0.5f), new Vector3(diameter, diameter, z: 0.5f)),
                TargetBuffer.Complex)
        {
            (BlockModel openX, BlockModel openY, BlockModel openZ) = BlockModel.Load(openModel).CreateAllAxis();
            (BlockModel closedX, BlockModel closedY, BlockModel closedZ) = BlockModel.Load(closedModel).CreateAllAxis();

            meshes.Add(openX.GetMesh());
            meshes.Add(openY.GetMesh());
            meshes.Add(openZ.GetMesh());
            meshes.Add(item: null);

            meshes.Add(closedX.GetMesh());
            meshes.Add(closedY.GetMesh());
            meshes.Add(closedZ.GetMesh());
            meshes.Add(item: null);
        }

        public bool RenderLiquid => false;

        public bool AllowInflow(World world, Vector3i position, BlockSide side, Liquid liquid)
        {
            return IsSideOpen(world, position, side);
        }

        public bool AllowOutflow(World world, Vector3i position, BlockSide side)
        {
            return IsSideOpen(world, position, side);
        }

        public bool IsConnectable(World world, BlockSide side, Vector3i position)
        {
            world.GetBlock(position, out uint data);

            return side.Axis() == (Axis) (data & 0b00_0011);
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            BlockMesh? mesh = meshes[(int) info.Data & 0b00_0111];
            Debug.Assert(mesh != null);

            return mesh.GetComplexMeshData();
        }

        protected override void DoPlace(World world, Vector3i position, PhysicsEntity? entity)
        {
            world.SetBlock(this, (uint) (entity?.TargetSide ?? BlockSide.Front).Axis(), position);
        }

        protected override void EntityInteract(PhysicsEntity entity, Vector3i position, uint data)
        {
            entity.World.SetBlock(this, data ^ 0b00_0100, position);
        }

        private static bool IsSideOpen(World world, Vector3i position, BlockSide side)
        {
            world.GetBlock(position, out uint data);

            if ((data & 0b00_0100) != 0) return false;

            return side.Axis() == (Axis) (data & 0b00_0011);
        }
    }
}