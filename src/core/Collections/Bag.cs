// <copyright file="GappedList.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace VoxelGame.Core.Collections;

/// <summary>
///     Implements a bag, a structure that allows insertion and removal of items in O(1) time.
///     This specific implementation uses a gap buffer.
/// </summary>
/// <typeparam name="T">The type of the items in the bag.</typeparam>
public class Bag<T> : IEnumerable<T>
{
    private readonly PriorityQueue<Int32, Int32> gaps = new();

    private readonly T gapValue;
    private readonly List<T> items = new();

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
    public T this[Int32 index]
    {
        get => items[index];
        set
        {
            Debug.Assert(!Equals(value, gapValue));

            items[index] = value;
        }
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
        return CollectionsMarshal.AsSpan(items);
    }

    /// <summary>
    ///     Removes a value from the bag.
    /// </summary>
    /// <param name="index">The index of the item to remove.</param>
    public void RemoveAt(Int32 index)
    {
        items[index] = gapValue;
        gaps.Enqueue(index, index);
    }

    /// <summary>
    ///     Adds an item to the bag.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <returns>The index of the item.</returns>
    public Int32 Add(T item)
    {
        Int32 gap = GetNextGap();
        items[gap] = item;

        return gap;
    }

    /// <summary>
    ///     Apply a function to all items in the bag.
    /// </summary>
    /// <param name="function">The function. If the function returns false, the item is removed.</param>
    public void Apply(Func<T, Boolean> function)
    {
        Int32 count = items.Count;

        for (var index = 0; index < count; index++)
        {
            if (Equals(items[index], gapValue)) continue;

            if (!function(items[index])) RemoveAt(index);
        }
    }

    private Int32 GetNextGap()
    {
        if (gaps.TryDequeue(out Int32 index, out _)) return index;

        index = items.Count;
        items.Add(gapValue);

        return index;
    }
}
