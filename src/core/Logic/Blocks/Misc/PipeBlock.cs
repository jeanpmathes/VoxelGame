// <copyright file="PipeBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;
using System.Collections.Generic;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// A block that connects to other pipes and allows water flow.
    /// Data bit usage: <c>fblrdt</c>
    /// </summary>
    // f: front
    // b: back
    // l: left
    // r: right
    // d: bottom
    // t: top
    internal class PipeBlock<TConnect> : Block, IFillable where TConnect : IPipeConnectable
    {
        private readonly BlockModel center;
        private readonly (BlockModel front, BlockModel back, BlockModel left, BlockModel right, BlockModel bottom, BlockModel top) connector;
        private readonly (BlockModel front, BlockModel back, BlockModel left, BlockModel right, BlockModel bottom, BlockModel top) surface;

        private readonly float diameter;

        public bool RenderLiquid => false;

        internal PipeBlock(string name, string namedId, float diameter, string centerModel, string connectorModel, string surfaceModel) :
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
                isInteractable: false,
                boundingBox: new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(diameter, diameter, diameter)),
                targetBuffer: TargetBuffer.Complex)
        {
            this.diameter = diameter;

            center = BlockModel.Load(centerModel);

            BlockModel frontConnector = BlockModel.Load(connectorModel);
            BlockModel frontSurface = BlockModel.Load(surfaceModel);

            connector = frontConnector.CreateAllSides();
            surface = frontSurface.CreateAllSides();

            center.Lock();
            connector.Lock();
            surface.Lock();
        }

        protected override BoundingBox GetBoundingBox(uint data)
        {
            List<BoundingBox> connectors = new List<BoundingBox>(BitHelper.CountSetBits(data));

            float connectorWidth = (0.5f - diameter) / 2f;

            if ((data & 0b10_0000) != 0) connectors.Add(new BoundingBox(new Vector3(0.5f, 0.5f, 1f - connectorWidth), new Vector3(diameter, diameter, connectorWidth)));
            if ((data & 0b01_0000) != 0) connectors.Add(new BoundingBox(new Vector3(0.5f, 0.5f, connectorWidth), new Vector3(diameter, diameter, connectorWidth)));
            if ((data & 0b00_1000) != 0) connectors.Add(new BoundingBox(new Vector3(connectorWidth, 0.5f, 0.5f), new Vector3(connectorWidth, diameter, diameter)));
            if ((data & 0b00_0100) != 0) connectors.Add(new BoundingBox(new Vector3(1f - connectorWidth, 0.5f, 0.5f), new Vector3(connectorWidth, diameter, diameter)));
            if ((data & 0b00_0010) != 0) connectors.Add(new BoundingBox(new Vector3(0.5f, connectorWidth, 0.5f), new Vector3(diameter, connectorWidth, diameter)));
            if ((data & 0b00_0001) != 0) connectors.Add(new BoundingBox(new Vector3(0.5f, 1f - connectorWidth, 0.5f), new Vector3(diameter, connectorWidth, diameter)));

            return new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(diameter, diameter, diameter), connectors.ToArray());
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            (float[] vertices, int[] textureIndices, uint[] indices) = BlockModel.CombineData(out uint vertexCount, center,
                (info.Data & 0b10_0000) == 0 ? surface.front : connector.front,
                (info.Data & 0b01_0000) == 0 ? surface.back : connector.back,
                (info.Data & 0b00_1000) == 0 ? surface.left : connector.left,
                (info.Data & 0b00_0100) == 0 ? surface.right : connector.right,
                (info.Data & 0b00_0010) == 0 ? surface.bottom : connector.bottom,
                (info.Data & 0b00_0001) == 0 ? surface.top : connector.top);

            return BlockMeshData.Complex(vertexCount, vertices, textureIndices, indices);
        }

        protected override void DoPlace(World world, int x, int y, int z, PhysicsEntity? entity)
        {
            uint data = GetConnectionData(world, x, y, z);

            OpenOpposingSide(ref data);

            world.SetBlock(this, data, x, y, z);
        }

        internal override void BlockUpdate(World world, int x, int y, int z, uint data, BlockSide side)
        {
            uint updatedData = GetConnectionData(world, x, y, z);
            OpenOpposingSide(ref updatedData);

            if (updatedData != data)
            {
                world.SetBlock(this, updatedData, x, y, z);
            }
        }

        private uint GetConnectionData(World world, int x, int y, int z)
        {
            uint data = 0;

            if (IsConnectable(BlockSide.Back, x, y, z + 1)) data |= 0b10_0000;
            if (IsConnectable(BlockSide.Front, x, y, z - 1)) data |= 0b01_0000;
            if (IsConnectable(BlockSide.Right, x - 1, y, z)) data |= 0b00_1000;
            if (IsConnectable(BlockSide.Left, x + 1, y, z)) data |= 0b00_0100;
            if (IsConnectable(BlockSide.Top, x, y - 1, z)) data |= 0b00_0010;
            if (IsConnectable(BlockSide.Bottom, x, y + 1, z)) data |= 0b00_0001;

            return data;

            bool IsConnectable(BlockSide side, int cx, int cy, int cz)
            {
                Block? block = world.GetBlock(cx, cy, cz, out _);

                return block == this || (block is TConnect connectable && connectable.IsConnectable(world, side, cx, cy, cz));
            }
        }

        private static void OpenOpposingSide(ref uint data)
        {
            if (BitHelper.CountSetBits(data) != 1) return;

            switch (data)
            {
                case 0b10_0000:
                case 0b01_0000:
                    data = 0b11_0000;
                    break;

                case 0b00_1000:
                case 0b00_0100:
                    data = 0b00_1100;
                    break;

                case 0b00_0010:
                case 0b00_0001:
                    data = 0b00_0011;
                    break;
            }
        }

        public bool AllowInflow(World world, int x, int y, int z, BlockSide side, Liquid liquid)
        {
            return IsSideOpen(world, x, y, z, side);
        }

        public bool AllowOutflow(World world, int x, int y, int z, BlockSide side)
        {
            return IsSideOpen(world, x, y, z, side);
        }

        private static bool IsSideOpen(World world, int x, int y, int z, BlockSide side)
        {
            world.GetBlock(x, y, z, out uint data);

            return side switch
            {
                BlockSide.Front => (data & 0b10_0000) != 0,
                BlockSide.Back => (data & 0b01_0000) != 0,
                BlockSide.Left => (data & 0b00_1000) != 0,
                BlockSide.Right => (data & 0b00_0100) != 0,
                BlockSide.Bottom => (data & 0b00_0010) != 0,
                BlockSide.Top => (data & 0b00_0001) != 0,
                _ => true
            };
        }
    }
}