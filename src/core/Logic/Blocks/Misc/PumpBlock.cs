// <copyright file="PumpBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// Pumps water upwards when interacted with.
    /// Data bit usage: <c>------</c>
    /// </summary>
    internal class PumpBlock : BasicBlock, IIndustrialPipeConnectable, IFillable
    {
        private readonly int pumpDistance;

        public PumpBlock(string name, string namedId, int pumpDistance, TextureLayout layout) :
            base(
                name,
                namedId,
                layout,
                isOpaque: true,
                renderFaceAtNonOpaques: true,
                isSolid: true,
                receiveCollisions: false,
                isTrigger: false,
                isInteractable: true)
        {
            this.pumpDistance = pumpDistance;
        }

        protected override void EntityInteract(PhysicsEntity entity, int x, int y, int z, uint data)
        {
            Liquid.Elevate(x, y, z, pumpDistance);
        }

        public bool AllowInflow(int x, int y, int z, BlockSide side, Liquid liquid)
        {
            return side != BlockSide.Top;
        }

        public bool AllowOutflow(int x, int y, int z, BlockSide side)
        {
            return side == BlockSide.Top;
        }
    }
}