// <copyright file="Sides.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections;
using System.Collections.Generic;
using VoxelGame.Core.Logic.Elements;

namespace VoxelGame.Core.Collections;

#pragma warning disable S3876 // Indexing using the enum is the entire point of this class.

/// <summary>
///     An array to store elements associated with the sides of a cube.
/// </summary>
/// <typeparam name="T">The type of the elements.</typeparam>
public class Sides<T> : IEnumerable<T> // todo: check all arrays with length 6 and replace with this
{
    private readonly T[] sides = new T[6];

    /// <summary>
    ///     Get or set the element for the given side.
    /// </summary>
    public T this[BlockSide side]
    {
        get => Get(side);
        set => Set(side, value);
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private ref T GetRef(BlockSide side)
    {
        return ref sides[(Int32) side];
    }

    /// <summary>
    ///     Set the element for the given side.
    /// </summary>
    /// <param name="side">The side.</param>
    /// <param name="value">The value.</param>
    public void Set(BlockSide side, T value)
    {
        GetRef(side) = value;
    }

    /// <summary>
    ///     Get the element for the given side.
    /// </summary>
    /// <param name="side">The side.</param>
    /// <returns>The value.</returns>
    public T Get(BlockSide side)
    {
        return GetRef(side);
    }

    /// <summary>
    ///     Get the enumerator.
    /// </summary>
    public Enumerator GetEnumerator()
    {
        return new Enumerator(sides);
    }

    /// <summary>
    ///     The internally-used enumerator.
    /// </summary>
    public struct Enumerator : IEnumerator<T>, IEquatable<Enumerator>
    {
        private readonly T[] sides;
        private Int32 index;

        /// <summary>
        ///     Create a new enumerator.
        /// </summary>
        public Enumerator(T[] sides)
        {
            this.sides = sides;
            index = -1;
        }

        /// <inheritdoc />
        public T Current => sides[index];

        Object? IEnumerator.Current => Current;

        /// <inheritdoc />
        public Boolean MoveNext()
        {
            index += 1;

            return index < sides.Length;
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
            return ReferenceEquals(sides, other.sides) && index == other.index;
        }

        /// <inheritdoc />
        public override Boolean Equals(Object? obj)
        {
            return obj is Enumerator other && Equals(other);
        }

        /// <inheritdoc />
        public override Int32 GetHashCode()
        {
            return HashCode.Combine(sides, index);
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
}
