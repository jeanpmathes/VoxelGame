// <copyright file="StraightSteelPipeBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenToolkit.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// A block that only connects to steel pipes at specific sides.
    /// Data bit usage: <c>----aa</c>
    /// </summary>
    // aa = axis
    internal class StraightSteelPipeBlock : Block, IFillable, IIndustrialPipeConnectable
    {
        private protected const uint AxisDataMask = 0b00_0011;

        private protected readonly (BlockModel x, BlockModel y, BlockModel z) models;

        private readonly float diameter;

        public bool RenderLiquid => false;

        internal StraightSteelPipeBlock(string name, string namedId, float diameter, string model, bool isInteractable = false) :
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
                isInteractable,
                boundingBox: new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(diameter, diameter, 0.5f)),
                targetBuffer: TargetBuffer.Complex)
        {
            BlockModel initial = BlockModel.Load(model);

            models = initial.CreateAllAxis();

            this.diameter = diameter;
        }

        protected override BoundingBox GetBoundingBox(int x, int y, int z, uint data)
        {
            return (Axis)(data & AxisDataMask) switch
            {
                Axis.X => new BoundingBox(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f), new Vector3(0.5f, diameter, diameter)),
                Axis.Y => new BoundingBox(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f), new Vector3(diameter, 0.5f, diameter)),
                Axis.Z => new BoundingBox(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f), new Vector3(diameter, diameter, 0.5f)),
                _ => throw new NotSupportedException()
            };
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            uint vertexCount = SelectModel(models, (Axis)(info.Data & AxisDataMask), out float[] vertices, out int[] textureIndices, out uint[] indices);
            return new BlockMeshData(vertexCount, vertices, textureIndices, indices);
        }

        protected static uint SelectModel((BlockModel x, BlockModel y, BlockModel z) modelTuple, Axis axis, out float[] vertices, out int[] textureIndices, out uint[] indices)
        {
            var (x, y, z) = modelTuple;
            switch (axis)
            {
                case Axis.X:
                    x.ToData(out vertices, out textureIndices, out indices);
                    return (uint)x.VertexCount;

                case Axis.Y:
                    y.ToData(out vertices, out textureIndices, out indices);
                    return (uint)y.VertexCount;

                case Axis.Z:
                    z.ToData(out vertices, out textureIndices, out indices);
                    return (uint)z.VertexCount;

                default:
                    throw new NotSupportedException();
            }
        }

        protected override void DoPlace(int x, int y, int z, PhysicsEntity? entity)
        {
            Game.World.SetBlock(this, (uint)ToAxis(entity?.TargetSide ?? BlockSide.Front), x, y, z);
        }

        public virtual bool IsConnectable(BlockSide side, int x, int y, int z)
        {
            return IsSideOpen(x, y, z, side);
        }

        public bool AllowInflow(int x, int y, int z, BlockSide side, Liquid liquid)
        {
            return IsSideOpen(x, y, z, side);
        }

        public bool AllowOutflow(int x, int y, int z, BlockSide side)
        {
            return IsSideOpen(x, y, z, side);
        }

        protected virtual bool IsSideOpen(int x, int y, int z, BlockSide side)
        {
            Game.World.GetBlock(x, y, z, out uint data);
            return ToAxis(side) == (Axis)(data & AxisDataMask);
        }

        protected enum Axis
        {
            X = 0b00,
            Y = 0b01,
            Z = 0b10,
        }

        protected static Axis ToAxis(BlockSide side)
        {
            switch (side)
            {
                case BlockSide.Front:
                case BlockSide.Back:
                    return Axis.Z;

                case BlockSide.Left:
                case BlockSide.Right:
                    return Axis.X;

                case BlockSide.Bottom:
                case BlockSide.Top:
                    return Axis.Y;

                default:
                    throw new ArgumentOutOfRangeException(nameof(side));
            }
        }
    }
}