// <copyright file="FluidMeshInfo.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Information required to mesh a fluid.
/// </summary>
public readonly record struct FluidMeshInfo
{
    private FluidMeshInfo(BlockInstance block, FluidLevel level, BlockSide side, bool isStatic)
    {
        Block = block;
        Level = level;
        Side = side;
        IsStatic = isStatic;
    }

    /// <summary>
    ///     Get the current block.
    /// </summary>
    public BlockInstance Block { get; init; }

    /// <summary>
    ///     The level of the fluid.
    /// </summary>
    public FluidLevel Level { get; init; }

    /// <summary>
    ///     The side of the fluid that is being meshed.
    /// </summary>
    public BlockSide Side { get; init; }

    /// <summary>
    ///     Whether the fluid is static.
    /// </summary>
    public bool IsStatic { get; init; }

    /// <summary>
    ///     Create fluid meshing information.
    /// </summary>
    public static FluidMeshInfo Fluid(BlockInstance block, FluidLevel level, BlockSide side, bool isStatic)
    {
        return new FluidMeshInfo(block, level, side, isStatic);
    }
}
