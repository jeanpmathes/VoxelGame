﻿// <copyright file="PooledList.cs" company="VoxelGame">
//     Based on the implementation of System.Collections.Generic.List<T>
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Collections;

/// <summary>
///     A list that that uses a pool for its internal storage.
///     Dispose must be called to return the internal array to the pool.
/// </summary>
/// <typeparam name="T"></typeparam>
[DebuggerDisplay("Count = {" + nameof(Count) + "}")]
public sealed class PooledList<T> : IList<T>, IDisposable
{
    private const string NoUseAfterReturnMessage = "The list is not usable after it has been returned to the pool.";

    private readonly ArrayPool<T> arrayPool;

    private T[]? items;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PooledList{T}" /> class that is empty and has the default initial
    ///     capacity.
    /// </summary>
    public PooledList()
    {
        arrayPool = ArrayPool<T>.Shared;
        items = Array.Empty<T>();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PooledList{T}" /> class that is empty and has at least the specified
    ///     initial capacity.
    /// </summary>
    /// <param name="capacity">The minimum number of elements that the new list can initially store. The</param>
    [SuppressMessage("ReSharper.DPA", "DPA0002: Excessive memory allocations in SOH")]
    public PooledList(int capacity)
    {
        arrayPool = ArrayPool<T>.Shared;

        if (capacity < 0)
            throw new ArgumentOutOfRangeException(
                nameof(capacity),
                $@"Value '{capacity}' is negative, which is not allowed.");

        items = capacity == 0 ? Array.Empty<T>() : arrayPool.Rent(capacity);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PooledList{T}" /> class that is empty, has at least the specified
    ///     initial capacity and uses a specified <see cref="ArrayPool{T}" />
    /// </summary>
    /// <param name="capacity">The minimum number of elements that the new list can initially store. The</param>
    /// <param name="arrayPool">The <see cref="ArrayPool{T}" /> to use.</param>
    public PooledList(int capacity, ArrayPool<T> arrayPool) : this(capacity)
    {
        this.arrayPool = arrayPool;
    }

    /// <summary>
    ///     Gets or sets the minimum number of elements the internal data structure can hold without resizing.
    /// </summary>
    public int Capacity
    {
        get
        {
            Throw.IfDisposed(disposed);
            Throw.IfNull(items, NoUseAfterReturnMessage);

            return items.Length;
        }
        set
        {
            Throw.IfDisposed(disposed);
            Throw.IfNull(items, NoUseAfterReturnMessage);

            if (value < Count)
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    $@"Value '{value}' is smaller than size '{Count}'.");

            if (value != items.Length)
            {
                if (value > 0)
                {
                    items = MoveIntoNew(value);
                }
                else
                {
                    if (items.Length > 0) arrayPool.Return(items);
                    items = Array.Empty<T>();
                }
            }
        }
    }

    /// <inheritdoc />
    public bool Remove(T item)
    {
        Throw.IfDisposed(disposed);
        Throw.IfNull(items, NoUseAfterReturnMessage);

        int index = IndexOf(item);

        if (index < 0) return false;

        RemoveAt(index);

        return true;
    }

    /// <summary>
    ///     Gets the number of elements contained in the <see cref="PooledList{T}" />.
    /// </summary>
    public int Count { get; private set; }

    /// <inheritdoc />
    public bool IsReadOnly => false;

    /// <summary>
    ///     Gets or sets the element at the specified index.
    /// </summary>
    public T this[int index]
    {
        get
        {
            Throw.IfDisposed(disposed);
            Throw.IfNull(items, NoUseAfterReturnMessage);

            if ((uint) index >= (uint) Count)
                throw new ArgumentOutOfRangeException(
                    nameof(index),
                    $@"The index has to be smaller then the size '{Count}'.");

            return items[index];
        }

        set
        {
            Throw.IfDisposed(disposed);
            Throw.IfNull(items, NoUseAfterReturnMessage);

            if ((uint) index >= (uint) Count)
                throw new ArgumentOutOfRangeException(
                    nameof(index),
                    $@"The index has to be smaller then the size '{Count}'.");

            items[index] = value;
        }
    }

    /// <summary>
    ///     Returns an enumerator that iterates through the <see cref="PooledList{T}" />.
    /// </summary>
    public IEnumerator<T> GetEnumerator()
    {
        Throw.IfDisposed(disposed);
        Throw.IfNull(items, NoUseAfterReturnMessage);

        for (var i = 0; i < Count; i++) yield return items[i];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        Throw.IfDisposed(disposed);
        Throw.IfNull(items, NoUseAfterReturnMessage);

        return GetEnumerator();
    }

    /// <summary>
    ///     Adds an object to the end of the <see cref="PooledList{T}" />.
    /// </summary>
    /// <param name="item">
    ///     The object to be added to the end of the <see cref="PooledList{T}" />. The value can be null for
    ///     reference types.
    /// </param>
    public void Add(T item)
    {
        Throw.IfDisposed(disposed);
        Throw.IfNull(items, NoUseAfterReturnMessage);

        if (Count == items.Length) EnsureCapacity(Count + 1);

        items[Count++] = item;
    }

    /// <inheritdoc />
    public int IndexOf(T item)
    {
        Throw.IfDisposed(disposed);
        Throw.IfNull(items, NoUseAfterReturnMessage);

        return Array.IndexOf(items, item, startIndex: 0, Count);
    }

    /// <inheritdoc />
    public void Insert(int index, T item)
    {
        Throw.IfDisposed(disposed);
        Throw.IfNull(items, NoUseAfterReturnMessage);

        if ((uint) index > (uint) Count)
            throw new ArgumentOutOfRangeException(
                nameof(index),
                $@"The index has to be smaller or equal then the size '{Count}'.");

        if (Count == items.Length)
            Add(item);
        else this[index] = item;
    }

    /// <summary>
    ///     Removes the element at the specified index of the <see cref="PooledList{T}" />.
    /// </summary>
    public void RemoveAt(int index)
    {
        Throw.IfDisposed(disposed);
        Throw.IfNull(items, NoUseAfterReturnMessage);

        if ((uint) index >= (uint) Count)
            throw new ArgumentOutOfRangeException(
                $"The index '{index}' is not allowed to be larger then the size of the list.");

        Count--;

        if (index < Count) Array.Copy(items, index + 1, items, index, Count - index);

        items[Count] = default!;
    }

    /// <summary>
    ///     Clears the list.
    /// </summary>
    public void Clear()
    {
        Throw.IfDisposed(disposed);
        Throw.IfNull(items, NoUseAfterReturnMessage);

        Count = 0;
    }

    /// <inheritdoc />
    public bool Contains(T item)
    {
        Throw.IfDisposed(disposed);
        Throw.IfNull(items, NoUseAfterReturnMessage);

        for (var index = 0; index < Count; index++)
            if (Equals(items[index], item))
                return true;

        return false;
    }

    /// <inheritdoc />
    public void CopyTo(T[] array, int arrayIndex)
    {
        Throw.IfDisposed(disposed);
        Throw.IfNull(items, NoUseAfterReturnMessage);

        Array.Copy(items, sourceIndex: 0, array, arrayIndex, Count);
    }

    private T[] MoveIntoNew(int newSize)
    {
        Throw.IfDisposed(disposed);
        Throw.IfNull(items, NoUseAfterReturnMessage);

        T[] newItems = arrayPool.Rent(newSize);

        if (Count > 0) Array.Copy(items, sourceIndex: 0, newItems, destinationIndex: 0, Count);

        arrayPool.Return(items);

        return newItems;
    }

    /// <summary>
    ///     Adds the elements of the specified collection to the end of the <see cref="PooledList{T}" />.
    /// </summary>
    /// <param name="collection">
    ///     The collection whose elements should be added to the end of the <see cref="PooledList{T}" />.
    ///     The collection itself cannot be <c>null</c>, but it can contain elements that are <c>null</c>, if type <c>T</c> is
    ///     a reference type.
    /// </param>
    public void AddRange(ICollection<T> collection)
    {
        Throw.IfDisposed(disposed);
        Throw.IfNull(items, NoUseAfterReturnMessage);

        int count = collection.Count;

        if (count <= 0) return;

        EnsureCapacity(Count + count);

        collection.CopyTo(items, Count);

        Count += count;
    }

    /// <summary>
    ///     Adds the elements of the specified span to the end of the <see cref="PooledList{T}" />.
    /// </summary>
    /// <param name="span">
    ///     The span whose elements should be added to the end of the <see cref="PooledList{T}" />. The span
    ///     itself cannot be <c>null</c>, but it can contain elements that are <c>null</c>, if type <c>T</c> is a reference
    ///     type.
    /// </param>
    public void AddRange(Span<T> span)
    {
        Throw.IfDisposed(disposed);
        Throw.IfNull(items, NoUseAfterReturnMessage);

        if (span.Length <= 0) return;

        EnsureCapacity(Count + span.Length);

        Span<T> destination = items;
        span.CopyTo(destination[Count..]);

        Count += span.Length;
    }

    /// <summary>
    ///     Adds the elements of another <see cref="PooledList{T}" /> to the end of this <see cref="PooledList{T}" />.
    /// </summary>
    /// <param name="pooledList">
    ///     The <see cref="PooledList{T}" /> whose elements should be added to the end of the
    ///     <see cref="PooledList{T}" />. It is not allowed to be null or equal to the list it should be added to.
    /// </param>
    public void AddRange(PooledList<T> pooledList)
    {
        Throw.IfDisposed(disposed);
        Throw.IfNull(items, NoUseAfterReturnMessage);
        Throw.IfNull(pooledList.items, NoUseAfterReturnMessage);

        if (this == pooledList)
            throw new ArgumentException($@"Adding '{this}' to itself not allowed", nameof(pooledList));

        int count = pooledList.Count;

        if (count > 0)
        {
            EnsureCapacity(Count + count);

            Array.Copy(pooledList.items, sourceIndex: 0, items, Count, count);

            Count += count;
        }
    }

    /// <summary>
    ///     Ensures that the <see cref="PooledList{T}" /> can hold at least <paramref name="min" /> elements without resizing.
    ///     This operation can cause the <see cref="PooledList{T}" /> to be resized.
    /// </summary>
    /// <param name="min">The minimum amount of elements the <see cref="PooledList{T}" /> should be able to hold.</param>
    public void EnsureCapacity(int min)
    {
        Throw.IfDisposed(disposed);
        Throw.IfNull(items, NoUseAfterReturnMessage);

        if (items.Length >= min) return;

        int newCapacity = items.Length == 0 ? 4 : items.Length * 2;

        if ((uint) newCapacity > int.MaxValue) newCapacity = int.MaxValue;

        if (newCapacity < min) newCapacity = min;

        Capacity = newCapacity;
    }

    /// <summary>
    ///     Gives access to the internal memory of this <see cref="PooledList{T}" />.
    ///     Only valid until any other method is called on this <see cref="PooledList{T}" />.
    /// </summary>
    public Span<T> AsSpan()
    {
        Throw.IfDisposed(disposed);
        Throw.IfNull(items, NoUseAfterReturnMessage);

        return items.AsSpan(start: 0, Count);
    }

    /// <summary>
    ///     Return the internal array of this <see cref="PooledList{T}" /> to the pool. After calling this method, the exposed
    ///     array should no longer be used.
    /// </summary>
    private void ReturnToPool()
    {
        Throw.IfDisposed(disposed);
        Throw.IfNull(items, NoUseAfterReturnMessage);

        arrayPool.Return(items);
        items = null;

        Count = 0;
    }

    #region IDisposable Support

    private bool disposed;

    private void Dispose(bool disposing)
    {
        if (disposed) return;

        if (disposing) ReturnToPool();
        else Throw.ForMissedDispose(nameof(PooledList<T>));

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
    ~PooledList()
    {
        Dispose(disposing: false);
    }

    #endregion IDisposable Support
}
