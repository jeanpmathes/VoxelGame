// <copyright file="GrassBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
namespace VoxelGame.Logic.Blocks
{
    /// <summary>
    /// A block that changes into dirt when something is placed on top of it.
    /// </summary>
    public class CoveredDirtBlock : BasicBlock
    {
        public CoveredDirtBlock(string name, TextureLayout layout) : base(
            name,
            layout,
            isOpaque: true,
            renderFaceAtNonOpaques: true,
            isSolid: true)
        {
        }

        public override void BlockUpdate(int x, int y, int z, byte data)
        {
            // Check block on top of this block
            Block above = Game.World.GetBlock(x, y + 1, z, out _);

            if (above.IsSolid && above.IsFull)
            {
                Game.World.SetBlock(Block.DIRT, 0, x, y, z);
            }
        }
    }
}
