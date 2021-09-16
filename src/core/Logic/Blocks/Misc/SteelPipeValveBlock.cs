// <copyright file="SteelPipeValveBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;
using VoxelGame.Core.Entities;
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
    internal class SteelPipeValveBlock : StraightSteelPipeBlock
    {
        private readonly (BlockModel x, BlockModel y, BlockModel z) closedModels;

        internal SteelPipeValveBlock(string name, string namedId, float diameter, string openModel,
            string closedModel) :
            base(
                name,
                namedId,
                diameter,
                openModel,
                isInteractable: true)
        {
            BlockModel initial = BlockModel.Load(closedModel);

            closedModels = initial.CreateAllAxis();
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            uint vertexCount = SelectModel(
                (info.Data & 0b00_0100) == 0 ? models : closedModels,
                (Axis) (info.Data & AxisDataMask),
                out float[] vertices,
                out int[] textureIndices,
                out uint[] indices);

            return BlockMeshData.Complex(vertexCount, vertices, textureIndices, indices);
        }

        public override bool IsConnectable(World world, BlockSide side, Vector3i position)
        {
            return base.IsSideOpen(world, position, side);
        }

        protected override bool IsSideOpen(World world, Vector3i position, BlockSide side)
        {
            world.GetBlock(position, out uint data);

            if ((data & 0b00_0100) != 0) return false;

            return side.Axis() == (Axis) (data & AxisDataMask);
        }

        protected override void EntityInteract(PhysicsEntity entity, Vector3i position, uint data)
        {
            entity.World.SetBlock(this, data ^ 0b00_0100, position);
        }
    }
}