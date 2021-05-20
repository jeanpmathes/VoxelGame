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

        internal override bool CanPlace(World world, int x, int y, int z, PhysicsEntity? entity)
        {
            Block down = world.GetBlock(x, y - 1, z, out _) ?? Block.Air;
            return down == requiredGround || down == this;
        }

        internal override void BlockUpdate(World world, int x, int y, int z, uint data, BlockSide side)
        {
            if (side == BlockSide.Bottom)
            {
                Block below = world.GetBlock(x, y - 1, z, out _) ?? Block.Air;

                if (below != requiredGround && below != this)
                {
                    ScheduleDestroy(world, x, y, z);
                }
            }
        }

        internal override void RandomUpdate(World world, int x, int y, int z, uint data)
        {
            var age = (int)(data & 0b00_0111);

            if (age < 7)
            {
                world.SetBlock(this, (uint)(age + 1), x, y, z);
            }
            else
            {
                if (world.GetBlock(x, y + 1, z, out _)?.IsReplaceable ?? false)
                {
                    var height = 0;
                    for (var o = 0; o < maxHeight; o++)
                    {
                        if (world.GetBlock(x, y - o, z, out _) == this)
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
                        Place(world, x, y + 1, z);
                    }
                }
            }
        }
    }
}