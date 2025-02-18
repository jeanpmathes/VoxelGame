// <copyright file="Set.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VoxelGame.Core.Collections;

/// <summary>
///     Utility class to create sets.
/// </summary>
public static class Set
{
    /// <summary>
    ///     Get a set that contains a single element.
    ///     The set is read-only.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <typeparam name="T">The type of the element.</typeparam>
    /// <returns>The set.</returns>
    public static IReadOnlySet<T> Of<T>(T element)
    {
        return new SingleSet<T>(element);
    }

    /// <summary>
    ///     Create a set that contains multiple elements.
    /// </summary>
    /// <param name="elements">The elements.</param>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <returns>The set.</returns>
    public static IReadOnlySet<T> Of<T>(params T[] elements)
    {
        return new HashSet<T>(elements);
    }

    private sealed class SingleSet<T>(T element) : IReadOnlySet<T>
    {
        public IEnumerator<T> GetEnumerator()
        {
            yield return element;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Int32 Count => 1;

        public Boolean Contains(T item)
        {
            return Equals(element, item);
        }

        public Boolean IsProperSubsetOf(IEnumerable<T> other)
        {
            var found = false;
            var count = 0;

            foreach (T entry in other)
            {
                count++;

                found |= Equals(element, entry);

                if (count > 1 && found)
                    return true;
            }

            return false;
        }

        public Boolean IsProperSupersetOf(IEnumerable<T> other)
        {
            return !other.Any();
        }

        public Boolean IsSubsetOf(IEnumerable<T> other)
        {
            return other.Contains(element);
        }

        public Boolean IsSupersetOf(IEnumerable<T> other)
        {
            return other.All(entry => Equals(element, entry));
        }

        public Boolean Overlaps(IEnumerable<T> other)
        {
            return other.Contains(element);
        }

        public Boolean SetEquals(IEnumerable<T> other)
        {
            var count = 0;

            foreach (T entry in other)
            {
                count++;

                if (!Equals(element, entry))
                    return false;
            }

            return count == 1;
        }
    }
}
