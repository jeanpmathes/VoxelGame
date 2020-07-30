// <copyright file="GrowingBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Entities;
using VoxelGame.Logic.Interfaces;

namespace VoxelGame.Logic.Blocks
{
    /// <summary>
    /// A block that grows upwards and is destroyed if a certain ground block is not given.
    /// Data bit usage: <c>---aaa</c>
    /// </summary>
    // a = age
    public class GrowingBlock : BasicBlock, IFlammable
    {
        private protected readonly Block requiredGround;
        private protected readonly int maxHeight;

        public GrowingBlock(string name, string namedId, TextureLayout layout, Block ground, int maxHeight) :
            base(
                name,
                namedId,
                layout,
                isOpaque: true,
                renderFaceAtNonOpaques: true,
                isSolid: true,
                isInteractable: false)
        {
            requiredGround = ground;
            this.maxHeight = maxHeight;
        }

        protected override bool Place(PhysicsEntity? entity, int x, int y, int z)
        {
            Block down = Game.World.GetBlock(x, y - 1, z, out _) ?? Block.Air;

            if (down == requiredGround || down == this)
            {
                Game.World.SetBlock(this, 0, x, y, z);

                return true;
            }
            else
            {
                return false;
            }
        }

        internal override void BlockUpdate(int x, int y, int z, uint data, BlockSide side)
        {
            if (side == BlockSide.Bottom)
            {
                Block below = Game.World.GetBlock(x, y - 1, z, out _) ?? Block.Air;

                if (below != requiredGround && below != this)
                {
                    Destroy(x, y, z);
                }
            }
        }

        internal override void RandomUpdate(int x, int y, int z, uint data)
        {
            int age = (int)(data & 0b00_0111);

            if (age < 7)
            {
                Game.World.SetBlock(this, (uint)(age + 1), x, y, z);
            }
            else
            {
                if (Game.World.GetBlock(x, y + 1, z, out _)?.IsReplaceable ?? false)
                {
                    int height = 0;
                    for (int o = 0; o < maxHeight; o++)
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