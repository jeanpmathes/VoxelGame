// <copyright file="TintedBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// A block that has differently colored versions. Animation can be activated.
    /// Data bit usage: <c>-ccccc</c>
    /// </summary>
    // c = color
    public class TintedBlock : BasicBlock, IConnectable
    {
        private protected readonly bool isAnimated;

        public TintedBlock(string name, string namedId, TextureLayout layout, bool isAnimated = false) :
            base(
                name,
                namedId,
                layout,
                isOpaque: true,
                renderFaceAtNonOpaques: true,
                isSolid: true,
                isInteractable: true)
        {
            this.isAnimated = isAnimated;
        }

        public override uint GetMesh(BlockSide side, uint data, Liquid liquid, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint, out bool isAnimated)
        {
            tint = ((BlockColor)(0b01_1111 & data)).ToTintColor();
            isAnimated = this.isAnimated;

            return base.GetMesh(side, data, liquid, out vertices, out textureIndices, out indices, out _, out _);
        }

        protected override void EntityInteract(PhysicsEntity entity, int x, int y, int z, uint data)
        {
            Game.World.SetBlock(this, data + 1 & 0b01_1111, x, y, z);
        }
    }
}