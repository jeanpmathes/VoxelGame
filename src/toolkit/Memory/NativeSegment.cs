// <copyright file="NativeSegment.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using VoxelGame.Toolkit.Collections;

namespace VoxelGame.Toolkit.Memory;

/// <summary>
///     A segment of memory allocated on a native heap.
/// </summary>
public readonly unsafe struct NativeSegment<T> : IArray<T>, IEnumerable<T>, IEquatable<NativeSegment<T>> where T : unmanaged
{
    private readonly T* pointer;

    internal NativeSegment(T* pointer, Int32 count)
    {
        this.pointer = pointer;
        Count = count;
    }

    /// <inheritdoc />
    Int32 IArray<T>.Length => Count;

    /// <summary>
    ///     Get the number of elements in this segment.
    /// </summary>
    public Int32 Count { get; }

    /// <summary>
    ///     Get this segment as a span.
    /// </summary>
    /// <returns>The span.</returns>
    public Span<T> AsSpan()
    {
        return new Span<T>(pointer, Count);
    }

    /// <summary>
    ///     Get or set the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    public T this[Int32 index]
    {
        get
        {
            Debug.Assert(index >= 0);
            Debug.Assert(index < Count);

            return pointer[index];
        }
        set
        {
            Debug.Assert(index >= 0);
            Debug.Assert(index < Count);

            pointer[index] = value;
        }
    }

    /// <summary>
    ///     Slice this segment. Sliced segments share the same memory.
    /// </summary>
    /// <param name="start">The start index of the slice.</param>
    /// <param name="length">The length of the slice.</param>
    /// <returns>A subsegment of this segment.</returns>
    public NativeSegment<T> Slice(Int32 start, Int32 length)
    {
        Debug.Assert(start >= 0);
        Debug.Assert(length > 0);
        Debug.Assert(start + length <= Count);

        return new NativeSegment<T>(pointer + start, length);
    }

    #region Enumerable Support

    /// <summary>
    ///     The enumerator for this segment.
    /// </summary>
    public struct Enumerator : IEnumerator<T>, IEquatable<Enumerator>
    {
        private T* pointer;
        private Int32 count;
        private Int32 index;

        internal Enumerator(T* pointer, Int32 count)
        {
            this.pointer = pointer;
            this.count = count;

            index = -1;
        }

        /// <inheritdoc />
        public T Current => pointer[index];

        Object IEnumerator.Current => Current;

        /// <inheritdoc />
        public Boolean MoveNext()
        {
            index++;

            return index < count;
        }

        /// <inheritdoc />
        public void Reset()
        {
            index = -1;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            pointer = null;
            count = 0;
            index = -1;
        }

        #region EQUALITY

        /// <inheritdoc />
        public Boolean Equals(Enumerator other)
        {
            return pointer == other.pointer && count == other.count && index == other.index;
        }

        /// <inheritdoc />
        public override Boolean Equals(Object? obj)
        {
            return obj is Enumerator other && Equals(other);
        }

        /// <inheritdoc />
        public override Int32 GetHashCode()
        {
            return HashCode.Combine(unchecked((Int32) (Int64) pointer), count, index);
        }

        /// <summary>
        ///     Equality operator.
        /// </summary>
        public static Boolean operator ==(Enumerator left, Enumerator right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Inequality operator.
        /// </summary>
        public static Boolean operator !=(Enumerator left, Enumerator right)
        {
            return !left.Equals(right);
        }

        #endregion EQUALITY
    }

    /// <summary>
    ///     Get the enumerator for this segment.
    /// </summary>
    public Enumerator GetEnumerator()
    {
        return new Enumerator(pointer, Count);
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion

    #region EQUALITY

    /// <inheritdoc />
    public Boolean Equals(NativeSegment<T> other)
    {
        return pointer == other.pointer && Count == other.Count;
    }

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        return obj is NativeSegment<T> other && Equals(other);
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return HashCode.Combine(unchecked((Int32) (Int64) pointer), Count);
    }

    /// <summary>
    ///     Equality operator.
    /// </summary>
    public static Boolean operator ==(NativeSegment<T> left, NativeSegment<T> right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Inequality operator.
    /// </summary>
    public static Boolean operator !=(NativeSegment<T> left, NativeSegment<T> right)
    {
        return !left.Equals(right);
    }

    #endregion EQUALITY
}
