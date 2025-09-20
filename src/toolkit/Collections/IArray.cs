// <copyright file="IArray.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Toolkit.Collections;

/// <summary>
///     Common interface for custom array types.
///     Supports multidimensional arrays where each dimension has the same length.
/// </summary>
/// <typeparam name="T">The type of the elements.</typeparam>
public interface IArray<T>
{
    /// <summary>
    ///     The length of the array, in one dimension.
    /// </summary>
    public Int32 Length { get; }

    /// <summary>
    ///     The total number of elements in the array.
    /// </summary>
    public Int32 Count { get; }

    /// <summary>
    ///     Access the array using a flat index.
    /// </summary>
    /// <param name="index">The flat index. Must be between 0 and <see cref="Count" /> - 1.</param>
    public T this[Int32 index] { get; set; }
}
