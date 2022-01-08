// <copyright file="StraightSteelPipeBlock.cs" company="VoxelGame">
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
    ///     A block that only connects to steel pipes at specific sides.
    ///     Data bit usage: <c>----aa</c>
    /// </summary>
    // aa: axis
    internal class StraightSteelPipeBlock : Block, IFillable, IIndustrialPipeConnectable
    {
        private readonly float diameter;

        private readonly List<BlockMesh> meshes = new(capacity: 3);

        internal StraightSteelPipeBlock(string name, string namedId, float diameter, string model) :
            base(
                name,
                namedId,
                BlockFlags.Solid,
                new BoundingBox(new Vector3(x: 0.5f, y: 0.5f, z: 0.5f), new Vector3(diameter, diameter, z: 0.5f)),
                TargetBuffer.Complex)
        {
            this.diameter = diameter;

            (BlockModel x, BlockModel y, BlockModel z) = BlockModel.Load(model).CreateAllAxis();

            meshes.Add(x.GetMesh());
            meshes.Add(y.GetMesh());
            meshes.Add(z.GetMesh());
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
            return IsSideOpen(world, position, side);
        }

        protected override BoundingBox GetBoundingBox(uint data)
        {
            var axis = (Axis) (data & 0b00_0011);

            return new BoundingBox(new Vector3(x: 0.5f, y: 0.5f, z: 0.5f), axis.Vector3(onAxis: 0.5f, diameter));
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            BlockMesh mesh = meshes[(int) info.Data & 0b00_0011];

            return mesh.GetComplexMeshData();
        }

        protected override void DoPlace(World world, Vector3i position, PhysicsEntity? entity)
        {
            world.SetBlock(this.AsInstance((uint) (entity?.TargetSide ?? BlockSide.Front).Axis()), position);
        }

        private static bool IsSideOpen(World world, Vector3i position, BlockSide side)
        {
            BlockInstance block = world.GetBlock(position) ?? BlockInstance.Default;

            return side.Axis() == (Axis) (block.Data & 0b00_0011);
        }
    }
}