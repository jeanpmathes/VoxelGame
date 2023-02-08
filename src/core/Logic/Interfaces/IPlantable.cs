// <copyright file="IPlantable.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;

namespace VoxelGame.Core.Logic.Interfaces;

/// <summary>
///     Mark a block as able to support plant growth.
/// </summary>
public interface IPlantable : IBlockBase
{
    /// <summary>
    ///     Whether this block supports full plant growth.
    /// </summary>
    bool SupportsFullGrowth => false;

    /// <summary>
    ///     Try to grow a plant on this block.
    /// </summary>
    /// <param name="world">The world in which the operation takes place.</param>
    /// <param name="position">The position of the block.</param>
    /// <param name="fluid">The fluid that is required by the plant.</param>
    /// <param name="level">The amount of fluid required by the plant.</param>
    /// <returns>True if enough fluid was available.</returns>
    public bool TryGrow(World world, Vector3i position, Fluid fluid, FluidLevel level)
    {
        return fluid.TryTakeExact(world, position, level);
    }
}

