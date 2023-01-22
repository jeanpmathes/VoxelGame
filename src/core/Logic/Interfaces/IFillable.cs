// <copyright file="IFillable.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;

namespace VoxelGame.Core.Logic.Interfaces;

/// <summary>
///     Allows blocks to be filled with fluid.
/// </summary>
public interface IFillable : IBlockBase
{
    /// <summary>
    ///     Whether the fluid filling this block should be rendered.
    /// </summary>
    bool RenderFluid => !IsSolidAndFull();

    /// <summary>
    ///     Check whether a given block at a given location allows inflow trough a certain side.
    /// </summary>
    /// <param name="world">The world this block is in.</param>
    /// <param name="position">The block position.</param>
    /// <param name="side">The side through which water would flow in.</param>
    /// <param name="fluid">The fluid that flows in.</param>
    /// <returns>Whether the fluid is allowed to flow in.</returns>
    bool AllowInflow(World world, Vector3i position, BlockSide side, Fluid fluid)
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
    bool AllowOutflow(World world, Vector3i position, BlockSide side)
    {
        return true;
    }

    /// <summary>
    ///     Called when new fluid flows into or out of this block.
    /// </summary>
    void FluidChange(World world, Vector3i position, Fluid fluid, FluidLevel level)
    {
        // Method intentionally left empty.
        // Fillable blocks do not have to react when the fluid amount changes.
    }

    /// <summary>
    ///     Call this after placement, to dispatch correct fluid change events.
    /// </summary>
    /// <param name="world">The world this block is in.</param>
    /// <param name="position">The block position.</param>
    public static void OnPlace(World world, Vector3i position)
    {
        Content? content = world.GetContent(position);

        if (content == null) return;

        (BlockInstance block, FluidInstance fluid) = content.Value;

        if (fluid.Fluid != Fluids.Instance.None && block.Block is IFillable fillable)
            fillable.FluidChange(world, position, fluid.Fluid, fluid.Level);
    }
}

