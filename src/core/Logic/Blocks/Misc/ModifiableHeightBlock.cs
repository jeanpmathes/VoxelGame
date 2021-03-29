using System;
using System.Collections.Generic;
using System.Text;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;

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

        protected override void EntityInteract(PhysicsEntity entity, int x, int y, int z, uint data)
        {
            uint height = data & 0b00_1111;
            height++;

            if (height < IHeightVariable.MaximumHeight)
            {
                Game.World.SetBlock(this, height, x, y, z);
            }
        }
    }
}