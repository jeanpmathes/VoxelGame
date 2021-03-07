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

        public SteelPipeValveBlock(string name, string namedId, float diameter, string openModel, string closedModel) :
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

        public override uint GetMesh(BlockSide side, uint data, Liquid liquid, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint, out bool isAnimated)
        {
            tint = TintColor.None;
            isAnimated = false;

            return SelectModel((data & 0b00_0100) == 0 ? models : closedModels, (Axis)(data & AxisDataMask), out vertices, out textureIndices, out indices);
        }

        public override bool IsConnectable(BlockSide side, int x, int y, int z)
        {
            return base.IsSideOpen(x, y, z, side);
        }

        protected override bool IsSideOpen(int x, int y, int z, BlockSide side)
        {
            Game.World.GetBlock(x, y, z, out uint data);
            if ((data & 0b00_0100) != 0) return false;
            return ToAxis(side) == (Axis)(data & AxisDataMask);
        }

        protected override void EntityInteract(PhysicsEntity entity, int x, int y, int z, uint data)
        {
            Game.World.SetBlock(this, data ^ 0b00_0100, x, y, z);
        }
    }
}