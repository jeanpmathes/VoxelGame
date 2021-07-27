// <copyright file="GroundedBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// A BasicBlock that can only be placed on top of blocks that are both solid and full or will become such blocks. These blocks are also flammable.
    /// Data bit usage: <c>------</c>
    /// </summary>
    public class GroundedBlock : BasicBlock, IFlammable
    {
        internal GroundedBlock(string name, string namedId, TextureLayout layout, bool isOpaque = true, bool renderFaceAtNonOpaques = true, bool isSolid = true, bool isInteractable = false) :
            base(
                name,
                namedId,
                layout,
                isOpaque,
                renderFaceAtNonOpaques,
                isSolid,
                receiveCollisions: false,
                isTrigger: false,
                isInteractable)
        {
        }

        internal override bool CanPlace(World world, int x, int y, int z, PhysicsEntity? entity)
        {
            return world.HasSolidGround(x, y, z, solidify: true);
        }

        internal override void BlockUpdate(World world, int x, int y, int z, uint data, BlockSide side)
        {
            if (side == BlockSide.Bottom && !world.HasSolidGround(x, y, z))
            {
                ScheduleDestroy(world, x, y, z);
            }
        }
    }
}