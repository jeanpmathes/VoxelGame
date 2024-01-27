// <copyright file="GappedList.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VoxelGame.Core.Collections;

/// <summary>
///     Implements a bag, a structure that allows insertion and removal of items in O(1) time.
///     This specific implementation uses a gap buffer.
/// </summary>
/// <typeparam name="T">The type of the items in the bag.</typeparam>
public class Bag<T> : IEnumerable<T>
{
    private readonly PriorityQueue<int, int> gaps = new();

    private readonly T gapValue;
    private readonly PooledList<T> items = new();

    /// <summary>
    ///     Create a new gapped bag.
    /// </summary>
    /// <param name="gapValue">The value to use in gaps.</param>
    public Bag(T gapValue)
    {
        this.gapValue = gapValue;
    }

    /// <summary>
    ///     Access this bag by index.
    /// </summary>
    /// <param name="index">The index to access. Cannot be larger then the item count.</param>
    public T this[int index]
    {
        get => items[index];
        set => items[index] = value;
    }

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator()
    {
        return items.Where(item => !Equals(item, gapValue)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    ///     Gets a span of the bag. This also includes gaps.
    /// </summary>
    public Span<T> AsSpan()
    {
        return items.AsSpan();
    }

    /// <summary>
    ///     Removes a value from the bag.
    /// </summary>
    /// <param name="index">The index of the item to remove.</param>
    public void RemoveAt(int index)
    {
        items[index] = gapValue;
        gaps.Enqueue(index, index);
    }

    /// <summary>
    ///     Adds an item to the bag.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <returns>The index of the item.</returns>
    public int Add(T item)
    {
        int gap = GetNextGap();
        items[gap] = item;

        return gap;
    }

    private int GetNextGap()
    {
        if (gaps.TryDequeue(out int index, out _)) return index;

        index = items.Count;
        items.Add(gapValue);

        return index;
    }
}
