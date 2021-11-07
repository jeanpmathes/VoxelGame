// <copyright file="IGrassSpreadable.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Interfaces
{
    /// <summary>
    ///     Marks a block as able to have grass spread on it.
    /// </summary>
    internal interface IGrassSpreadable : IBlockBase
    {
        public bool SpreadGrass(World world, Vector3i position, Block grass)
        {
            if (world.GetBlock(position)?.Block != this || world.HasOpaqueTop(position)) return false;

            world.SetBlock(grass.AsInstance(), position);

            return false;
        }
    }
}