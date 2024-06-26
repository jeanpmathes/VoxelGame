﻿using System;
using OpenTK.Mathematics;

namespace VoxelGame.Core.Logic.Interfaces;

/// <summary>
///     Allows a block to connect to pipes.
/// </summary>
public interface IPipeConnectable : IBlockBase
{
    /// <summary>
    ///     Checks if this block supports connection at a specific side.
    /// </summary>
    /// <param name="world">The world this block is in.</param>
    /// <param name="side">The side to check for connect-ability.</param>
    /// <param name="position">The position of the block to check.</param>
    /// <returns>True if connection is supported; false if not.</returns>
    public Boolean IsConnectable(World world, BlockSide side, Vector3i position)
    {
        return true;
    }
}
