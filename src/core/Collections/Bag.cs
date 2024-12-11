// <copyright file="GappedList.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace VoxelGame.Core.Collections;

/// <summary>
///     Implements a bag, a structure that allows insertion and removal of items in O(1) time.
///     This specific implementation uses a gap buffer.
/// </summary>
/// <typeparam name="T">The type of the items in the bag.</typeparam>
public class Bag<T> : IEnumerable<T>
{
    private readonly List<T> items = [];
    private readonly PriorityQueue<Int32, Int32> gaps = new();

    private readonly T gapValue;

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
    ///     Setting an item to the gap value is not allowed.
    /// </summary>
    /// <param name="index">The index to access. Cannot be larger than the item count.</param>
    public T this[Int32 index]
    {
        get => items[index];
        set
        {
            Debug.Assert(!Equals(value, gapValue));

            items[index] = value;
        }
    }

    /// <summary>
    ///     Get the number of items in the bag.
    /// </summary>
    public Int32 Count => items.Count - gaps.Count;

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

    /// <summary>
    ///     Copy this bag into a list.
    ///     The list might contain gaps after this operation.
    /// </summary>
    #pragma warning disable S3956 // Concrete List type required for AddRange method
    public void CopyDirectlyTo(List<T?> other)
    #pragma warning restore S3956
    {
        other.AddRange(items);
    }

    /// <summary>
    ///     Clear the bag, removing all items.
    /// </summary>
    public void Clear()
    {
        items.Clear();
        gaps.Clear();
    }

    private Int32 GetNextGap()
    {
        if (gaps.TryDequeue(out Int32 index, out _)) return index;

        index = items.Count;
        items.Add(gapValue);

        return index;
    }

    #region IEnumerable

    /// <summary>
    ///     The internally-used enumerator.
    /// </summary>
    public struct Enumerator : IEnumerator<T>, IEquatable<Enumerator>
    {
        private readonly Bag<T> bag;
        private Int32 index;

        /// <summary>
        ///     Create a new enumerator.
        /// </summary>
        public Enumerator(Bag<T> bag)
        {
            this.bag = bag;
            index = -1;
        }

        /// <inheritdoc />
        public T Current => bag.items[index];

        Object? IEnumerator.Current => Current;

        /// <inheritdoc />
        public Boolean MoveNext()
        {
            do
            {
                index += 1;
            } while (index < bag.items.Count && Equals(bag.items[index], bag.gapValue));

            return index < bag.items.Count;
        }

        /// <inheritdoc />
        public void Reset()
        {
            index = -1;
        }

        /// <inheritdoc />
        public void Dispose() {}

        #region Equality Support

        /// <inheritdoc />
        public Boolean Equals(Enumerator other)
        {
            return ReferenceEquals(bag, other.bag) && index == other.index;
        }

        /// <inheritdoc />
        public override Boolean Equals(Object? obj)
        {
            return obj is Enumerator other && Equals(other);
        }

        /// <inheritdoc />
        public override Int32 GetHashCode()
        {
            return HashCode.Combine(bag, index);
        }

        /// <summary>
        ///     Test for equality.
        /// </summary>
        public static Boolean operator ==(Enumerator left, Enumerator right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Test for inequality.
        /// </summary>
        public static Boolean operator !=(Enumerator left, Enumerator right)
        {
            return !left.Equals(right);
        }

        #endregion Equality Support
    }

    /// <summary>
    ///     Get an enumerator for the bag.
    /// </summary>
    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion IEnumerable
}
