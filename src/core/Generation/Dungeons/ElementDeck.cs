// <copyright file="ElementDeck.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;

namespace VoxelGame.Core.Generation.Dungeons;

/// <summary>
///     A deck to draw elements from.
/// </summary>
/// <typeparam name="T">The type of elements in the deck.</typeparam>
public class ElementDeck<T> where T : class
{
    private readonly List<T> elements = [];

    /// <summary>
    ///     Create a new element deck.
    /// </summary>
    /// <param name="elements">The initial elements in the deck.</param>
    protected ElementDeck(IEnumerable<T> elements)
    {
        this.elements.AddRange(elements);
    }

    /// <summary>
    ///     Get the number of remaining elements in the deck.
    /// </summary>
    public Int32 Count => elements.Count;

    /// <summary>
    ///     Draw an element from the deck.
    /// </summary>
    /// <param name="random">The random number generator to use.</param>
    /// <returns>The drawn element.</returns>
    public T Draw(Random random)
    {
        Int32 index = random.Next(elements.Count);

        T room = elements[index];
        elements.RemoveAt(index);

        return room;
    }
}
