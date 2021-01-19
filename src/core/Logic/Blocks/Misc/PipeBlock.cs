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
    /// A block that connects to other pipes and allows water flow.
    /// Data bit usage: <c>fblrdt</c>
    /// </summary>
    // f: front
    // b: back
    // l: left
    // r: right
    // d: bottom
    // t: top
    internal class PipeBlock : Block, IFillable
    {
        private protected readonly BlockModel center;
        private protected readonly (BlockModel front, BlockModel back, BlockModel left, BlockModel right, BlockModel bottom, BlockModel top) connector;
        private protected readonly (BlockModel front, BlockModel back, BlockModel left, BlockModel right, BlockModel bottom, BlockModel top) surface;

        public PipeBlock(string name, string namedId, string centerModel, string connectorModel, string surfaceModel) :
            base(
                name,
                namedId,
                isFull: false,
                isOpaque: false,
                renderFaceAtNonOpaques: true,
                isSolid: true,
                recieveCollisions: false,
                isTrigger: false,
                isReplaceable: false,
                isInteractable: false,
                boundingBox: new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.375f, 0.375f, 0.375f)),
                targetBuffer: TargetBuffer.Complex)
        {
            center = BlockModel.Load(centerModel);

            BlockModel frontConnector = BlockModel.Load(connectorModel);
            BlockModel frontSurface = BlockModel.Load(surfaceModel);

            connector = frontConnector.CreateAllSides();
            surface = frontSurface.CreateAllSides();

            center.Lock();
            connector.Lock();
            surface.Lock();
        }

        public override uint GetMesh(BlockSide side, uint data, Liquid liquid, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint, out bool isAnimated)
        {
            (vertices, textureIndices, indices) = BlockModel.CombineData(out uint vertexCount, center,
                (data & 0b10_0000) == 0 ? surface.front : connector.front,
                (data & 0b01_0000) == 0 ? surface.back : connector.back,
                (data & 0b00_1000) == 0 ? surface.left : connector.left,
                (data & 0b00_0100) == 0 ? surface.right : connector.right,
                (data & 0b00_0010) == 0 ? surface.bottom : connector.bottom,
                (data & 0b00_0001) == 0 ? surface.top : connector.top);

            tint = TintColor.None;
            isAnimated = false;

            return vertexCount;
        }

        protected override bool Place(PhysicsEntity? entity, int x, int y, int z)
        {
            uint data = GetConnectionData(x, y, z);

            OpenOpposingSide(ref data);

            Game.World.SetBlock(this, data, x, y, z);

            return true;
        }

        internal override void BlockUpdate(int x, int y, int z, uint data, BlockSide side)
        {
            uint updatedData = GetConnectionData(x, y, z);
            OpenOpposingSide(ref updatedData);

            if (updatedData != data)
            {
                Game.World.SetBlock(this, updatedData, x, y, z);
            }
        }

        private uint GetConnectionData(int x, int y, int z)
        {
            uint data = 0;

            if (Game.World.GetBlock(x, y, z + 1, out _) == this) data |= 0b10_0000;
            if (Game.World.GetBlock(x, y, z - 1, out _) == this) data |= 0b01_0000;
            if (Game.World.GetBlock(x - 1, y, z, out _) == this) data |= 0b00_1000;
            if (Game.World.GetBlock(x + 1, y, z, out _) == this) data |= 0b00_0100;
            if (Game.World.GetBlock(x, y - 1, z, out _) == this) data |= 0b00_0010;
            if (Game.World.GetBlock(x, y + 1, z, out _) == this) data |= 0b00_0001;

            return data;
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

        public bool AllowInflow(int x, int y, int z, BlockSide side, Liquid liquid)
        {
            return IsSideOpen(x, y, z, side);
        }

        public bool AllowOutflow(int x, int y, int z, BlockSide side)
        {
            return IsSideOpen(x, y, z, side);
        }

        private static bool IsSideOpen(int x, int y, int z, BlockSide side)
        {
            Game.World.GetBlock(x, y, z, out uint data);

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