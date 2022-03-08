// <copyright file="BlockTickManagement.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenToolkit.Mathematics;
using VoxelGame.Core.Collections;

namespace VoxelGame.Core.Logic
{
    public partial class Block
    {
        private const int ScheduledDestroyOffset = 5;

        /// <summary>
        ///     The maximum amount of block ticks that should be processed per frame.
        /// </summary>
        public static int MaxBlockTicksPerFrameAndChunk => 1024;

        /// <summary>
        ///     Schedules a tick according to the given tick offset;
        /// </summary>
        /// <param name="world">The world in which the block is.</param>
        /// <param name="position">The position of the block a tick should be scheduled for.</param>
        /// <param name="tickOffset">The offset in frames to when the block should be ticked.</param>
        protected void ScheduleTick(World world, Vector3i position, int tickOffset)
        {
            Chunk? chunk = world.GetChunkWithPosition(position);
            chunk?.ScheduleBlockTick(new BlockTick(position, this, TickOperation.Tick), tickOffset);
        }

        /// <summary>
        ///     Schedule the destruction of this block.
        /// </summary>
        /// <param name="world">The world in which the block is located.</param>
        /// <param name="position">The position of the block that will be scheduled to be destroyed.</param>
        protected void ScheduleDestroy(World world, Vector3i position)
        {
            Chunk? chunk = world.GetChunkWithPosition(position);
            chunk?.ScheduleBlockTick(new BlockTick(position, this, TickOperation.Destroy), ScheduledDestroyOffset);
        }

        internal void TickNow(World world, Vector3i position, uint data)
        {
            if (this == Air) return;

            ScheduledUpdate(world, position, data);
        }

        internal enum TickOperation
        {
            Tick,
            Destroy
        }

        [Serializable]
        internal struct BlockTick : ITickable, IEquatable<BlockTick>
        {
            private readonly int x;
            private readonly int y;
            private readonly int z;

            private readonly uint target;
            private readonly TickOperation operation;

            public BlockTick(Vector3i position, Block target, TickOperation operation)
            {
                x = position.X;
                y = position.Y;
                z = position.Z;

                this.target = target.Id;

                this.operation = operation;
            }

            public void Tick(World world)
            {
                BlockInstance? block = world.GetBlock((x, y, z));

                if (block?.Block.Id != target) return;

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

            public bool Equals(BlockTick other)
            {
                return (x, y, z, target, operation) == (other.x, other.y, other.z, other.target, other.operation);
            }

            public override bool Equals(object? obj)
            {
                return obj is BlockTick other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(x, y, z, target, (int) operation);
            }

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
}
