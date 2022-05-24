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
    public BlockMeshInfo(BlockSide side, uint data, Fluid fluid)
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
}
