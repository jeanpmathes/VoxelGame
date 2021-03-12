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
    public class LiquidBarrierBlock : BasicBlock, IFillable, IFlammable
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
                receiveCollisions: false,
                isTrigger: false,
                isInteractable: true)
        {
            this.open = open;
        }

        protected override void Setup()
        {
            base.Setup();

            openTextureIndices = open.GetTexIndexArrays();
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            BlockMeshData mesh = base.GetMesh(info);

            if ((info.Data & 0b00_0001) == 1)
                mesh = mesh.SwapTextureIndices(openTextureIndices[(int)info.Side]);

            return mesh;
        }

        protected override void EntityInteract(PhysicsEntity entity, int x, int y, int z, uint data)
        {
            Game.World.SetBlock(this, data ^ 0b00_0001, x, y, z);
        }

        public bool AllowInflow(int x, int y, int z, BlockSide side, Liquid liquid)
        {
            Game.World.GetBlock(x, y, z, out uint data);
            return (data & 0b00_0001) == 1;
        }
    }
}