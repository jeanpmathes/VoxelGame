﻿// <copyright file="Cache.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace VoxelGame.Core.Collections;

/// <summary>
///     A cache for objects.
///     Uses the LRU (Least Recently Used) policy.
/// </summary>
/// <typeparam name="TK">The type of the key.</typeparam>
/// <typeparam name="TV">The type of the value.</typeparam>
public class Cache<TK, TV>
    where TK : notnull
    where TV : notnull
{
    private readonly LinkedList<Entry> list = [];
    private readonly Dictionary<TK, LinkedListNode<Entry>> map = new();

    private readonly Action<TV>? cleanup;

    /// <summary>
    ///     Create a new cache with the given capacity.
    /// </summary>
    /// <param name="capacity">The capacity of the cache.</param>
    public Cache(Int32 capacity)
    {
        Capacity = capacity;
    }

    /// <summary>
    ///     Create a new cache with the given capacity and cleanup action.
    /// </summary>
    /// <param name="capacity">The capacity of the cache.</param>
    /// <param name="cleanup">The cleanup action to be called when an entry is removed from the cache.</param>
    protected Cache(Int32 capacity, Action<TV> cleanup) : this(capacity)
    {
        this.cleanup = cleanup;
    }

    /// <summary>
    ///     The capacity of the cache.
    ///     This is the maximum number of objects that can be stored.
    /// </summary>
    private Int32 Capacity { get; }

    /// <summary>
    ///     Gets the number of elements in the cache.
    /// </summary>
    public Int32 Count => list.Count;

    /// <summary>
    ///     Tries to get the value associated with the specified key.
    ///     If the key is found, the value is returned and the entry is moved to the end of the cache.
    ///     If the key is not found, the default value for the type is returned.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="value">
    ///     When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the
    ///     default value for the type of the value parameter.
    /// </param>
    /// <param name="remove">
    ///     Whether to remove the entry from the cache.
    ///     If true, the cache essentially becomes a linked map.
    /// </param>
    /// <returns>true if the cache contains an element with the specified key; otherwise, false.</returns>
    public Boolean TryGet(TK key, [NotNullWhen(returnValue: true)] out TV? value, Boolean remove = false)
    {
        if (map.TryGetValue(key, out LinkedListNode<Entry>? node))
        {
            list.Remove(node);

            if (remove) map.Remove(key);
            else list.AddLast(node);

            value = node.Value.Value;

            return true;
        }

        value = default;

        return false;
    }

    /// <summary>
    ///     Adds a key/value pair to the cache.
    ///     If the key already exists, the old value is replaced by the new value.
    ///     If the cache is full, the least recently used item is removed before the new item is added.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The value of the element to add.</param>
    public void Add(TK key, TV value)
    {
        if (map.TryGetValue(key, out LinkedListNode<Entry>? existing))
        {
            list.Remove(existing);
        }
        else if (list.Count >= Capacity)
        {
            LinkedListNode<Entry> node = list.First!;

            list.RemoveFirst();
            map.Remove(node.Value.Key);

            cleanup?.Invoke(node.Value.Value);
        }

        LinkedListNode<Entry> newNode = new(new Entry(key, value));

        list.AddLast(newNode);
        map[key] = newNode;
    }

    /// <summary>
    ///     Flushes the cache, disposing all values.
    /// </summary>
    public void Flush()
    {
        if (cleanup != null)
            foreach (Entry entry in list)
                cleanup(entry.Value);

        list.Clear();
        map.Clear();
    }

    private sealed record Entry(TK Key, TV Value);
}

/// <summary>
///     A cache for objects that can be disposed.
///     Uses the LRU (Least Recently Used) policy.
/// </summary>
/// <typeparam name="TK">The type of the key.</typeparam>
/// <typeparam name="TV">The type of the value.</typeparam>
public sealed class DisposableCache<TK, TV> : Cache<TK, TV>, IDisposable
    where TK : notnull
    where TV : IDisposable
{
    /// <inheritdoc />
    public DisposableCache(Int32 capacity) : base(capacity, Cleanup) {}

    private static void Cleanup(TV value)
    {
        value.Dispose();
    }

    #region DISPOSABLE

    private Boolean disposed;

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing) Flush();

        disposed = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~DisposableCache()
    {
        Dispose(disposing: false);
    }

    #endregion
}
