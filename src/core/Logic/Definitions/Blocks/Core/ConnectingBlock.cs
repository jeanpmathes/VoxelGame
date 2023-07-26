// <copyright file="ConnectingBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Entities;
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
    /// <param name="namedId">The string ID of the block.</param>
    /// <param name="flags">The flags describing the block.</param>
    /// <param name="boundingVolume">The block bounding box.</param>
    protected ConnectingBlock(string name, string namedId, BlockFlags flags, BoundingVolume boundingVolume) :
        base(
            name,
            namedId,
            flags,
            boundingVolume) {}

    /// <inheritdoc />
    protected override void DoPlace(World world, Vector3i position, PhysicsEntity? entity)
    {
        world.SetBlock(this.AsInstance(IConnectable.GetConnectionData<TConnectable>(world, position)), position);
    }

    /// <inheritdoc />
    public override void NeighborUpdate(World world, Vector3i position, uint data, BlockSide side)
    {
        uint newData = data;

        if (side.IsLateral())
            newData = CheckNeighbor(side.Offset(position), side.Opposite(), side.ToOrientation().ToFlag(), newData);

        if (newData != data) world.SetBlock(this.AsInstance(newData), position);

        uint CheckNeighbor(Vector3i neighborPosition, BlockSide neighborSide, uint mask, uint oldData)
        {
            if (world.GetBlock(neighborPosition)?.Block is TConnectable neighbor &&
                neighbor.IsConnectable(world, neighborSide, neighborPosition)) oldData |= mask;
            else oldData &= ~mask;

            return oldData;
        }
    }
}
