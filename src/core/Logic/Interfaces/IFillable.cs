// <copyright file="IFillable.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
namespace VoxelGame.Core.Logic.Interfaces
{
    public interface IFillable : IBlockBase
    {
        bool RenderLiquid => !IsSolidAndFull;

        /// <summary>
        /// Check whether a given block at a given location allows inflow trough a certain side.
        /// </summary>
        /// <param name="world">The world this block is in.</param>
        /// <param name="x">The x position of the block.</param>
        /// <param name="y">The y position of the block.</param>
        /// <param name="z">The z position of the block.</param>
        /// <param name="side">The side through which water would flow in.</param>
        /// <param name="liquid">The liquid that flows in.</param>
        /// <returns>Whether the liquid is allowed to flow in.</returns>
        bool AllowInflow(World world, int x, int y, int z, BlockSide side, Liquid liquid)
        {
            return true;
        }

        /// <summary>
        /// Check whether a given block at a given position allows outflow through a certain side.
        /// </summary>
        /// <param name="world">The world this block is in.</param>
        /// <param name="x">The x position of the block.</param>
        /// <param name="y">The y position of the block.</param>
        /// <param name="z">The z position of the block.</param>
        /// <param name="side">The side through which the liquid wants to flow.</param>
        /// <returns>true if outflow is allowed.</returns>
        bool AllowOutflow(World world, int x, int y, int z, BlockSide side)
        {
            return true;
        }

        /// <summary>
        /// Called when new liquid flows into or out of this block.
        /// </summary>
        void LiquidChange(World world, int x, int y, int z, Liquid liquid, LiquidLevel level)
        {
            // Method intentionally left empty.
            // Fillable blocks do not have to react when the liquid amount changes.
        }
    }
}