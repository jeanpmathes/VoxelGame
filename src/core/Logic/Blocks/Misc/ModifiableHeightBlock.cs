// <copyright file="ModifiableHeightBlock.cs" company="VoxelGame">
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
    /// A block that allows to change its height by interacting.
    /// Data bit usage: <c>--hhhh</c>
    /// </summary>
    public class ModifiableHeightBlock : VaryingHeightBlock
    {
        internal ModifiableHeightBlock(string name, string namedId, TextureLayout layout) :
            base(
                name,
                namedId,
                layout,
                isSolid: true,
                receiveCollisions: false,
                isTrigger: false,
                isReplaceable: false,
                isInteractable: true) {}

        internal override bool CanPlace(World world, int x, int y, int z, PhysicsEntity? entity)
        {
            return world.HasSolidGround(x, y, z, solidify: true);
        }

        internal override void BlockUpdate(World world, int x, int y, int z, uint data, BlockSide side)
        {
            if (side == BlockSide.Bottom && !world.HasSolidGround(x, y, z))
            {
                if (GetHeight(data) == IHeightVariable.MaximumHeight)
                    ScheduleDestroy(world, x, y, z);
                else
                    Destroy(world, x, y, z);
            }
        }

        protected override void EntityInteract(PhysicsEntity entity, int x, int y, int z, uint data)
        {
            uint height = data & 0b00_1111;
            height++;

            if (height <= IHeightVariable.MaximumHeight)
            {
                entity.World.SetBlock(this, height, x, y, z);
            }
        }
    }
}