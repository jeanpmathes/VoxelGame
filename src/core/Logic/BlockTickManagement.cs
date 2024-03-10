// <copyright file="BlockTickManagement.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Serialization;

namespace VoxelGame.Core.Logic;

public partial class Block
{
    private const int ScheduledDestroyOffset = 5;

    /// <summary>
    ///     The maximum amount of block ticks that should be processed per frame.
    /// </summary>
    public static int MaxBlockTicksPerFrameAndChunk => 1024;

    /// <summary>
    ///     Schedules a tick according to the given tick offset.
    ///     Note that the system does not guarantee that the tick will be executed exactly at the given offset, as chunks could
    ///     be inactive.
    /// </summary>
    /// <param name="world">The world in which the block is.</param>
    /// <param name="position">The position of the block a tick should be scheduled for.</param>
    /// <param name="tickOffset">The offset in frames to when the block should be ticked. Must be greater than 0.</param>
    protected void ScheduleTick(World world, Vector3i position, uint tickOffset)
    {
        Chunk? chunk = world.GetActiveChunk(position);
        chunk?.ScheduleBlockTick(new BlockTick(position, this, TickOperation.Tick), tickOffset);
    }

    /// <summary>
    ///     Schedule the destruction of this block.
    /// </summary>
    /// <param name="world">The world in which the block is located.</param>
    /// <param name="position">The position of the block that will be scheduled to be destroyed.</param>
    protected void ScheduleDestroy(World world, Vector3i position)
    {
        Chunk? chunk = world.GetActiveChunk(position);
        chunk?.ScheduleBlockTick(new BlockTick(position, this, TickOperation.Destroy), ScheduledDestroyOffset);
    }

    internal void TickNow(World world, Vector3i position, uint data)
    {
        if (this == Blocks.Instance.Air) return;

        ScheduledUpdate(world, position, data);
    }

    internal enum TickOperation
    {
        Tick,
        Destroy
    }

    internal struct BlockTick(Vector3i position, IBlockBase target, TickOperation operation) : ITickable, IEquatable<BlockTick>
    {
        private int x = position.X;
        private int y = position.Y;
        private int z = position.Z;

        private uint target = target.ID;
        private TickOperation operation = operation;

        public void Tick(World world)
        {
            BlockInstance? potentialBlock = world.GetBlock((x, y, z));

            if (potentialBlock is not {} block) return;
            if (block.Block.ID != target) return;

            switch (operation)
            {
                case TickOperation.Tick:
                    block.Block.ScheduledUpdate(world, (x, y, z), block.Data);

                    break;

                case TickOperation.Destroy:
                    block.Block.Destroy(world, (x, y, z));

                    break;

                default: throw new InvalidOperationException();
            }
        }

        public void Serialize(Serializer serializer)
        {
            serializer.Serialize(ref x);
            serializer.Serialize(ref y);
            serializer.Serialize(ref z);
            serializer.Serialize(ref target);
            serializer.Serialize(ref operation);
        }

        public bool Equals(BlockTick other)
        {
            return (x, y, z, target, operation) == (other.x, other.y, other.z, other.target, other.operation);
        }

        public override bool Equals(object? obj)
        {
            return obj is BlockTick other && Equals(other);
        }

#pragma warning disable S2328
        public override int GetHashCode()
        {
            return HashCode.Combine(x, y, z, target, (int) operation);
        }
#pragma warning restore S2328

        public static bool operator ==(BlockTick left, BlockTick right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BlockTick left, BlockTick right)
        {
            return !(left == right);
        }
    }
}
