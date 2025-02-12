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
///     A category of things that can be searched for in the world.
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
