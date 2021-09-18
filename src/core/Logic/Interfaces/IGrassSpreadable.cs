// <copyright file="IGrassSpreadable.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;

namespace VoxelGame.Core.Logic.Interfaces
{
    /// <summary>
    ///     Marks a block as able to have grass spread on it.
    /// </summary>
    internal interface IGrassSpreadable : IBlockBase
    {
        public bool SpreadGrass(World world, Vector3i position, Block grass)
        {
            Block above = world.GetBlock(position + Vector3i.UnitY, out _) ?? Block.Air;

            if (world.GetBlock(position, out _) != this || above.IsSolidAndFull && above.IsOpaque) return false;

            world.SetBlock(grass, data: 0, position);

            return false;
        }
    }
}