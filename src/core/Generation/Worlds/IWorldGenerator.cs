// <copyright file="IWorldGenerator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.IO;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Generation.Worlds;

/// <summary>
///     Generates a world.
/// </summary>
public interface IWorldGenerator : IDisposable
{
    /// <summary>
    ///     Get the map of the world.
    /// </summary>
    IMap Map { get; }

    /// <summary>
    ///     Get the resource catalog containing the resources the generator uses.
    /// </summary>
    static abstract ICatalogEntry CreateResourceCatalog();

    /// <summary>
    ///     Link all loaded resources so the generator can access them later.
    /// </summary>
    /// <param name="context">The context in which the resources are loaded.</param>
    static abstract void LinkResources(IResourceContext context);

    /// <summary>
    ///     Create an instance of the world generator.
    ///     Each instance is meant to generate a single world - a generator is stateful.
    /// </summary>
    /// <param name="context">The context in which the generator is created.</param>
    /// <returns>The world generator, or <c>null</c> if there are missing resources.</returns>
    static abstract IWorldGenerator? Create(IWorldGeneratorContext context);

    /// <summary>
    ///     Create a context in which chunks can be generated.
    ///     Must be called and disposed on the main thread.
    /// </summary>
    /// <param name="hint">A hint on which chunks will be generated with the context.</param>
    /// <returns>The generation context.</returns>
    IGenerationContext CreateGenerationContext(ChunkPosition hint);

    /// <summary>
    ///     Create a context in which decorations can be generated.
    ///     Must be called and disposed on the main thread.
    /// </summary>
    /// <param name="hint">A hint on which chunks will be decorated with the context.</param>
    /// <param name="extents">
    ///     A hint on th size of the neighborhood that is decorated, use 0 for single chunk and 1 for 3x3x3
    ///     chunks.
    /// </param>
    /// <returns>The decoration context.</returns>
    IDecorationContext CreateDecorationContext(ChunkPosition hint, Int32 extents = 0);

    /// <summary>
    ///     Emit views of global generated data for debugging.
    /// </summary>
    /// <param name="path">A path to the debug directory.</param>
    void EmitViews(DirectoryInfo path);

    /// <summary>
    ///     Search for named generated elements, such as structures.
    /// </summary>
    /// <param name="start">The start position.</param>
    /// <param name="name">The name of the element.</param>
    /// <param name="maxDistance">The maximum distance to search.</param>
    /// <returns>The positions of the elements, or null if the name is not valid.</returns>
    IEnumerable<Vector3i>? SearchNamedGeneratedElements(Vector3i start, String name, UInt32 maxDistance);
}
