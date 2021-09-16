// <copyright file="GrowingFlatBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    ///     A block that grows downwards and can hang freely. This block is affected by neutral tint.
    ///     Data bit usage: <c>-aaaoo</c>
    /// </summary>
    // o = orientation
    // a = age
    public class GrowingFlatBlock : FlatBlock, IFlammable, IFillable
    {
        internal GrowingFlatBlock(string name, string namedId, string texture, float climbingVelocity,
            float slidingVelocity) :
            base(
                name,
                namedId,
                texture,
                climbingVelocity,
                slidingVelocity) {}

        public void LiquidChange(World world, Vector3i position, Liquid liquid, LiquidLevel level)
        {
            if (liquid.Direction > 0 && level > LiquidLevel.Two) ScheduleDestroy(world, position);
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            return base.GetMesh(info).Modified(TintColor.Neutral);
        }

        internal override void BlockUpdate(World world, Vector3i position, uint data, BlockSide side)
        {
            var orientation = (Orientation) (data & 0b00_0011);

            // If another block of this type is above, no solid block is required to hold.
            if ((world.GetBlock(position + Vector3i.UnitY, out uint dataAbove) ?? Air) == this &&
                orientation == (Orientation) (dataAbove & 0b00_0011)) return;

            if (side == BlockSide.Top) side = orientation.Opposite().ToBlockSide();

            CheckBack(world, position, side, orientation, schedule: true);
        }

        internal override void RandomUpdate(World world, Vector3i position, uint data)
        {
            var orientation = (Orientation) (data & 0b00_0011);
            var age = (int) ((data & 0b1_1100) >> 2);

            if (age < 7) world.SetBlock(this, (uint) (((age + 1) << 2) | (int) orientation), position);
            else if (world.GetBlock(position - Vector3i.UnitY, out _) == Air)
                world.SetBlock(this, (uint) orientation, position - Vector3i.UnitY);
        }
    }
}