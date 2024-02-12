// <copyright file="Cache.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     A cache for objects that can be disposed.
///     Uses the LRU (Least Recently Used) policy.
/// </summary>
/// <typeparam name="TK">The type of the key.</typeparam>
/// <typeparam name="TV">The type of the value.</typeparam>
public sealed class Cache<TK, TV> : IDisposable
    where TK : notnull
    where TV : IDisposable
{
    private readonly LinkedList<Entry> list = new();
    private readonly Dictionary<TK, LinkedListNode<Entry>> map = new();

    /// <summary>
    ///     Create a new cache with the given capacity.
    /// </summary>
    /// <param name="capacity">The capacity of the cache.</param>
    public Cache(int capacity)
    {
        Capacity = capacity;
    }

    /// <summary>
    ///     The capacity of the cache.
    ///     This is the maximum number of objects that can be stored.
    /// </summary>
    private int Capacity { get; }

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
    public bool TryGet(TK key, [NotNullWhen(returnValue: true)] out TV? value, bool remove = false)
    {
        Throw.IfDisposed(disposed);

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
        Throw.IfDisposed(disposed);

        if (map.TryGetValue(key, out LinkedListNode<Entry>? existing))
        {
            list.Remove(existing);
        }
        else if (list.Count >= Capacity)
        {
            LinkedListNode<Entry> node = list.First!;

            list.RemoveFirst();
            map.Remove(node.Value.Key);

            node.Value.Value.Dispose();
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
        Throw.IfDisposed(disposed);

        foreach (Entry entry in list) entry.Value.Dispose();

        list.Clear();
        map.Clear();
    }

    private sealed record Entry(TK Key, TV Value);

    #region IDisposable Support

    private bool disposed;

    private void Dispose(bool disposing)
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
    ~Cache()
    {
        Dispose(disposing: false);
    }

    #endregion
}
