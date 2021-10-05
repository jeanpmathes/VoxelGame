// <copyright file="BlockTickManagment.cs" company="VoxelGame">
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
        public const int MaxLiquidTicksPerFrameAndChunk = 1024;
        private const int ScheduledDestroyOffset = 5;

        /// <summary>
        ///     Schedules a tick according to the given tick offset;
        /// </summary>
        /// <param name="world">The world in which the block is.</param>
        /// <param name="position">The position of the block a tick should be scheduled for.</param>
        /// <param name="tickOffset">The offset in frames to when the block should be ticked.</param>
        protected void ScheduleTick(World world, Vector3i position, int tickOffset)
        {
            Chunk? chunk = world.GetChunkOfPosition(position);
            chunk?.ScheduleBlockTick(new BlockTick(position, this, TickOperation.Tick), tickOffset);
        }

        /// <summary>
        ///     Schedule the destruction of this block.
        /// </summary>
        /// <param name="world">The world in which the block is located.</param>
        /// <param name="position">The position of the block that will be scheduled to be destroyed.</param>
        protected void ScheduleDestroy(World world, Vector3i position)
        {
            Chunk? chunk = world.GetChunkOfPosition(position);
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
#pragma warning disable CA1815 // Override equals and operator equals on value types
        internal struct BlockTick : ITickable
#pragma warning restore CA1815 // Override equals and operator equals on value types
        {
            private readonly int x, y, z;
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
                Block? block = world.GetBlock((x, y, z), out uint data);

                if (block?.Id == target)
                    switch (operation)
                    {
                        case TickOperation.Tick:
                            block.ScheduledUpdate(world, (x, y, z), data);

                            break;

                        case TickOperation.Destroy:
                            block.Destroy(world, (x, y, z));

                            break;
                    }
            }
        }
    }
}