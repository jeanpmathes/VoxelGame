// <copyright file="GrowingFlatBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Logic.Interfaces;
using VoxelGame.Utilities;
using VoxelGame.Visuals;

namespace VoxelGame.Logic.Blocks
{
    /// <summary>
    /// A block that grows downwards and can hang freely. This block is affected by neutral tint.
    /// Data bit usage: <c>-aaaoo</c>
    /// </summary>
    // o = orientation
    // a = age
    public class GrowingFlatBlock : FlatBlock, IFlammable
    {
        public GrowingFlatBlock(string name, string namedId, string texture, float climbingVelocity, float slidingVelocity) :
            base(
                name,
                namedId,
                texture,
                climbingVelocity,
                slidingVelocity)
        {
        }

        public override uint GetMesh(BlockSide side, uint data, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint, out bool isAnimated)
        {
            tint = TintColor.Neutral;

            return base.GetMesh(side, data, out vertices, out textureIndices, out indices, out _, out isAnimated);
        }

        internal override void BlockUpdate(int x, int y, int z, uint data, BlockSide side)
        {
            Orientation orientation = (Orientation)(data & 0b00_0011);

            // If another block of this type is above, no solid block is required to hold.
            if ((Game.World.GetBlock(x, y + 1, z, out uint dataAbove) ?? Block.Air) == this && orientation == (Orientation)(dataAbove & 0b00_0011))
            {
                return;
            }
            else if (side == BlockSide.Top)
            {
                side = orientation.Invert().ToBlockSide();
            }

            CheckBack(x, y, z, side, orientation);
        }

        internal override void RandomUpdate(int x, int y, int z, uint data)
        {
            Orientation orientation = (Orientation)(data & 0b00_0011);
            int age = (int)((data & 0b1_1100) >> 2);

            if (age < 7)
            {
                Game.World.SetBlock(this, (uint)(((age + 1) << 2) | (int)orientation), x, y, z);
            }
            else if (Game.World.GetBlock(x, y - 1, z, out _) == Block.Air)
            {
                Game.World.SetBlock(this, (uint)orientation, x, y - 1, z);
            }
        }
    }
}