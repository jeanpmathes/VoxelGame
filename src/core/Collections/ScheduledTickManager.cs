// <copyright file="ScheduledTicks.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Collections
{
    [Serializable]
    public class ScheduledTickManager<T> where T : ITickable
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<ScheduledTickManager<T>>();

        private readonly int maxTicks;
        private TicksHolder? nextTicks;

        [NonSerialized] private UpdateCounter updateCounter;

        public ScheduledTickManager(int maxTicks, UpdateCounter updateCounter)
        {
            this.maxTicks = maxTicks;
            this.updateCounter = updateCounter;
        }

        public void Setup(UpdateCounter counter)
        {
            updateCounter = counter;

            Load();
        }

        public void Add(T tick, int tickOffset)
        {
            long targetUpdate = updateCounter.CurrentUpdate + tickOffset;
            TicksHolder ticks = FindOrCreateTargetTick(targetUpdate);

            if (ticks.tickables.Count >= maxTicks)
            {
                logger.LogWarning("For update {update} a tick was scheduled although the limit for this update is already reached. It has been scheduled for the following update.", targetUpdate);
                Add(tick, tickOffset + 1);
            }
            else
            {
                ticks.tickables.Add(tick);
            }
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
                if (current.targetUpdate == targetTick)
                {
                    return current;
                }

                if (current.targetUpdate > targetTick)
                {
                    if (last == null)
                    {
                        nextTicks = new TicksHolder(targetTick) { next = current };

                        return nextTicks;
                    }
                    else
                    {
                        var newTicks = new TicksHolder(targetTick);
                        last.next = newTicks;
                        newTicks.next = current;

                        return newTicks;
                    }
                }

                last = current;
                current = current.next;
            }

            var newLastTicks = new TicksHolder(targetTick);
            last!.next = newLastTicks;

            return newLastTicks;
        }

        public void Process(World world)
        {
            if (nextTicks != null && nextTicks.targetUpdate <= updateCounter.CurrentUpdate)
            {
                foreach (T scheduledTick in nextTicks.tickables)
                {
                    scheduledTick.Tick(world);
                }

                nextTicks = nextTicks.next;
            }
        }

        /// <summary>
        /// Subtracts the current update from all target updates so they are update-independent.
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
        /// Adds the current update to all target updates so they will be called.
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
            public long targetUpdate;
            public TicksHolder? next;
            public List<T> tickables = new List<T>();

            public TicksHolder(long targetUpdate)
            {
                this.targetUpdate = targetUpdate;
            }
        }
    }
}