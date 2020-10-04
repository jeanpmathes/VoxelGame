// <copyright file="LiquidBarrierBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// A block that lets liquids through but can be closed by interacting with it.
    /// Data bit usage: <c>-----o</c>
    /// </summary>
    // o = open
    public class LiquidBarrierBlock : BasicBlock, IFillable
    {
        private protected int[][] openTextureIndices = null!;

        private protected TextureLayout open;

        public LiquidBarrierBlock(string name, string namedId, TextureLayout closed, TextureLayout open) :
            base(
                name,
                namedId,
                layout: closed,
                isOpaque: true,
                renderFaceAtNonOpaques: true,
                isSolid: true,
                isInteractable: true)
        {
            this.open = open;
        }

        protected override void Setup()
        {
            base.Setup();

            openTextureIndices = open.GetTexIndexArrays();
        }

        public override uint GetMesh(BlockSide side, uint data, Liquid liquid, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint, out bool isAnimated)
        {
            uint verts = base.GetMesh(side, data, TODO, out vertices, out textureIndices, out indices, out tint, out isAnimated);

            if ((data & 0b00_0001) == 1) textureIndices = openTextureIndices[(int)side];

            return verts;
        }

        protected override void EntityInteract(PhysicsEntity entity, int x, int y, int z, uint data)
        {
            Game.World.SetBlock(this, data ^ 0b00_0001, x, y, z);
        }

        public bool IsFillable(int x, int y, int z, Liquid liquid)
        {
            Game.World.GetBlock(x, y, z, out uint data);
            return (data & 0b00_0001) == 1;
        }
    }
}