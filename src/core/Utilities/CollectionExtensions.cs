// <copyright file="CollectionExtensions.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

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
    /// <typeparam name="TK">The type of the key.</typeparam>
    /// <typeparam name="TV">The type of the value.</typeparam>
    /// <returns>The value.</returns>
    public static TV GetOrAdd<TK, TV>(this IDictionary<TK, TV> dictionary, TK key) where TV : new()
    {
        if (dictionary.TryGetValue(key, out TV? value)) return value;

        value = new TV();
        dictionary.Add(key, value);

        return value;
    }
}
