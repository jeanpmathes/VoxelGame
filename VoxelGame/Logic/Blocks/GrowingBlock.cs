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
#pragma warning disable CA1051 // Do not declare visible instance fields
        protected readonly Block requiredGround;
        protected readonly int maxHeight;
#pragma warning restore CA1051 // Do not declare visible instance fields

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

        public override bool Place(int x, int y, int z, PhysicsEntity entity)
        {
            Block down = Game.World.GetBlock(x, y - 1, z, out _);

            if (down == requiredGround || down == this)
            {
                return base.Place(x, y, z, entity);
            }
            else
            {
                return false;
            }
        }

        public override void BlockUpdate(int x, int y, int z, byte data)
        {
            Block down = Game.World.GetBlock(x, y - 1, z, out _);

            if (down != requiredGround && down != this)
            {
                Destroy(x, y, z, null);
            }
        }

        public override void RandomUpdate(int x, int y, int z, byte data)
        {
            int age = data & 0b0_0111;

            if (age < 7)
            {
                Game.World.SetBlock(this, (byte)(age + 1), x, y, z);
            }
            else
            {
                if (Game.World.GetBlock(x, y + 1, z, out _).IsReplaceable)
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