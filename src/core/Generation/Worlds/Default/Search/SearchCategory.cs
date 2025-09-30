// <copyright file="SearchCategory.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace VoxelGame.Core.Generation.Worlds.Default.Search;

/// <summary>
///     Base class for all search categories.
/// </summary>
/// <param name="searcher">The searcher that owns this category.</param>
public abstract class SearchCategory(Searcher searcher)
{
    /// <summary>
    ///     Get the searcher that owns this category.
    /// </summary>
    protected Searcher Searcher { get; } = searcher;

    /// <summary>
    ///     Get the generator that owns the searcher that owns this category.
    /// </summary>
    protected Generator Generator => Searcher.Generator;

    /// <summary>
    ///     See <see cref="Searcher.Search" />.
    /// </summary>
    public abstract IEnumerable<Vector3i>? Search(Vector3i start, String entity, String? modifier, UInt32 maxDistance);
}

/// <summary>
///     A category of things that can be searched for in the world.
/// </summary>
/// <param name="elements">The elements that can be searched for.</param>
/// <param name="modifiers">The modifiers that can be applied to the search.</param>
/// <param name="searcher">The searcher that owns this category.</param>
/// <typeparam name="T">The type of thing that can be searched for.</typeparam>
public abstract class SearchCategory<T>(Dictionary<String, T> elements, List<String> modifiers, Searcher searcher) : SearchCategory(searcher) where T : class
{
    private readonly HashSet<String> modifiers = [..modifiers];

    /// <inheritdoc />
    public override IEnumerable<Vector3i>? Search(Vector3i start, String entity, String? modifier, UInt32 maxDistance)
    {
        if (!IsModifierValid(modifier))
            return null;

        return elements.GetValueOrDefault(entity) is {} element
            ? SearchElement(element, modifier, start, maxDistance)
            : null;
    }

    private Boolean IsModifierValid(String? modifier)
    {
        return modifier == null || modifiers.Contains(modifier);
    }

    /// <summary>
    ///     Searches for the element in the world.
    /// </summary>
    /// <param name="element">The element to search for.</param>
    /// <param name="modifier">The search modifier.</param>
    /// <param name="start">The search start position.</param>
    /// <param name="maxBlockDistance">The maximum block distance to search.</param>
    /// <returns>All found positions that have the element.</returns>
    protected abstract IEnumerable<Vector3i> SearchElement(T element, String? modifier, Vector3i start, UInt32 maxBlockDistance);
}
