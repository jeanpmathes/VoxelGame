// <copyright file="BlockMeshInfo.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Provides information required to create a block mesh.
/// </summary>
public sealed class BlockMeshInfo
{
    private BlockMeshInfo(BlockSide side, uint data, Fluid fluid)
    {
        Side = side;
        Data = data;
        Fluid = fluid;
    }

    /// <summary>
    ///     The side that is meshed.
    /// </summary>
    public BlockSide Side { get; }

    /// <summary>
    ///     The data of the block.
    /// </summary>
    public uint Data { get; }

    /// <summary>
    ///     The fluid at the block position.
    /// </summary>
    public Fluid Fluid { get; }

    /// <summary>
    ///     Mesh info for a simple block.
    /// </summary>
    public static BlockMeshInfo Simple(BlockSide side, uint data, Fluid fluid)
    {
        return new BlockMeshInfo(side, data, fluid);
    }

    /// <summary>
    ///     Mesh info for a complex block.
    /// </summary>
    public static BlockMeshInfo Complex(uint data, Fluid fluid)
    {
        return new BlockMeshInfo(BlockSide.All, data, fluid);
    }

    /// <summary>
    ///     Mesh info for a cross plant.
    /// </summary>
    public static BlockMeshInfo CrossPlant(uint data, Fluid fluid)
    {
        return new BlockMeshInfo(BlockSide.All, data, fluid);
    }

    /// <summary>
    ///     Mesh info for a crop plant.
    /// </summary>
    public static BlockMeshInfo CropPlant(uint data, Fluid fluid)
    {
        return new BlockMeshInfo(BlockSide.All, data, fluid);
    }
}
