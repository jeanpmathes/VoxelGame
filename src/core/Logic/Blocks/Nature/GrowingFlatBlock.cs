// <copyright file="GrowingFlatBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// A block that grows downwards and can hang freely. This block is affected by neutral tint.
    /// Data bit usage: <c>-aaaoo</c>
    /// </summary>
    // o = orientation
    // a = age
    public class GrowingFlatBlock : FlatBlock, IFlammable, IFillable
    {
        internal GrowingFlatBlock(string name, string namedId, string texture, float climbingVelocity, float slidingVelocity) :
            base(
                name,
                namedId,
                texture,
                climbingVelocity,
                slidingVelocity)
        {
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            return base.GetMesh(info).Modified(TintColor.Neutral);
        }

        internal override void BlockUpdate(int x, int y, int z, uint data, BlockSide side)
        {
            var orientation = (Orientation)(data & 0b00_0011);

            // If another block of this type is above, no solid block is required to hold.
            if ((Game.World.GetBlock(x, y + 1, z, out uint dataAbove) ?? Block.Air) == this && orientation == (Orientation)(dataAbove & 0b00_0011))
            {
                return;
            }
            else if (side == BlockSide.Top)
            {
                side = orientation.Invert().ToBlockSide();
            }

            CheckBack(x, y, z, side, orientation, true);
        }

        internal override void RandomUpdate(int x, int y, int z, uint data)
        {
            var orientation = (Orientation)(data & 0b00_0011);
            var age = (int)((data & 0b1_1100) >> 2);

            if (age < 7)
            {
                Game.World.SetBlock(this, (uint)(((age + 1) << 2) | (int)orientation), x, y, z);
            }
            else if (Game.World.GetBlock(x, y - 1, z, out _) == Block.Air)
            {
                Game.World.SetBlock(this, (uint)orientation, x, y - 1, z);
            }
        }

        public void LiquidChange(int x, int y, int z, Liquid liquid, LiquidLevel level)
        {
            if (liquid.Direction > 0 && level > LiquidLevel.Two) ScheduleDestroy(x, y, z);
        }
    }
}