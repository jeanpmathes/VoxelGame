﻿// <copyright file="IConnectable.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Interfaces
{
    public interface IConnectable : IBlockBase
    {
        /// <summary>
        ///     Checks if this block supports connection at a specific side.
        /// </summary>
        /// <param name="world">The world this block is in.</param>
        /// <param name="side">The side to check for connect-ability. Only front, back, left and right are valid.</param>
        /// <param name="position">The block position.</param>
        /// <returns>True if connection is supported; false if not.</returns>
        public bool IsConnectable(World world, BlockSide side, Vector3i position)
        {
            return true;
        }

        /// <summary>
        ///     Get the data about connectables around a block. The data is packed in an uint using four bits.
        /// </summary>
        /// <param name="world">The world the block is in.</param>
        /// <param name="position">The world position of the block.</param>
        /// <returns>The connection data.</returns>
        public static uint GetConnectionData<TConnectable>(World world, Vector3i position)
            where TConnectable : IConnectable
        {
            uint data = 0;

            for (var orientation = Orientation.North; orientation <= Orientation.West; orientation++)
            {
                Vector3i neighborPosition = orientation.Offset(position);

                if (world.GetBlock(neighborPosition, out _) is TConnectable connectable &&
                    connectable.IsConnectable(world, orientation.ToBlockSide().Opposite(), neighborPosition))
                    data |= orientation.ToFlag();
            }

            return data;
        }
    }
}