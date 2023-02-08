// <copyright file="CollectionExtensions.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;

namespace VoxelGame.Core.Utilities;

/**
 * Extensions for different collections.
 */
public static class CollectionExtensions
{
    /// <summary>
    ///     Get a value from a dictionary or add it if it does not exist.
    /// </summary>
    /// <param name="dictionary">The dictionary to get the value from.</param>
    /// <param name="key">The key of the value.</param>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <returns>The value.</returns>
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
    {
        if (dictionary.TryGetValue(key, out TValue? value)) return value;

        value = new TValue();
        dictionary.Add(key, value);

        return value;
    }

    /// <summary>
    ///     Get a value from a dictionary or add it if it does not exist.
    /// </summary>
    /// <param name="dictionary">The dictionary to get the value from.</param>
    /// <param name="key">The key of the value.</param>
    /// <param name="value">The value to add if the key does not exist.</param>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <returns>The value.</returns>
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, in TValue value)
    {
        if (dictionary.TryGetValue(key, out TValue? existingValue)) return existingValue;

        dictionary.Add(key, value);

        return value;
    }
}

