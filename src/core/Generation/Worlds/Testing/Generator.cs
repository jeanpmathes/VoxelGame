// <copyright file="Generator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.IO;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Generation.Worlds.Testing;

/// <summary>
///     This world generator creates a simple testing world with a single layer of blocks at Y-level 0.
/// </summary>
public sealed class Generator : IWorldGenerator
{
    private readonly Content empty = Content.Default;
    private readonly Content ground = new(Blocks.Instance.Core.Dev);

    /// <inheritdoc />
    public IMap Map { get; } = new Map();

    /// <inheritdoc />
    public static ICatalogEntry CreateResourceCatalog()
    {
        return new Catalog();
    }

    /// <inheritdoc />
    public static void LinkResources(IResourceContext context)
    {
        // No resources to link.
    }

    /// <inheritdoc />
    public static IWorldGenerator Create(IWorldGeneratorContext context)
    {
        return new Generator();
    }

    /// <inheritdoc />
    public IGenerationContext CreateGenerationContext(ChunkPosition hint)
    {
        return new GenerationContext(this);
    }

    /// <inheritdoc />
    public IDecorationContext CreateDecorationContext(ChunkPosition hint, Int32 extents = 0)
    {
        return new DecorationContext(this);
    }

    /// <inheritdoc />
    public Operation EmitWorldInfo(DirectoryInfo path)
    {
        return Operations.CreateDone();
    }

    /// <inheritdoc />
    public IEnumerable<Vector3i>? SearchNamedGeneratedElements(Vector3i start, String name, UInt32 maxDistance)
    {
        #pragma warning disable S1168 // A null-return indicates that the name is not valid, which is different from not finding anything.
        return null;
        #pragma warning restore S1168
    }

    #region DISPOSABLE

    /// <inheritdoc />
    public void Dispose()
    {
        // Nothing to dispose of.
    }

    #endregion DISPOSABLE

    /// <inheritdoc cref="IGenerationContext.GenerateColumn" />
    public IEnumerable<Content> GenerateColumn(Int32 x, Int32 z, (Int32 start, Int32 end) heightRange)
    {
        for (Int32 y = heightRange.start; y < heightRange.end; y++) yield return GenerateContent((x, y, z));
    }

    private Content GenerateContent(Vector3i position)
    {
        return position.Y == 0 ? ground : empty;
    }
}
