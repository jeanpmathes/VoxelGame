// <copyright file="IFillable.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
namespace VoxelGame.Core.Logic.Interfaces
{
    public interface IFillable : IBlockBase
    {
        bool RenderLiquid { get => !IsSolidAndFull; }

        bool IsFillable(int x, int y, int z, Liquid liquid)
        {
            return true;
        }

        /// <summary>
        /// Called when new liquid flows into or out of this block.
        /// </summary>
        void LiquidChange(int x, int y, int z, Liquid liquid, LiquidLevel level)
        {
            // Method intentionally left empty.
            // IFillables do not have to react when the liquid amount changes.
        }
    }
}