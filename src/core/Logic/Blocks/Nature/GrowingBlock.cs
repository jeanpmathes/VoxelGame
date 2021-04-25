// <copyright file="GrowingBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// A block that grows upwards and is destroyed if a certain ground block is not given.
    /// Data bit usage: <c>---aaa</c>
    /// </summary>
    // a = age
    public class GrowingBlock : BasicBlock, IFlammable
    {
        private readonly Block requiredGround;
        private readonly int maxHeight;

        internal GrowingBlock(string name, string namedId, TextureLayout layout, Block ground, int maxHeight) :
            base(
                name,
                namedId,
                layout,
                isOpaque: true,
                renderFaceAtNonOpaques: true,
                isSolid: true,
                receiveCollisions: false,
                isTrigger: false,
                isInteractable: false)
        {
            requiredGround = ground;
            this.maxHeight = maxHeight;
        }

        internal override bool CanPlace(int x, int y, int z, PhysicsEntity? entity)
        {
            Block down = Game.World.GetBlock(x, y - 1, z, out _) ?? Block.Air;
            return down == requiredGround || down == this;
        }

        internal override void BlockUpdate(int x, int y, int z, uint data, BlockSide side)
        {
            if (side == BlockSide.Bottom)
            {
                Block below = Game.World.GetBlock(x, y - 1, z, out _) ?? Block.Air;

                if (below != requiredGround && below != this)
                {
                    ScheduleDestroy(x, y, z);
                }
            }
        }

        internal override void RandomUpdate(int x, int y, int z, uint data)
        {
            var age = (int)(data & 0b00_0111);

            if (age < 7)
            {
                Game.World.SetBlock(this, (uint)(age + 1), x, y, z);
            }
            else
            {
                if (Game.World.GetBlock(x, y + 1, z, out _)?.IsReplaceable ?? false)
                {
                    var height = 0;
                    for (var o = 0; o < maxHeight; o++)
                    {
                        if (Game.World.GetBlock(x, y - o, z, out _) == this)
                        {
                            height++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (height < maxHeight)
                    {
                        Place(x, y + 1, z);
                    }
                }
            }
        }
    }
}