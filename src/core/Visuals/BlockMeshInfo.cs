// <copyright file="BlockMeshInfo.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Provides information required to create a block mesh.
/// </summary>
public readonly struct BlockMeshInfo : IEquatable<BlockMeshInfo>
{
    /// <summary>
    ///     Create a new block mesh info.
    /// </summary>
    public BlockMeshInfo(BlockSide side, uint data, Fluid fluid)
    {
        Side = side;
        Data = data;
        Fluid = fluid;
    }

    /// <summary>
    ///     The side that is meshed.
    /// </summary>
    public BlockSide Side { get; init; }

    /// <summary>
    ///     The data of the block.
    /// </summary>
    public uint Data { get; init; }

    /// <summary>
    ///     The fluid at the block position.
    /// </summary>
    public Fluid Fluid { get; init; }

    /// <inheritdoc />
    public bool Equals(BlockMeshInfo other)
    {
        return Side == other.Side && Data == other.Data && Fluid.Equals(other.Fluid);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is BlockMeshInfo other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine((int) Side, Data, Fluid);
    }

    /// <summary>
    ///     The equality operator.
    /// </summary>
    public static bool operator ==(BlockMeshInfo left, BlockMeshInfo right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     The inequality operator.
    /// </summary>
    public static bool operator !=(BlockMeshInfo left, BlockMeshInfo right)
    {
        return !left.Equals(right);
    }
}
