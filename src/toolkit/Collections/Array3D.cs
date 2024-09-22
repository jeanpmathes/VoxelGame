// <copyright file="Array3D.cs" company="VoxelGame">
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
///     A 3D array where each dimension has the same length.
///     Internally, the array is stored as an 1D array.
/// </summary>
/// <typeparam name="T">The type of the elements.</typeparam>
public class Array3D<T> : IEnumerable<T>
{
    private readonly T[] array;

    private readonly Int32 xFactor;
    private readonly Int32 yFactor;
    private readonly Int32 zFactor;

    /// <summary>
    ///     Create a new array with the given length in all dimensions.
    /// </summary>
    /// <param name="length">The length of all dimensions. Must be greater than 0.</param>
    /// <param name="transpose">Whether to transpose the array - this will swap the coordinates on access.</param>
    public Array3D(Int32 length, Boolean transpose = false)
    {
        Debug.Assert(length > 0);

        array = new T[length * length * length];
        Length = length;

        (xFactor, yFactor, zFactor) = !transpose ? (length * length, length, 1) : (1, length, length * length);
    }

    /// <summary>
    ///     Get the length of each dimension.
    /// </summary>
    public Int32 Length { get; }

    /// <summary>
    ///     Access the element at the given position.
    /// </summary>
    /// <param name="x">The x coordinate. Must be between 0 and <see cref="Length" /> - 1.</param>
    /// <param name="y">The y coordinate. Must be between 0 and <see cref="Length" /> - 1.</param>
    /// <param name="z">The z coordinate. Must be between 0 and <see cref="Length" /> - 1.</param>
    public T this[Int32 x, Int32 y, Int32 z]
    {
        get => GetRef(x, y, z);
        set => GetRef(x, y, z) = value;
    }

    /// <summary>
    ///     Access the element at the given position.
    /// </summary>
    /// <param name="position">The position. All components must be between 0 and <see cref="Length" /> - 1.</param>
    #pragma warning disable S3876 // Vector3i is a fitting near-primitive type.
    public T this[Vector3i position]
    #pragma warning restore S3876
    {
        get => GetRef(position.X, position.Y, position.Z);
        set => GetRef(position.X, position.Y, position.Z) = value;
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
    /// Get the array as a span.
    /// </summary>
    /// <returns>The array as a span.</returns>
    public Span<T> AsSpan() => array;

    private ref T GetRef(Int32 x, Int32 y, Int32 z)
    {
        Debug.Assert(x >= 0 && x < Length);
        Debug.Assert(y >= 0 && y < Length);
        Debug.Assert(z >= 0 && z < Length);

        return ref array[x * xFactor + y * yFactor + z * zFactor];
    }
}
