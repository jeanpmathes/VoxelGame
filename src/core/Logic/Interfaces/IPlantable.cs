// <copyright file="IPlantable.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
namespace VoxelGame.Core.Logic.Interfaces
{
    /// <summary>
    /// Mark a block as able to support plant growth.
    /// </summary>
    public interface IPlantable : IBlockBase
    {
        bool SupportsFullGrowth => false;

        public bool TryGrow(World world, int x, int y, int z, Liquid liquid, LiquidLevel level)
        {
            return liquid.TryTakeExact(world, x, y, z, level);
        }
    }
}