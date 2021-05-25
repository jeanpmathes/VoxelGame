// <copyright file="IGrassSpreadable.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
namespace VoxelGame.Core.Logic.Interfaces
{
    /// <summary>
    /// Marks a block as able to have grass spread on it.
    /// </summary>
    internal interface IGrassSpreadable : IBlockBase
    {
        public bool SpreadGrass(World world, int x, int y, int z, Block grass)
        {
            Block above = world.GetBlock(x, y + 1, z, out _) ?? Block.Air;

            if (world.GetBlock(x, y, z, out _) != this || (above.IsSolidAndFull && above.IsOpaque))
            {
                return false;
            }

            world.SetBlock(grass, 0, x, y, z);

            return false;
        }
    }
}