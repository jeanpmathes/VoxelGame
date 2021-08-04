// <copyright file="SteelPipeValveBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Entities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// A block that only connects to steel pipes at specific sides and can be closed.
    /// Data bit usage: <c>---oaa</c>
    /// </summary>
    // aa = axis
    // o = open
    internal class SteelPipeValveBlock : StraightSteelPipeBlock
    {
        private readonly (BlockModel x, BlockModel y, BlockModel z) closedModels;

        internal SteelPipeValveBlock(string name, string namedId, float diameter, string openModel, string closedModel) :
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
            uint vertexCount = SelectModel((info.Data & 0b00_0100) == 0 ? models : closedModels,
                (Axis)(info.Data & AxisDataMask), out float[] vertices, out int[] textureIndices, out uint[] indices);

            return BlockMeshData.Complex(vertexCount, vertices, textureIndices, indices);
        }

        public override bool IsConnectable(World world, BlockSide side, int x, int y, int z)
        {
            return base.IsSideOpen(world, x, y, z, side);
        }

        protected override bool IsSideOpen(World world, int x, int y, int z, BlockSide side)
        {
            world.GetBlock(x, y, z, out uint data);
            if ((data & 0b00_0100) != 0) return false;
            return ToAxis(side) == (Axis)(data & AxisDataMask);
        }

        protected override void EntityInteract(PhysicsEntity entity, int x, int y, int z, uint data)
        {
            entity.World.SetBlock(this, data ^ 0b00_0100, x, y, z);
        }
    }
}