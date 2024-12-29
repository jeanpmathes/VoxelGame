// <copyright file="Registry.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;

namespace VoxelGame.Core.Utilities;

/// <summary>
/// Utility to easily create instances and collect them at the same time.
/// </summary>
/// <typeparam name="T">The type of the instances.</typeparam>
public class Registry<T> where T : class
{
    private readonly Func<T, String> keySelector;

    private readonly List<T> values = [];
    private readonly Dictionary<String, T> valueByKey = [];

    /// <summary>
    /// Create a new registry.
    /// </summary>
    /// <param name="keySelector">The key selector giving a unique key for each instance.</param>
    public Registry(Func<T, String> keySelector)
    {
        this.keySelector = keySelector;
    }

    /// <summary>
    /// Get all registered instances.
    /// </summary>
    public IEnumerable<T> Values => values;

    /// <summary>
    /// The amount of registered instances.
    /// </summary>
    public Int32 Count => values.Count;

    /// <summary>
    /// Get an instance by its key.
    /// </summary>
    /// <param name="key">The key of the instance.</param>
    public T? this[String key] => valueByKey.GetValueOrDefault(key);

    /// <summary>
    /// Get an instance by its index.
    /// </summary>
    /// <param name="index">The index of the instance.</param>
    public T this[Int32 index] => values[index];

    /// <summary>
    /// Register a new instance.
    /// </summary>
    /// <param name="value">The instance to register.</param>
    /// <returns>The registered instance.</returns>
    public T Register(T value)
    {
        values.Add(value);
        valueByKey[keySelector(value)] = value;

        return value;
    }
}
