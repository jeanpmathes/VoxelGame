// <copyright file="ConnectingBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// A base class for different blocks that connect to other blocks. This class handles placement and updates.
    /// Data bit usage: <c>--nesw</c>
    /// </summary>
    /// <typeparam name="TConnectable">The connection interface.</typeparam>
    // n = connected north
    // e = connected east
    // s = connected south
    // w = connected west
    public abstract class ConnectingBlock<TConnectable> : Block, IFillable where TConnectable : IConnectable
    {
        protected ConnectingBlock(string name, string namedId, bool isFull, bool isOpaque, bool renderFaceAtNonOpaques,
            bool isSolid, bool receiveCollisions, bool isTrigger, bool isReplaceable, bool isInteractable,
            BoundingBox boundingBox, TargetBuffer targetBuffer) :
            base(
                name,
                namedId,
                isFull,
                isOpaque,
                renderFaceAtNonOpaques,
                isSolid,
                receiveCollisions,
                isTrigger,
                isReplaceable,
                isInteractable,
                boundingBox,
                targetBuffer) {}

        protected override void DoPlace(World world, int x, int y, int z, PhysicsEntity? entity)
        {
            world.SetBlock(this, IConnectable.GetConnectionData<TConnectable>(world, x, y, z), x, y, z);
        }

        internal override void BlockUpdate(World world, int x, int y, int z, uint data, BlockSide side)
        {
            uint newData = data;

            newData = side switch
            {
                BlockSide.Back => CheckNeighbor(x, y, z - 1, BlockSide.Front, 0b00_1000, newData),
                BlockSide.Right => CheckNeighbor(x + 1, y, z, BlockSide.Left, 0b00_0100, newData),
                BlockSide.Front => CheckNeighbor(x, y, z + 1, BlockSide.Back, 0b00_0010, newData),
                BlockSide.Left => CheckNeighbor(x - 1, y, z, BlockSide.Right, 0b00_0001, newData),
                _ => newData
            };

            if (newData != data)
            {
                world.SetBlock(this, newData, x, y, z);
            }

            uint CheckNeighbor(int nx, int ny, int nz, BlockSide neighborSide, uint mask, uint oldData)
            {
                if (world.GetBlock(nx, ny, nz, out _) is TConnectable neighbor &&
                    neighbor.IsConnectable(world, neighborSide, nx, ny, nz))
                {
                    oldData |= mask;
                }
                else
                {
                    oldData &= ~mask;
                }

                return oldData;
            }
        }
    }
}