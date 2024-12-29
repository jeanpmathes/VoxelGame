// <copyright file="CollectionExtensions.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Linq;

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

    /// <summary>
    ///     A fast(er) reverse iteration over a list.
    /// </summary>
    public static IEnumerable<T> FastReverse<T>(this IList<T> list)
    {
        for (Int32 i = list.Count - 1; i >= 0; i--) yield return list[i];
    }

    /// <summary>
    /// A simple method to filter out null values from an enumerable.
    /// </summary>
    /// <param name="enumerable">The enumerable to filter.</param>
    /// <typeparam name="T">The type of the enumerable.</typeparam>
    /// <returns>The filtered enumerable.</returns>
    #pragma warning disable S3242
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable) where T : class
    #pragma warning restore S3242
    {
        return enumerable.OfType<T>();
    }
}
