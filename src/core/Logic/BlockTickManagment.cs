// <copyright file="BlockTickManagment.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using VoxelGame.Core.Collections;

namespace VoxelGame.Core.Logic
{
    public partial class Block
    {
        public const int MaxLiquidTicksPerFrameAndChunk = 1024;
        private const int ScheduledDestroyOffset = 5;

        /// <summary>
        ///     Schedules a tick according to the given tick offset;
        /// </summary>
        /// <param name="world">The world in which the block is.</param>
        /// <param name="x">The x position of the block.</param>
        /// <param name="y">The y position of the block.</param>
        /// <param name="z">The z position of the block.</param>
        /// <param name="tickOffset">The offset in frames to when the block should be ticked.</param>
        protected void ScheduleTick(World world, int x, int y, int z, int tickOffset)
        {
            Chunk? chunk = world.GetChunkOfPosition(x, z);
            chunk?.ScheduleBlockTick(new BlockTick(x, y, z, this, TickOperation.Tick), tickOffset);
        }

        /// <summary>
        ///     Schedule the destruction of this block.
        /// </summary>
        /// <param name="world">The world in which the block is located.</param>
        /// <param name="x">The x position of the block to destroy.</param>
        /// <param name="y">The y position of the block to destroy.</param>
        /// <param name="z">The z position of the block to destroy.</param>
        protected void ScheduleDestroy(World world, int x, int y, int z)
        {
            Chunk? chunk = world.GetChunkOfPosition(x, z);
            chunk?.ScheduleBlockTick(new BlockTick(x, y, z, this, TickOperation.Destroy), ScheduledDestroyOffset);
        }

        internal void TickNow(World world, int x, int y, int z, uint data)
        {
            if (this == Air) return;

            ScheduledUpdate(world, x, y, z, data);
        }

        internal enum TickOperation
        {
            Tick,
            Destroy
        }

        [Serializable]
#pragma warning disable CA1815 // Override equals and operator equals on value types
        internal struct BlockTick : ITickable
#pragma warning restore CA1815 // Override equals and operator equals on value types
        {
            private readonly int x, y, z;
            private readonly uint target;
            private readonly TickOperation operation;

            public BlockTick(int x, int y, int z, Block target, TickOperation operation)
            {
                this.x = x;
                this.y = y;
                this.z = z;

                this.target = target.Id;

                this.operation = operation;
            }

            public void Tick(World world)
            {
                Block? block = world.GetBlock(x, y, z, out uint data);

                if (block?.Id == target)
                    switch (operation)
                    {
                        case TickOperation.Tick:
                            block.ScheduledUpdate(world, x, y, z, data);

                            break;

                        case TickOperation.Destroy:
                            block.Destroy(world, x, y, z);

                            break;
                    }
            }
        }
    }
}