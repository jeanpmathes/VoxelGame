// <copyright file="GrowingFlatBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Utilities;
using VoxelGame.Visuals;

namespace VoxelGame.Logic.Blocks
{
    /// <summary>
    /// A block that grows downwards and can hang freely. This block is affected by neutral tint.
    /// Data bit usage: <c>aaaoo</c>
    /// </summary>
    // o = orientation
    // a = age
    public class GrowingFlatBlock : FlatBlock
    {
        public GrowingFlatBlock(string name, string texture, float climbingVelocity, float slidingVelocity) :
            base(
                name,
                texture,
                climbingVelocity,
                slidingVelocity)
        {
        }

        public override uint GetMesh(BlockSide side, byte data, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint)
        {
            tint = TintColor.Neutral;

            return base.GetMesh(side, data, out vertices, out textureIndices, out indices, out _);
        }

        internal override void BlockUpdate(int x, int y, int z, byte data)
        {
            Orientation orientation = (Orientation)(data & 0b0_0011);

            // If another block of this type is above, no solid block is required to hold.
            if ((Block)(Game.World.GetBlock(x, y + 1, z, out byte dataAbove) ?? Block.AIR) == this && orientation == (Orientation)(dataAbove & 0b0_0011))
            {
                return;
            }

            if (orientation == Orientation.North && (Game.World.GetBlock(x, y, z + 1, out _)?.IsSolidAndFull != true))
            {
                Destroy(x, y, z, null);
            }

            if (orientation == Orientation.South && (Game.World.GetBlock(x, y, z - 1, out _)?.IsSolidAndFull != true))
            {
                Destroy(x, y, z, null);
            }

            if (orientation == Orientation.East && (Game.World.GetBlock(x - 1, y, z, out _)?.IsSolidAndFull != true))
            {
                Destroy(x, y, z, null);
            }

            if (orientation == Orientation.West && (Game.World.GetBlock(x + 1, y, z, out _)?.IsSolidAndFull != true))
            {
                Destroy(x, y, z, null);
            }
        }

        internal override void RandomUpdate(int x, int y, int z, byte data)
        {
            Orientation orientation = (Orientation)(data & 0b0_0011);
            int age = (data & 0b1_1100) >> 2;

            if (age < 7)
            {
                Game.World.SetBlock(this, (byte)(((age + 1) << 2) | (int)orientation), x, y, z);
            }
            else if (Game.World.GetBlock(x, y - 1, z, out _) == Block.AIR)
            {
                Game.World.SetBlock(this, (byte)orientation, x, y - 1, z);
            }
        }
    }
}