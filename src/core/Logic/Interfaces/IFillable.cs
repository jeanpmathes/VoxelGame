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
    }
}