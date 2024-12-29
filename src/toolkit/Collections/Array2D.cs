// <copyright file="Array2D.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections;
using System.Diagnostics;
using JetBrains.Annotations;
using OpenTK.Mathematics;

namespace VoxelGame.Toolkit.Collections;

/// <summary>
///     A 2D array where each dimension has the same length.
///     Internally, the array is stored as an 1D array.
/// </summary>
/// <typeparam name="T">The type of the elements.</typeparam>
public sealed class Array2D<T> : IEnumerable<T>, IArray<T>
{
    private readonly T[] array;

    private readonly Int32 xFactor;
    private readonly Int32 yFactor;

    /// <summary>
    ///     Create a new array with the given length in all dimensions.
    /// </summary>
    /// <param name="length">The length of all dimensions. Must be greater than 0.</param>
    /// <param name="transpose">Whether to transpose the array - this will swap the coordinates on access.</param>
    public Array2D(Int32 length, Boolean transpose = false)
    {
        Debug.Assert(length > 0);

        array = new T[length * length];
        Length = length;

        (xFactor, yFactor) = !transpose ? (length, 1) : (1, length);
    }

    /// <summary>
    ///     Create a new array with the given values.
    /// </summary>
    /// <param name="values">The values to initialize the array with.</param>
    public Array2D(T[][] values) : this(values.Length)
    {
        Debug.Assert(values.Length == Length);

        for (var x = 0; x < Length; x++)
        {
            Debug.Assert(values[x].Length == Length);

            for (var y = 0; y < Length; y++) this[x, y] = values[x][y];
        }
    }

    /// <summary>
    ///     Access the element at the given position.
    /// </summary>
    /// <param name="x">The x coordinate. Must be between 0 and <see cref="Length" /> - 1.</param>
    /// <param name="y">The y coordinate. Must be between 0 and <see cref="Length" /> - 1.</param>
    public ref T this[Int32 x, Int32 y] => ref GetRef(x, y);

    /// <summary>
    ///     Access the element at the given position.
    /// </summary>
    /// <param name="position">The position. All components must be between 0 and <see cref="Length" /> - 1.</param>
    #pragma warning disable S3876 // Vector3i is a fitting near-primitive type.
    public ref T this[Vector2i position]
    #pragma warning restore S3876
    {
        get => ref GetRef(position.X, position.Y);
    }

    /// <summary>
    ///     Get the length of each dimension.
    /// </summary>
    public Int32 Length { get; }

    Int32 IArray<T>.Count => array.Length;

    T IArray<T>.this[Int32 index]
    {
        get => array[index];
        set => array[index] = value;
    }

    /// <inheritdoc />
    [MustDisposeResource]
    public IEnumerator<T> GetEnumerator()
    {
        return ((IEnumerable<T>) array).GetEnumerator();
    }

    [MustDisposeResource]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    ///     Get the array as a span.
    /// </summary>
    /// <returns>The array as a span.</returns>
    public Span<T> AsSpan()
    {
        return array;
    }

    /// <summary>
    ///     Get a reference to the element at the given position.
    /// </summary>
    private ref T GetRef(Int32 x, Int32 y)
    {
        Debug.Assert(x >= 0 && x < Length);
        Debug.Assert(y >= 0 && y < Length);

        return ref array[x * xFactor + y * yFactor];
    }

    /// <summary>
    ///     Fill the array with the given value.
    /// </summary>
    /// <param name="value">The value to fill the array with.</param>
    public void Fill(T value)
    {
        Array.Fill(array, value);
    }
}
