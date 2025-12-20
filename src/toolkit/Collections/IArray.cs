// <copyright file="IArray.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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
    Int32 Length { get; }

    /// <summary>
    ///     The total number of elements in the array.
    /// </summary>
    Int32 Count { get; }

    /// <summary>
    ///     Access the array using a flat index.
    /// </summary>
    /// <param name="index">The flat index. Must be between 0 and <see cref="Count" /> - 1.</param>
    T this[Int32 index] { get; set; }
}
