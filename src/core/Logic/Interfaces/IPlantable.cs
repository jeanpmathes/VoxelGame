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
        bool SupportsFullGrowth { get => false; }

        public bool TryGrow(int x, int y, int z, Liquid liquid, LiquidLevel level)
        {
            return liquid.TryTakeExact(x, y, z, level);
        }
    }
}