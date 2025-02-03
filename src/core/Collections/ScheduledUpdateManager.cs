// <copyright file="ScheduledUpdateManager.cs" company="VoxelGame">
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
///     Manages scheduled updates.
/// </summary>
/// <typeparam name="T">The type of updates to manage.</typeparam>
/// <typeparam name="TMaxScheduledUpdatesPerLogicUpdate">The maximum amount of updates per update.</typeparam>
public partial class ScheduledUpdateManager<T, TMaxScheduledUpdatesPerLogicUpdate> : IEntity
    where T : IUpdateable, new()
    where TMaxScheduledUpdatesPerLogicUpdate : IConstantInt32
{
    #pragma warning disable S2743 // Intentional and known.
    private static readonly ObjectPool<UpdateHolder> holderPool = new(CreateUpdateHolder);
    #pragma warning restore S2743

    private readonly UpdateCounter updateCounter;

    private World? world;

    private UpdateHolder? nextUpdates;

    /// <summary>
    ///     Create a new scheduled update manager.
    /// </summary>
    /// <param name="updateCounter">The current update counter.</param>
    public ScheduledUpdateManager(UpdateCounter updateCounter)
    {
        this.updateCounter = updateCounter;
    }

    /// <inheritdoc />
    public static UInt32 CurrentVersion => 1;

    /// <inheritdoc />
    public void Serialize(Serializer serializer, IEntity.Header header)
    {
        UpdateHolder? first = nextUpdates;
        UpdateHolder? previous = null;
        UpdateHolder? current = first;

        UpdateHolder? next = current?.next;
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

        nextUpdates = first;
    }

    private static void SerializeHolder(Serializer serializer, ref UpdateHolder? holder)
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
    ///     Set the world in which the updates will be scheduled.
    ///     Use null to unset the world.
    ///     Only if the world is set, the updates can be processed.
    /// </summary>
    /// <param name="newWorld">The world.</param>
    public void SetWorld(World? newWorld)
    {
        world = newWorld;
    }

    /// <summary>
    ///     Add an updateable to the manager.
    /// </summary>
    /// <param name="updateable">The updateable to add.</param>
    /// <param name="updateOffset">
    ///     The offset from the current update number until the updateable should be updated. Must be greater than
    ///     0.
    /// </param>
    public void Add(T updateable, UInt32 updateOffset)
    {
        Debug.Assert(updateOffset > 0);

        UpdateHolder update;

        do
        {
            UInt64 targetUpdate = updateCounter.Current + updateOffset;
            update = FindOrCreateTargetHolder(targetUpdate);

            if (update.updateables.Count == TMaxScheduledUpdatesPerLogicUpdate.Value - 1)
                LogUpdateScheduleLimitReached(logger);

            if (update.updateables.Count < TMaxScheduledUpdatesPerLogicUpdate.Value) break;

            updateOffset += 1;

        } while (update.updateables.Count >= TMaxScheduledUpdatesPerLogicUpdate.Value);

        update.updateables.Add(updateable);
    }

    private UpdateHolder FindOrCreateTargetHolder(UInt64 targetUpdate)
    {
        UpdateHolder? last = null;
        UpdateHolder? current = nextUpdates;

        if (current == null)
            return GetInsertedHolder(previous: null, targetUpdate, next: null);

        while (current != null)
        {
            if (current.targetUpdate == targetUpdate)
                return current;

            if (current.targetUpdate > targetUpdate)
                return GetInsertedHolder(last, targetUpdate, current);

            last = current;
            current = current.next;
        }

        return GetInsertedHolder(last, targetUpdate, next: null);
    }

    private UpdateHolder GetInsertedHolder(UpdateHolder? previous, UInt64 targetUpdate, UpdateHolder? next)
    {
        UpdateHolder newUpdate = GetHolderFromPool(targetUpdate);

        if (previous == null) nextUpdates = newUpdate;
        else previous.next = newUpdate;

        newUpdate.next = next;

        return newUpdate;
    }

    /// <summary>
    ///     Update all updateables that are scheduled for the current update cycle.
    ///     Earlier scheduled updates are not valid.
    ///     Requires the world to be set.
    /// </summary>
    public void Process()
    {
        while (nextUpdates != null && nextUpdates.targetUpdate <= updateCounter.Current)
        {
            Debug.Assert(nextUpdates.targetUpdate == updateCounter.Current);

            foreach (T scheduledUpdate in nextUpdates.updateables)
                scheduledUpdate.Update(world!);

            UpdateHolder? next = nextUpdates.next;

            ReturnHolderToPool(nextUpdates);

            nextUpdates = next;
        }
    }

    /// <summary>
    ///     Normalizes all target updates so they are offsets from zero, by subtracting the current update.
    ///     Do this before saving, and before resetting the update counter.
    /// </summary>
    public void Normalize()
    {
        UpdateHolder? current = nextUpdates;

        while (current != null)
        {
            current.targetUpdate -= updateCounter.Current;
            current = current.next;
        }
    }

    /// <summary>
    ///     Clear all scheduled updates.
    /// </summary>
    public void Clear()
    {
        while (nextUpdates != null)
        {
            UpdateHolder? next = nextUpdates.next;

            ReturnHolderToPool(nextUpdates);

            nextUpdates = next;
        }
    }

    private static UpdateHolder CreateUpdateHolder()
    {
        return new UpdateHolder(UInt64.MaxValue);
    }

    private static UpdateHolder GetHolderFromPool(UInt64 targetUpdate)
    {
        UpdateHolder holder = holderPool.Get();

        holder.targetUpdate = targetUpdate;

        return holder;
    }

    private static void ReturnHolderToPool(UpdateHolder holder)
    {
        holder.updateables.Clear();

        holderPool.Return(holder);
    }

    private sealed class UpdateHolder(UInt64 targetUpdate) : IValue
    {
        public readonly List<T> updateables = [];
        public UpdateHolder? next;

        public UInt64 targetUpdate = targetUpdate;

        public void Serialize(Serializer serializer)
        {
            serializer.SerializeSmall(ref targetUpdate);
            serializer.SerializeValues(updateables);
        }
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<ScheduledUpdateManager<T, TMaxScheduledUpdatesPerLogicUpdate>>();

    [LoggerMessage(EventId = LogID.ScheduledUpdateManager + 0,
        Level = LogLevel.Warning,
        Message = "The maximum number of scheduled updates for a single update cycle have been reached, further updates will be scheduled later")]
    private static partial void LogUpdateScheduleLimitReached(ILogger logger);

    #endregion LOGGING
}
