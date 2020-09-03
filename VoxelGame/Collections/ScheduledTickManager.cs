// <copyright file="ScheduledTicks.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace VoxelGame.Collections
{
    [Serializable]
    public class ScheduledTickManager<T> where T : ITickable
    {
        private static readonly ILogger logger = Program.CreateLogger<ScheduledTickManager<T>>();

        private readonly int maxTicks;
        private TicksHolder? nextTicks;

        public ScheduledTickManager(int maxTicks)
        {
            this.maxTicks = maxTicks;
        }

        public void Add(T tick, int tickOffset)
        {
            TicksHolder ticks = FindOrCreateTargetTick(Game.CurrentUpdate + tickOffset);

            if (ticks.tickables.Count >= maxTicks)
            {
                logger.LogWarning("For update {update} a tick was scheduled although the limit for this update is already reached. It has been scheduled for the following update.");
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
                        TicksHolder? newTicks = new TicksHolder(targetTick);
                        last.next = newTicks;
                        newTicks.next = current;

                        return newTicks;
                    }
                }

                last = current;
                current = current.next;
            }

            TicksHolder? newLastTicks = new TicksHolder(targetTick);
            last!.next = newLastTicks;

            return newLastTicks;
        }

        public void Process()
        {
            if (nextTicks != null && nextTicks.targetUpdate <= Game.CurrentUpdate)
            {
                foreach (T scheduledTick in nextTicks.tickables)
                {
                    scheduledTick.Tick();
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
                current.targetUpdate -= Game.CurrentUpdate;
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
                current.targetUpdate += Game.CurrentUpdate;
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
