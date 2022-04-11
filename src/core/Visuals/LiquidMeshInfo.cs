// <copyright file="FluidMeshInfo.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Information required to mesh a fluid.
/// </summary>
public sealed class FluidMeshInfo
{
    private FluidMeshInfo(FluidLevel level, BlockSide side, bool isStatic)
    {
        Level = level;
        Side = side;
        IsStatic = isStatic;
    }

    /// <summary>
    ///     The level of the fluid.
    /// </summary>
    public FluidLevel Level { get; }

    /// <summary>
    ///     The side of the fluid that is being meshed.
    /// </summary>
    public BlockSide Side { get; }

    /// <summary>
    ///     Whether the fluid is static.
    /// </summary>
    public bool IsStatic { get; }

    /// <summary>
    ///     Create fluid meshing information.
    /// </summary>
    public static FluidMeshInfo Fluid(FluidLevel level, BlockSide side, bool isStatic)
    {
        return new FluidMeshInfo(level, side, isStatic);
    }
}
