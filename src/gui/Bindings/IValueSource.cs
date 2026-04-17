// <copyright file="IValueSource.cs" company="VoxelGame">
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

namespace VoxelGame.GUI.Bindings;

/// <summary>
///     Represents a source of values that can notify listeners when the value changes.
/// </summary>
public interface IValueSource
{
    /// <summary>
    ///     The event that is raised when the value changes.
    /// </summary>
    public event EventHandler? ValueChanged;
}

/// <summary>
///     Represents a source of values of type T.
/// </summary>
/// <typeparam name="T">The type of the value provided by this source.</typeparam>
public interface IValueSource<out T> : IValueSource
{
    /// <summary>
    ///     Gets the current value.
    /// </summary>
    /// <returns>The current value.</returns>
    public T GetValue();
}

/// <summary>
///     Represents a parametrized source of values, where the value can depend on an input of type TIn. The output value is
///     of type TOut.
/// </summary>
/// <typeparam name="TIn">The type of the input parameter that the value depends on.</typeparam>
/// <typeparam name="TOut">The type of the value provided by this source.</typeparam>
public interface IValueSource<in TIn, out TOut> : IValueSource
{
    /// <summary>
    ///     Gets the current value for the given input.
    /// </summary>
    /// <param name="input">The input parameter that the value depends on.</param>
    /// <returns>The current value for the given input.</returns>
    public TOut GetValue(TIn input);
}
