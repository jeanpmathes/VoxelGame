// <copyright file="NativeSegment.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections;
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
        this.Count = count;
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

        #region Equality Support

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

        #endregion Equality Support
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

    #region Equality Support

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

    #endregion Equality Support
}
