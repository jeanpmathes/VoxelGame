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
///     Implements a list that allows gaps when removing items.
/// </summary>
/// <typeparam name="T">The type of the items in the list.</typeparam>
public class GappedList<T> : IEnumerable<T> // todo: rename to bag, also rename usage
{
    private readonly PriorityQueue<int, int> gaps = new();

    private readonly T gapValue;
    private readonly PooledList<T> items = new();

    /// <summary>
    ///     Create a new gapped list.
    /// </summary>
    /// <param name="gapValue">The value to use in gaps.</param>
    public GappedList(T gapValue)
    {
        this.gapValue = gapValue;
    }

    /// <summary>
    ///     Access this list by index.
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
    ///     Gets a span of the list. This also includes gaps.
    /// </summary>
    public Span<T> AsSpan()
    {
        return items.AsSpan();
    }

    /// <summary>
    ///     Removes a value from the list.
    /// </summary>
    /// <param name="index">The index of the item to remove.</param>
    public void RemoveAt(int index)
    {
        items[index] = gapValue;
        gaps.Enqueue(index, index);
    }

    /// <summary>
    ///     Adds an item to the list.
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
