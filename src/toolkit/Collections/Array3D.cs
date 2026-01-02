// <copyright file="Array3D.cs" company="VoxelGame">
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
using JetBrains.Annotations;
using OpenTK.Mathematics;

namespace VoxelGame.Toolkit.Collections;

/// <summary>
///     A 3D array where each dimension has the same length.
///     Internally, the array is stored as an 1D array.
/// </summary>
/// <typeparam name="T">The type of the elements.</typeparam>
public class Array3D<T> : IEnumerable<T>, IArray<T>
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
    ///     Get the total number of elements in the array.
    /// </summary>
    protected Int32 Count => array.Length;

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

    /// <summary>
    ///     Get the length of each dimension.
    /// </summary>
    public Int32 Length { get; }

    Int32 IArray<T>.Count => Count;

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

    private ref T GetRef(Int32 x, Int32 y, Int32 z)
    {
        Debug.Assert(x >= 0 && x < Length);
        Debug.Assert(y >= 0 && y < Length);
        Debug.Assert(z >= 0 && z < Length);

        return ref array[x * xFactor + y * yFactor + z * zFactor];
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
