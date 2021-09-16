// <copyright file="IPlantable.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;

namespace VoxelGame.Core.Logic.Interfaces
{
    /// <summary>
    ///     Mark a block as able to support plant growth.
    /// </summary>
    public interface IPlantable : IBlockBase
    {
        bool SupportsFullGrowth => false;

        public bool TryGrow(World world, Vector3i position, Liquid liquid, LiquidLevel level)
        {
            return liquid.TryTakeExact(world, position, level);
        }
    }
}