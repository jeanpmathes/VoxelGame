﻿// <copyright file="PipeBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using OpenToolkit.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    ///     A block that connects to other pipes and allows water flow.
    ///     Data bit usage: <c>fblrdt</c>
    /// </summary>
    // f: front
    // b: back
    // l: left
    // r: right
    // d: bottom
    // t: top
    internal class PipeBlock<TConnect> : Block, IFillable where TConnect : IPipeConnectable
    {
        private readonly float diameter;
        private readonly List<BlockMesh> meshes = new(capacity: 64);

        internal PipeBlock(string name, string namedId, float diameter, string centerModel, string connectorModel,
            string surfaceModel) :
            base(
                name,
                namedId,
                BlockFlags.Solid,
                new BoundingBox(new Vector3(x: 0.5f, y: 0.5f, z: 0.5f), new Vector3(diameter, diameter, diameter)),
                TargetBuffer.Complex)
        {
            this.diameter = diameter;

            BlockModel center = BlockModel.Load(centerModel);

            BlockModel frontConnector = BlockModel.Load(connectorModel);
            BlockModel frontSurface = BlockModel.Load(surfaceModel);

            (BlockModel front, BlockModel back, BlockModel left, BlockModel right, BlockModel bottom, BlockModel top)
                connectors = frontConnector.CreateAllSides();

            (BlockModel front, BlockModel back, BlockModel left, BlockModel right, BlockModel bottom, BlockModel top)
                surfaces = frontSurface.CreateAllSides();

            center.Lock();
            connectors.Lock();
            surfaces.Lock();

            for (uint data = 0b00_0000; data <= 0b11_1111; data++)
            {
                BlockMesh mesh = BlockModel.GetCombinedMesh(
                    center,
                    BlockSide.Front.IsSet(data) ? connectors.front : surfaces.front,
                    BlockSide.Back.IsSet(data) ? connectors.back : surfaces.back,
                    BlockSide.Left.IsSet(data) ? connectors.left : surfaces.left,
                    BlockSide.Right.IsSet(data) ? connectors.right : surfaces.right,
                    BlockSide.Bottom.IsSet(data) ? connectors.bottom : surfaces.bottom,
                    BlockSide.Top.IsSet(data) ? connectors.top : surfaces.top);

                meshes.Add(mesh);
            }
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

        protected override BoundingBox GetBoundingBox(uint data)
        {
            List<BoundingBox> connectors = new(BitHelper.CountSetBits(data));

            float connectorWidth = (0.5f - diameter) / 2f;

            foreach (BlockSide side in BlockSide.All.Sides())
            {
                if (!side.IsSet(data)) continue;

                var direction = side.Direction().ToVector3();

                connectors.Add(
                    new BoundingBox(
                        (0.5f, 0.5f, 0.5f) + direction * (0.5f - connectorWidth),
                        (diameter, diameter, diameter) + direction.Absolute() * (connectorWidth - diameter)));
            }

            return new BoundingBox(
                new Vector3(x: 0.5f, y: 0.5f, z: 0.5f),
                new Vector3(diameter, diameter, diameter),
                connectors.ToArray());
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            BlockMesh mesh = meshes[(int) info.Data];

            return mesh.GetComplexMeshData();
        }

        protected override void DoPlace(World world, Vector3i position, PhysicsEntity? entity)
        {
            uint data = GetConnectionData(world, position);

            OpenOpposingSide(ref data);

            world.SetBlock(this, data, position);
        }

        internal override void BlockUpdate(World world, Vector3i position, uint data, BlockSide side)
        {
            uint updatedData = GetConnectionData(world, position);
            OpenOpposingSide(ref updatedData);

            if (updatedData != data) world.SetBlock(this, updatedData, position);
        }

        private uint GetConnectionData(World world, Vector3i position)
        {
            uint data = 0;

            foreach (BlockSide side in BlockSide.All.Sides())
            {
                Vector3i otherPosition = side.Offset(position);
                Block? otherBlock = world.GetBlock(otherPosition, out _);

                if (otherBlock == this || otherBlock is TConnect connectable &&
                    connectable.IsConnectable(world, side, otherPosition)) data |= side.ToFlag();
            }

            return data;
        }

        private static void OpenOpposingSide(ref uint data)
        {
            if (BitHelper.CountSetBits(data) != 1) return;

            if ((data & 0b11_0000) != 0) data = 0b11_0000;

            if ((data & 0b00_1100) != 0) data = 0b00_1100;

            if ((data & 0b00_0011) != 0) data = 0b00_0011;
        }

        private static bool IsSideOpen(World world, Vector3i position, BlockSide side)
        {
            world.GetBlock(position, out uint data);

            return side.IsSet(data);
        }
    }
}