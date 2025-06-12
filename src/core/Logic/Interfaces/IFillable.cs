// <copyright file="IFillable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Elements.Legacy;

namespace VoxelGame.Core.Logic.Interfaces;

/// <summary>
///     Allows blocks to be filled with fluid.
/// </summary>
public interface IFillable : IBlockBase
{
    /// <summary>
    ///     Whether the fluid filling this block should be rendered.
    /// </summary>
    Boolean IsFluidRendered => !IsSolidAndFull();

    /// <summary>
    ///     Check whether a given block at a given location allows inflow trough a certain side.
    /// </summary>
    /// <param name="world">The world this block is in.</param>
    /// <param name="position">The block position.</param>
    /// <param name="side">The side through which water would flow in.</param>
    /// <param name="fluid">The fluid that flows in.</param>
    /// <returns>Whether the fluid is allowed to flow in.</returns>
    Boolean IsInflowAllowed(World world, Vector3i position, Side side, Fluid fluid)
    {
        return true;
    }

    /// <summary>
    ///     Check whether a given block at a given position allows outflow through a certain side.
    /// </summary>
    /// <param name="world">The world this block is in.</param>
    /// <param name="position">The block position.</param>
    /// <param name="side">The side through which the fluid wants to flow.</param>
    /// <returns>true if outflow is allowed.</returns>
    Boolean IsOutflowAllowed(World world, Vector3i position, Side side)
    {
        return true;
    }
}
