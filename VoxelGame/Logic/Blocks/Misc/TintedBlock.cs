// <copyright file="TintedBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Logic.Interfaces;
using VoxelGame.Visuals;

namespace VoxelGame.Logic.Blocks
{
    /// <summary>
    /// A block that has differently colored versions.
    /// Data bit usage: <c>--ccc</c>
    /// </summary>
    // c = color
    public class TintedBlock : BasicBlock, IConnectable
    {
        public TintedBlock(string name, TextureLayout layout) :
            base(
                name,
                layout,
                isOpaque: true,
                renderFaceAtNonOpaques: true,
                isSolid: true)
        {
        }

        public override bool Place(int x, int y, int z, Entities.PhysicsEntity? entity)
        {
            if (Game.World.GetBlock(x, y, z, out _)?.IsReplaceable != true)
            {
                return false;
            }

            Game.World.SetBlock(this, (byte)(x & 0b111), x, y, z);

            return true;
        }

        public override uint GetMesh(BlockSide side, byte data, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint)
        {
            tint = BlockToTintColor((BlockColor)(0b0_0111 & data));

            return base.GetMesh(side, data, out vertices, out textureIndices, out indices, out _);
        }

        protected enum BlockColor
        {
            Default,
            Red,
            Green,
            Blue,
            Yellow,
            Cyan,
            Magenta
        }

        protected static TintColor BlockToTintColor(BlockColor color)
        {
            return color switch
            {
                BlockColor.Red => TintColor.Red,
                BlockColor.Green => TintColor.Green,
                BlockColor.Blue => TintColor.Blue,
                BlockColor.Yellow => TintColor.Yellow,
                BlockColor.Cyan => TintColor.Cyan,
                BlockColor.Magenta => TintColor.Magenta,
                _ => new TintColor(1f, 1f, 1f),
            };
        }
    }
}