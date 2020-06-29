// <copyright file="GrowingBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Entities;

namespace VoxelGame.Logic.Blocks
{
    /// <summary>
    /// A block that grows upwards and is destroyed if a certain ground block is not given.
    /// Data bit usage: <c>--aaa</c>
    /// </summary>
    // a = age
    public class GrowingBlock : BasicBlock
    {
        private protected readonly Block requiredGround;
        private protected readonly int maxHeight;

        public GrowingBlock(string name, TextureLayout layout, Block ground, int maxHeight) :
            base(
                name,
                layout,
                isOpaque: true,
                renderFaceAtNonOpaques: true,
                isSolid: true)
        {
            requiredGround = ground;
            this.maxHeight = maxHeight;
        }

        protected override bool Place(int x, int y, int z, bool? replaceable, PhysicsEntity? entity)
        {
            Block down = Game.World.GetBlock(x, y - 1, z, out _) ?? Block.AIR;

            if (replaceable == true && (down == requiredGround || down == this))
            {
                Game.World.SetBlock(this, 0, x, y, z);

                return true;
            }
            else
            {
                return false;
            }
        }

        internal override void BlockUpdate(int x, int y, int z, byte data)
        {
            Block down = Game.World.GetBlock(x, y - 1, z, out _) ?? Block.AIR;

            if (down != requiredGround && down != this)
            {
                Destroy(x, y, z, null);
            }
        }

        internal override void RandomUpdate(int x, int y, int z, byte data)
        {
            int age = data & 0b0_0111;

            if (age < 7)
            {
                Game.World.SetBlock(this, (byte)(age + 1), x, y, z);
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
                        Place(x, y + 1, z, null);
                    }
                }
            }
        }
    }
}