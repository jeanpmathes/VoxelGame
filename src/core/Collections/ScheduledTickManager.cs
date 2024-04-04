// <copyright file="ScheduledTicks.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Serialization;
using VoxelGame.Core.Updates;
using VoxelGame.Logging;

namespace VoxelGame.Core.Collections;

/// <summary>
///     Manages scheduled ticks.
/// </summary>
/// <typeparam name="T">The type of tickables to manage.</typeparam>
public class ScheduledTickManager<T> : IEntity where T : ITickable, new()
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<ScheduledTickManager<T>>();

    private readonly UpdateCounter updateCounter;
    private readonly Int32 maxTicksPerUpdate;

    private World? world;

    private TicksHolder? nextTicks;

    /// <summary>
    ///     Create a new scheduled tick manager.
    /// </summary>
    /// <param name="maxTicksPerUpdate">The maximum amount of ticks per update.</param>
    /// <param name="updateCounter">The current update counter.</param>
    public ScheduledTickManager(Int32 maxTicksPerUpdate, UpdateCounter updateCounter)
    {
        this.maxTicksPerUpdate = maxTicksPerUpdate;
        this.updateCounter = updateCounter;
    }

    /// <inheritdoc />
    public static Int32 Version => 1;

    /// <inheritdoc />
    public void Serialize(Serializer serializer, IEntity.Header header)
    {
        TicksHolder? first = nextTicks;
        TicksHolder? previous = null;
        TicksHolder? current = first;

        TicksHolder? next = current?.next;
        serializer.SerializeNullableValue(ref current);

        while (current != null)
        {
            if (previous != null)
                previous.next = current;

            first ??= current;

            previous = current;
            current = next;

            next = current?.next;
            serializer.SerializeNullableValue(ref current);
        }

        nextTicks = first;
    }

    /// <summary>
    /// Set the world in which the ticks will be scheduled.
    /// Use null to unset the world.
    /// Only if the world is set, the ticks can be processed.
    /// </summary>
    /// <param name="newWorld">The world.</param>
    public void SetWorld(World? newWorld)
    {
        world = newWorld;
    }

    /// <summary>
    ///     Add a tickable to the manager.
    /// </summary>
    /// <param name="tick">The tickable to add.</param>
    /// <param name="tickOffset">
    ///     The offset from the current update until the tickable should be ticked. Must be greater than
    ///     0.
    /// </param>
    public void Add(T tick, UInt32 tickOffset)
    {
        Debug.Assert(tickOffset > 0);

        TicksHolder ticks;

        do
        {
            UInt64 targetUpdate = updateCounter.Current + tickOffset;
            ticks = FindOrCreateTargetTick(targetUpdate);

            if (ticks.tickables.Count < maxTicksPerUpdate) break;

            logger.LogWarning(
                "Tick for {Update} has been scheduled for following update as limit is reached",
                targetUpdate);

            tickOffset += 1;

        } while (ticks.tickables.Count >= maxTicksPerUpdate);

        ticks.tickables.Add(tick);
    }

    private TicksHolder FindOrCreateTargetTick(UInt64 targetTick)
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

    /// <summary>
    ///     Tick all tickables that are scheduled for the current update.
    ///     Earlier scheduled ticks are not valid.
    ///     Requires the world to be set.
    /// </summary>
    public void Process()
    {
        while (nextTicks != null && nextTicks.targetUpdate <= updateCounter.Current)
        {
            Debug.Assert(nextTicks.targetUpdate == updateCounter.Current);

            foreach (T scheduledTick in nextTicks.tickables) scheduledTick.Tick(world!);

            nextTicks = nextTicks.next;
        }
    }

    /// <summary>
    ///     Normalizes all target updates so they are offsets from zero, by subtracting the current update.
    ///     Do this before saving, and before resetting the update counter.
    /// </summary>
    public void Normalize()
    {
        TicksHolder? current = nextTicks;

        while (current != null)
        {
            current.targetUpdate -= updateCounter.Current;
            current = current.next;
        }
    }

    /// <summary>
    ///     Clear all scheduled ticks.
    /// </summary>
    public void Clear()
    {
        nextTicks = null;
    }

    private sealed class TicksHolder(UInt64 targetUpdate) : IValue
    {
        public readonly List<T> tickables = [];
        public TicksHolder? next;

        public UInt64 targetUpdate = targetUpdate;

        public TicksHolder() : this(UInt64.MaxValue) {}

        public void Serialize(Serializer serializer)
        {
            serializer.SerializeSmall(ref targetUpdate);
            serializer.SerializeValues(tickables);
        }
    }
}
