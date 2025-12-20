// <copyright file="Registry.cs" company="VoxelGame">
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
using System.Collections.Generic;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     Utility to easily create instances and collect them at the same time.
/// </summary>
/// <typeparam name="T">The type of the instances.</typeparam>
public class Registry<T> where T : class
{
    private readonly Func<T, String> keySelector;
    private readonly Dictionary<String, T> valueByKey = [];

    private readonly List<T> values = [];

    /// <summary>
    ///     Create a new registry.
    /// </summary>
    /// <param name="keySelector">The key selector giving a unique key for each instance.</param>
    public Registry(Func<T, String> keySelector)
    {
        this.keySelector = keySelector;
    }

    /// <summary>
    ///     Get all registered instances.
    /// </summary>
    public IEnumerable<T> Values => values;

    /// <summary>
    ///     The amount of registered instances.
    /// </summary>
    public Int32 Count => values.Count;

    /// <summary>
    ///     Get an instance by its key.
    /// </summary>
    /// <param name="key">The key of the instance.</param>
    public T? this[String key] => valueByKey.GetValueOrDefault(key);

    /// <summary>
    ///     Get an instance by its index.
    /// </summary>
    /// <param name="index">The index of the instance.</param>
    public T this[Int32 index] => values[index];

    /// <summary>
    ///     Register a new instance.
    /// </summary>
    /// <param name="value">The instance to register.</param>
    /// <returns>The registered instance.</returns>
    public T Register(T value)
    {
        values.Add(value);
        valueByKey[keySelector(value)] = value;

        return value;
    }

    /// <summary>
    ///     Register a new instance and return it as a specific type.
    /// </summary>
    /// <param name="value">The instance to register.</param>
    /// <typeparam name="TS">The specific type of the instance.</typeparam>
    /// <returns>The registered instance as the specific type.</returns>
    public TS Register<TS>(TS value) where TS : T
    {
        Register((T) value);

        return value;
    }
}
