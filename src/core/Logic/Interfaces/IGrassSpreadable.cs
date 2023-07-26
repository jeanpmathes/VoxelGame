// <copyright file="IGrassSpreadable.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Interfaces;

/// <summary>
///     Marks a block as able to have grass spread on it.
/// </summary>
public interface IGrassSpreadable : IBlockBase
{
    /// <summary>
    ///     Spreads grass on the block. This operation does not always succeed.
    /// </summary>
    /// <param name="world">The world the block is in.</param>
    /// <param name="position">The position of the block.</param>
    /// <param name="grass">The grass block that is spreading.</param>
    /// <returns>True when the grass block successfully spread.</returns>
    public bool SpreadGrass(World world, Vector3i position, Block grass)
    {
        if (world.GetBlock(position)?.Block != this || world.HasOpaqueTop(position) != false) return false;

        world.SetBlock(grass.AsInstance(), position);

        return true;
    }
}
