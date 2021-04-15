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
        public ModifiableHeightBlock(string name, string namedId, TextureLayout layout) :
            base(
                name,
                namedId,
                layout,
                isSolid: true,
                receiveCollisions: false,
                isTrigger: false,
                isReplaceable: false,
                isInteractable: true)
        {
        }

        internal override bool CanPlace(int x, int y, int z, PhysicsEntity? entity)
        {
            return Game.World.HasSolidGround(x, y, z);
        }

        internal override void BlockUpdate(int x, int y, int z, uint data, BlockSide side)
        {
            if (side == BlockSide.Bottom && !Game.World.HasSolidGround(x, y, z))
            {
                if (GetHeight(data) == IHeightVariable.MaximumHeight)
                    ScheduleDestroy(x, y, z);
                else
                    Destroy(x, y, z);
            }
        }

        protected override void EntityInteract(PhysicsEntity entity, int x, int y, int z, uint data)
        {
            uint height = data & 0b00_1111;
            height++;

            if (height <= IHeightVariable.MaximumHeight)
            {
                Game.World.SetBlock(this, height, x, y, z);
            }
        }
    }
}