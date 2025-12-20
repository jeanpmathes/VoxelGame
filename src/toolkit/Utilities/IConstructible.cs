// <copyright file="IConstructible.cs" company="VoxelGame">
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

namespace VoxelGame.Toolkit.Utilities;

#pragma warning disable S2436 // Required for these fine interfaces.

/// <summary>
///     Allows construction of an object from an input argument.
/// </summary>
/// <remarks>
///     The <c>ConstructibleGenerator</c> expects this interface to stay at <c>VoxelGame.Toolkit.Utilities</c>.
/// </remarks>
/// <typeparam name="TIn">The type of the input argument.</typeparam>
/// <typeparam name="TOut">The type of the output object.</typeparam>
public interface IConstructible<in TIn, out TOut>
{
    /// <summary>
    ///     Constructs an object of type <typeparamref name="TOut" /> from the given input argument.
    /// </summary>
    /// <param name="input">The input argument of type <typeparamref name="TIn" />.</param>
    /// <returns>The constructed object of type <typeparamref name="TOut" />.</returns>
    static abstract TOut Construct(TIn input);
}

/// <summary>
///     Allows construction of an object from two input arguments.
/// </summary>
/// <typeparam name="TIn1">The type of the first input argument.</typeparam>
/// <typeparam name="TIn2">The type of the second input argument.</typeparam>
/// <typeparam name="TOut">The type of the output object.</typeparam>
public interface IConstructible<in TIn1, in TIn2, out TOut>
{
    /// <summary>
    ///     Constructs an object of type <typeparamref name="TOut" /> from the given input argument.
    /// </summary>
    /// <param name="input1">The first input argument of type <typeparamref name="TIn1" />.</param>
    /// <param name="input2">The second input argument of type <typeparamref name="TIn2" />.</param>
    /// <returns>The constructed object of type <typeparamref name="TOut" />.</returns>
    static abstract TOut Construct(TIn1 input1, TIn2 input2);
}
