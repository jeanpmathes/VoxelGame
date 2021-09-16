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
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    ///     A block that only connects to steel pipes at specific sides.
    ///     Data bit usage: <c>----aa</c>
    /// </summary>
    // aa = axis
    internal class StraightSteelPipeBlock : Block, IFillable, IIndustrialPipeConnectable
    {
        private protected const uint AxisDataMask = 0b00_0011;

        private readonly float diameter;

        private protected readonly (BlockModel x, BlockModel y, BlockModel z) models;

        internal StraightSteelPipeBlock(string name, string namedId, float diameter, string model,
            bool isInteractable = false) :
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
                new BoundingBox(new Vector3(x: 0.5f, y: 0.5f, z: 0.5f), new Vector3(diameter, diameter, z: 0.5f)),
                TargetBuffer.Complex)
        {
            BlockModel initial = BlockModel.Load(model);

            models = initial.CreateAllAxis();

            this.diameter = diameter;
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

        public virtual bool IsConnectable(World world, BlockSide side, Vector3i position)
        {
            return IsSideOpen(world, position, side);
        }

        protected override BoundingBox GetBoundingBox(uint data)
        {
            var axis = (Axis) (data & AxisDataMask);

            return new BoundingBox(new Vector3(x: 0.5f, y: 0.5f, z: 0.5f), axis.Vector3(onAxis: 0.5f, diameter));
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            uint vertexCount = SelectModel(
                models,
                (Axis) (info.Data & AxisDataMask),
                out float[] vertices,
                out int[] textureIndices,
                out uint[] indices);

            return BlockMeshData.Complex(vertexCount, vertices, textureIndices, indices);
        }

        protected static uint SelectModel((BlockModel x, BlockModel y, BlockModel z) modelTuple, Axis axis,
            out float[] vertices, out int[] textureIndices, out uint[] indices)
        {
            var (x, y, z) = modelTuple;

            switch (axis)
            {
                case Axis.X:
                    x.ToData(out vertices, out textureIndices, out indices);

                    return (uint) x.VertexCount;

                case Axis.Y:
                    y.ToData(out vertices, out textureIndices, out indices);

                    return (uint) y.VertexCount;

                case Axis.Z:
                    z.ToData(out vertices, out textureIndices, out indices);

                    return (uint) z.VertexCount;

                default:
                    throw new NotSupportedException();
            }
        }

        protected override void DoPlace(World world, Vector3i position, PhysicsEntity? entity)
        {
            world.SetBlock(this, (uint) (entity?.TargetSide ?? BlockSide.Front).Axis(), position);
        }

        protected virtual bool IsSideOpen(World world, Vector3i position, BlockSide side)
        {
            world.GetBlock(position, out uint data);

            return side.Axis() == (Axis) (data & AxisDataMask);
        }
    }
}