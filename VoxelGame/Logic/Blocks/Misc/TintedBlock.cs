// <copyright file="TintedBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Entities;
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
                isSolid: true,
                isInteractable: true)
        {
        }

        public override uint GetMesh(BlockSide side, byte data, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint)
        {
            tint = ((BlockColor)(0b0_0111 & data)).ToTintColor();

            return base.GetMesh(side, data, out vertices, out textureIndices, out indices, out _);
        }

        protected override bool Place(Entities.PhysicsEntity? entity, int x, int y, int z)
        {
            Game.World.SetBlock(this, 0, x, y, z);

            return true;
        }

        protected override void EntityInteract(PhysicsEntity entity, int x, int y, int z, byte data)
        {
            Game.World.SetBlock(this, (byte)(data + 1 & 0b0_0111), x, y, z);
        }
    }
}