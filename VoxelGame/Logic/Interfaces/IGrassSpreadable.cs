// <copyright file="IGrassSpreadable.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
namespace VoxelGame.Logic.Interfaces
{
    internal interface IGrassSpreadable : IBlockBase
    {
        public bool SpreadGrass(int x, int y, int z, Block grass)
        {
            if (Game.World.GetBlock(x, y, z, out _) != this || (Game.World.GetBlock(x, y + 1, z, out _) ?? Block.Air).IsSolidAndFull)
            {
                return false;
            }

            Game.World.SetBlock(grass, 0, x, y, z);

            return false;
        }
    }
}