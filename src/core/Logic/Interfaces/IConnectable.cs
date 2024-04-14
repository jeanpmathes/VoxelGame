// <copyright file="IConnectable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Interfaces;

/// <summary>
///     Allows connection of blocks.
/// </summary>
public interface IConnectable : IBlockBase
{
    /// <summary>
    ///     Checks if this block supports connection at a specific side.
    /// </summary>
    /// <param name="world">The world this block is in.</param>
    /// <param name="side">The side to check for connect-ability. Only front, back, left and right are valid.</param>
    /// <param name="position">The block position.</param>
    /// <returns>True if connection is supported; false if not.</returns>
    public Boolean IsConnectable(World world, BlockSide side, Vector3i position)
    {
        return true;
    }

    /// <summary>
    ///     Get the data about connectables around a block. The data is packed in an uint using four bits.
    /// </summary>
    /// <param name="world">The world the block is in.</param>
    /// <param name="position">The world position of the block.</param>
    /// <returns>The connection data.</returns>
    public static UInt32 GetConnectionData<TConnectable>(World world, Vector3i position)
        where TConnectable : IConnectable
    {
        UInt32 data = 0;

        foreach (Orientation orientation in Orientations.All)
        {
            Vector3i neighborPosition = orientation.Offset(position);

            if (world.GetBlock(neighborPosition)?.Block is TConnectable connectable &&
                connectable.IsConnectable(world, orientation.ToBlockSide().Opposite(), neighborPosition))
                data |= orientation.ToFlag();
        }

        return data;
    }
}
