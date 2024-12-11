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
using VoxelGame.Toolkit.Utilities.Constants;

namespace VoxelGame.Core.Collections;

/// <summary>
///     Manages scheduled ticks.
/// </summary>
/// <typeparam name="T">The type of tickables to manage.</typeparam>
/// <typeparam name="TMaxTicksPerUpdate">The maximum amount of ticks per update.</typeparam>
public partial class ScheduledTickManager<T, TMaxTicksPerUpdate> : IEntity
    where T : ITickable, new()
    where TMaxTicksPerUpdate : IConstantInt32
{
    #pragma warning disable S2743 // Intentional and known.
    private static readonly ObjectPool<TicksHolder> holderPool = new(CreateTicksHolder);
    #pragma warning restore S2743

    private readonly UpdateCounter updateCounter;

    private World? world;

    private TicksHolder? nextTicks;

    /// <summary>
    ///     Create a new scheduled tick manager.
    /// </summary>
    /// <param name="updateCounter">The current update counter.</param>
    public ScheduledTickManager(UpdateCounter updateCounter)
    {
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
        SerializeHolder(serializer, ref current);

        while (current != null)
        {
            if (previous != null)
                previous.next = current;

            first ??= current;

            previous = current;
            current = next;

            next = current?.next;
            SerializeHolder(serializer, ref current);
        }

        nextTicks = first;
    }

    private static void SerializeHolder(Serializer serializer, ref TicksHolder? holder)
    {
        Boolean hasValue = holder != null;
        serializer.Serialize(ref hasValue);

        if (hasValue)
        {
            holder ??= GetHolderFromPool(UInt64.MaxValue);
            serializer.SerializeValue(ref holder);
        }
        else
        {
            holder = null;
        }
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
            ticks = FindOrCreateTargetHolder(targetUpdate);

            if (ticks.tickables.Count == TMaxTicksPerUpdate.Value - 1)
                LogTickScheduleLimitReached(logger);

            if (ticks.tickables.Count < TMaxTicksPerUpdate.Value) break;

            tickOffset += 1;

        } while (ticks.tickables.Count >= TMaxTicksPerUpdate.Value);

        ticks.tickables.Add(tick);
    }

    private TicksHolder FindOrCreateTargetHolder(UInt64 targetTick)
    {
        TicksHolder? last = null;
        TicksHolder? current = nextTicks;

        if (current == null)
            return GetInsertedHolder(previous: null, targetTick, next: null);

        while (current != null)
        {
            if (current.targetUpdate == targetTick)
                return current;

            if (current.targetUpdate > targetTick)
                return GetInsertedHolder(last, targetTick, current);

            last = current;
            current = current.next;
        }

        return GetInsertedHolder(last, targetTick, next: null);
    }

    private TicksHolder GetInsertedHolder(TicksHolder? previous, UInt64 targetTick, TicksHolder? next)
    {
        TicksHolder newTicks = GetHolderFromPool(targetTick);

        if (previous == null) nextTicks = newTicks;
        else previous.next = newTicks;

        newTicks.next = next;

        return newTicks;
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

            foreach (T scheduledTick in nextTicks.tickables)
                scheduledTick.Tick(world!);

            TicksHolder? next = nextTicks.next;

            ReturnHolderToPool(nextTicks);

            nextTicks = next;
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
        while (nextTicks != null)
        {
            TicksHolder? next = nextTicks.next;

            ReturnHolderToPool(nextTicks);

            nextTicks = next;
        }
    }

    private static TicksHolder CreateTicksHolder()
    {
        return new TicksHolder(UInt64.MaxValue);
    }

    private static TicksHolder GetHolderFromPool(UInt64 targetUpdate)
    {
        TicksHolder holder = holderPool.Get();

        holder.targetUpdate = targetUpdate;

        return holder;
    }

    private static void ReturnHolderToPool(TicksHolder holder)
    {
        holder.tickables.Clear();

        holderPool.Return(holder);
    }

    private sealed class TicksHolder(UInt64 targetUpdate) : IValue
    {
        public readonly List<T> tickables = [];
        public TicksHolder? next;

        public UInt64 targetUpdate = targetUpdate;

        public void Serialize(Serializer serializer)
        {
            serializer.SerializeSmall(ref targetUpdate);
            serializer.SerializeValues(tickables);
        }
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<ScheduledTickManager<T, TMaxTicksPerUpdate>>();

    [LoggerMessage(EventId = Events.Simulation, Level = LogLevel.Warning, Message = "The maximum number of ticks for a single update have been reached, further ticks we be scheduled later")]
    private static partial void LogTickScheduleLimitReached(ILogger logger);

    #endregion LOGGING
}
