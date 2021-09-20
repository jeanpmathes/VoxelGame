// <copyright file="ScheduledTicks.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Updates;
using VoxelGame.Logging;

namespace VoxelGame.Core.Collections
{
    [Serializable]
    public class ScheduledTickManager<T> where T : ITickable
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<ScheduledTickManager<T>>();

        private readonly int maxTicks;
        private TicksHolder? nextTicks;
        [NonSerialized] private UpdateCounter updateCounter;

        [NonSerialized] private World world;

        public ScheduledTickManager(int maxTicks, World world, UpdateCounter updateCounter)
        {
            this.maxTicks = maxTicks;

            this.world = world;
            this.updateCounter = updateCounter;
        }

        public void Setup(World containingWorld, UpdateCounter counter)
        {
            world = containingWorld;
            updateCounter = counter;

            Load();
        }

        public void Add(T tick, int tickOffset)
        {
            TicksHolder ticks;

            do
            {
                long targetUpdate = updateCounter.CurrentUpdate + tickOffset;
                ticks = FindOrCreateTargetTick(targetUpdate);

                if (ticks.tickables.Count < maxTicks) break;

                logger.LogWarning(
                    "Tick for {Update} has been scheduled for following update as limit is reached",
                    targetUpdate);

                tickOffset += 1;

            } while (ticks.tickables.Count >= maxTicks);

            ticks.tickables.Add(tick);
        }

        private TicksHolder FindOrCreateTargetTick(long targetTick)
        {
            TicksHolder? last = null;
            TicksHolder? current = nextTicks;

            if (current == null)
            {
                nextTicks = new TicksHolder(targetTick);

                return nextTicks;
            }

            while (current != null)
            {
                if (current.targetUpdate == targetTick) return current;

                if (current.targetUpdate > targetTick)
                {
                    if (last == null)
                    {
                        nextTicks = new TicksHolder(targetTick) {next = current};

                        return nextTicks;
                    }

                    var newTicks = new TicksHolder(targetTick);
                    last.next = newTicks;
                    newTicks.next = current;

                    return newTicks;
                }

                last = current;
                current = current.next;
            }

            var newLastTicks = new TicksHolder(targetTick);
            last!.next = newLastTicks;

            return newLastTicks;
        }

        public void Process()
        {
            if (nextTicks != null && nextTicks.targetUpdate <= updateCounter.CurrentUpdate)
            {
                foreach (T scheduledTick in nextTicks.tickables) scheduledTick.Tick(world);

                nextTicks = nextTicks.next;
            }
        }

        /// <summary>
        ///     Subtracts the current update from all target updates so they are update-independent.
        /// </summary>
        public void Unload()
        {
            TicksHolder? current = nextTicks;

            while (current != null)
            {
                current.targetUpdate -= updateCounter.CurrentUpdate;
                current = current.next;
            }
        }

        /// <summary>
        ///     Adds the current update to all target updates so they will be called.
        /// </summary>
        public void Load()
        {
            TicksHolder? current = nextTicks;

            while (current != null)
            {
                current.targetUpdate += updateCounter.CurrentUpdate;
                current = current.next;
            }
        }

        [Serializable]
        private class TicksHolder
        {
            public TicksHolder? next;
            public long targetUpdate;
            public List<T> tickables = new();

            public TicksHolder(long targetUpdate)
            {
                this.targetUpdate = targetUpdate;
            }
        }
    }
}