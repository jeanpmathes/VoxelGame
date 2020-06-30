// <copyright file="TintedBlock.cs" company="VoxelGame">
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

        public override uint GetMesh(BlockSide side, byte data, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint)
        {
            tint = ((BlockColor)(0b0_0111 & data)).ToTintColor();

            return base.GetMesh(side, data, out vertices, out textureIndices, out indices, out _);
        }

        protected override bool Place(int x, int y, int z, bool? replaceable, Entities.PhysicsEntity? entity)
        {
            if (replaceable != true)
            {
                return false;
            }

            Game.World.SetBlock(this, (byte)(x & 0b111), x, y, z);

            return true;
        }
    }
}