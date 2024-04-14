// <copyright file="Array3D.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Collections;

/// <summary>
///     A 3D array where each dimension has the same length.
/// </summary>
/// <typeparam name="T">The type of the elements.</typeparam>
public class Array3D<T> : IEnumerable<T>
{
    private readonly T[] array;

    /// <summary>
    ///     Create a new array with the given length in all dimensions.
    /// </summary>
    /// <param name="length">The length of all dimensions. Must be greater than 0.</param>
    public Array3D(Int32 length)
    {
        Debug.Assert(length > 0);

        array = new T[length * length * length];
        Length = length;
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
    ///     Get all indices of the array.
    /// </summary>
    public IEnumerable<(Int32 x, Int32 y, Int32 z)> Indices => VMath.Range3(Length, Length, Length);

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator()
    {
        return ((IEnumerable<T>) array).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private ref T GetRef(Int32 x, Int32 y, Int32 z)
    {
        Debug.Assert(x >= 0 && x < Length);
        Debug.Assert(y >= 0 && y < Length);
        Debug.Assert(z >= 0 && z < Length);

        return ref array[x * Length * Length + y * Length + z];
    }

    /// <summary>
    ///     Get the value at the given position.
    /// </summary>
    /// <param name="position">The position. All coordinates must be between 0 and <see cref="Length" /> - 1.</param>
    /// <returns>The value at the given position.</returns>
    public T GetAt(Vector3i position)
    {
        return GetRef(position.X, position.Y, position.Z);
    }

    /// <summary>
    ///     Set the value at the given position.
    /// </summary>
    /// <param name="position">The position. All coordinates must be between 0 and <see cref="Length" /> - 1.</param>
    /// <param name="value">The value to set.</param>
    public void SetAt(Vector3i position, T value)
    {
        GetRef(position.X, position.Y, position.Z) = value;
    }
}
