// <copyright file="ConnectingBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A base class for different blocks that connect to other blocks. This class handles placement and updates.
///     Data bit usage: <c>--nesw</c>
/// </summary>
/// <typeparam name="TConnectable">The connection interface.</typeparam>
// n: connected north
// e: connected east
// s: connected south
// w: connected west
public class ConnectingBlock<TConnectable> : Block, IFillable where TConnectable : IConnectable
{
    /// <summary>
    ///     Create a new connecting block.
    /// </summary>
    /// <param name="name">The name of the blocks.</param>
    /// <param name="namedID">The string ID of the block.</param>
    /// <param name="flags">The flags describing the block.</param>
    /// <param name="boundingVolume">The block bounding box.</param>
    protected ConnectingBlock(String name, String namedID, BlockFlags flags, BoundingVolume boundingVolume) :
        base(
            name,
            namedID,
            flags,
            boundingVolume) {}

    /// <inheritdoc />
    protected override void DoPlace(World world, Vector3i position, PhysicsActor? actor)
    {
        world.SetBlock(this.AsInstance(IConnectable.GetConnectionData<TConnectable>(world, position)), position);
    }

    /// <inheritdoc />
    public override void NeighborUpdate(World world, Vector3i position, UInt32 data, Side side)
    {
        UInt32 newData = data;

        if (side.IsLateral())
            newData = CheckNeighbor(side.Offset(position), side.Opposite(), side.ToOrientation().ToFlag(), newData);

        if (newData != data) world.SetBlock(this.AsInstance(newData), position);

        UInt32 CheckNeighbor(Vector3i neighborPosition, Side neighborSide, UInt32 mask, UInt32 oldData)
        {
            if (world.GetBlock(neighborPosition)?.Block is TConnectable neighbor &&
                neighbor.IsConnectable(world, neighborSide, neighborPosition)) oldData |= mask;
            else oldData &= ~mask;

            return oldData;
        }
    }
}
