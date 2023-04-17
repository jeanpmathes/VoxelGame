// <copyright file="PooledList.cs" company="VoxelGame">
//     Based on the implementation of System.Collections.Generic.List<T>
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace VoxelGame.Core.Collections;

/// <summary>
///     A list that that uses a pool for its internal storage.
/// </summary>
/// <typeparam name="T"></typeparam>
[DebuggerDisplay("Count = {" + nameof(Count) + "}")]
public class PooledList<T> : IEnumerable<T>
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
            Debug.Assert(items != null, NoUseAfterReturnMessage);

            return items.Length;
        }
        set
        {
            Debug.Assert(items != null, NoUseAfterReturnMessage);

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

    /// <summary>
    ///     Gets the number of elements contained in the <see cref="PooledList{T}" />.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    ///     Gets or sets the element at the specified index.
    /// </summary>
    public T this[int index]
    {
        get
        {
            Debug.Assert(items != null, NoUseAfterReturnMessage);

            if ((uint) index >= (uint) Count)
                throw new ArgumentOutOfRangeException(
                    nameof(index),
                    $@"The index has to be smaller then the size '{Count}'.");

            return items[index];
        }

        set
        {
            Debug.Assert(items != null, NoUseAfterReturnMessage);

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
        Debug.Assert(items != null, NoUseAfterReturnMessage);

        for (var i = 0; i < Count; i++) yield return items[i];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        Debug.Assert(items != null, NoUseAfterReturnMessage);

        return GetEnumerator();
    }

    private T[] MoveIntoNew(int newSize)
    {
        Debug.Assert(items != null, NoUseAfterReturnMessage);

        T[] newItems = arrayPool.Rent(newSize);

        if (Count > 0) Array.Copy(items, sourceIndex: 0, newItems, destinationIndex: 0, Count);

        arrayPool.Return(items);

        return newItems;
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
        Debug.Assert(items != null, NoUseAfterReturnMessage);

        if (Count == items.Length) EnsureCapacity(Count + 1);

        items[Count++] = item;
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
        Debug.Assert(items != null, NoUseAfterReturnMessage);

        int count = collection.Count;

        if (count <= 0) return;

        EnsureCapacity(Count + count);

        collection.CopyTo(items, Count);

        Count += count;
    }

    /// <summary>
    ///     Adds the elements of the specified array to the end of the <see cref="PooledList{T}" />.
    /// </summary>
    /// <param name="array">
    ///     The array whose elements should be added to the end of the <see cref="PooledList{T}" />. The array
    ///     itself cannot be <c>null</c>, but it can contain elements that are <c>null</c>, if type <c>T</c> is a reference
    ///     type.
    /// </param>
    /// <param name="count">The amount of elements to add.</param>
    public void AddRange(T[] array, int count)
    {
        Debug.Assert(items != null, NoUseAfterReturnMessage);

        if (count <= 0) return;

        EnsureCapacity(Count + count);

        Array.Copy(array, sourceIndex: 0, items, Count, count);

        Count += count;
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
        Debug.Assert(items != null, NoUseAfterReturnMessage);
        Debug.Assert(pooledList.items != null, NoUseAfterReturnMessage);

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
    ///     Removes the element at the specified index of the <see cref="PooledList{T}" />.
    /// </summary>
    public void RemoveAt(int index)
    {
        Debug.Assert(items != null, NoUseAfterReturnMessage);

        if ((uint) index >= (uint) Count)
            throw new ArgumentOutOfRangeException(
                $"The index '{index}' is not allowed to be larger then the size of the list.");

        Count--;

        if (index < Count) Array.Copy(items, index + 1, items, index, Count - index);

        items[Count] = default!;
    }

    private void EnsureCapacity(int min)
    {
        Debug.Assert(items != null, NoUseAfterReturnMessage);

        if (items.Length >= min) return;

        int newCapacity = items.Length == 0 ? 4 : items.Length * 2;

        if ((uint) newCapacity > int.MaxValue) newCapacity = int.MaxValue;

        if (newCapacity < min) newCapacity = min;

        Capacity = newCapacity;
    }

    /// <summary>
    ///     Clears the list.
    /// </summary>
    public void Clear()
    {
        Debug.Assert(items != null, NoUseAfterReturnMessage);

        Count = 0;
    }

    /// <summary>
    ///     Gives access to the internal memory of this <see cref="PooledList{T}" />.
    ///     Only valid until any other method is called on this <see cref="PooledList{T}" />.
    /// </summary>
    public Span<T> AsSpan()
    {
        Debug.Assert(items != null, NoUseAfterReturnMessage);

        return items.AsSpan(start: 0, Count);
    }

    /// <summary>
    ///     Return the internal array of this <see cref="PooledList{T}" /> to the pool. After calling this method, the exposed
    ///     array should no longer be used.
    /// </summary>
    public void ReturnToPool()
    {
        if (items == null) Debug.Fail("The array is already returned to the pool.");

        arrayPool.Return(items);
        items = null;

        Count = 0;
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~PooledList()
    {
        if (items == null) return;

        Debug.Fail("The array is not returned to the pool.");
    }
}
