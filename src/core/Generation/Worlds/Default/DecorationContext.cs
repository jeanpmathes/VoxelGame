// <copyright file="DecorationContext.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Utilities;
using VoxelGame.Toolkit.Collections;

namespace VoxelGame.Core.Generation.Worlds.Default;

/// <summary>
///     Implementation of <see cref="IDecorationContext" />.
/// </summary>
public sealed class DecorationContext(Generator generator, ChunkPosition hint, Int32 extents) : IDecorationContext
{
    private (Array2D<ColumnSampleStore?> array, ChunkPosition anchor) columns = GetColumns(generator, hint, extents);

    /// <inheritdoc />
    public IWorldGenerator Generator => generator;

    /// <inheritdoc />
    public void DecorateSection(Neighborhood<Section> sections)
    {
        ChunkPosition chunk = sections.Center.Position.Chunk;

        Int32 xOffset = chunk.X - columns.anchor.X;
        Int32 zOffset = chunk.Z - columns.anchor.Z;

        ColumnSampleStore? store = null;

        if (columns.array.IsInBounds(xOffset, zOffset))
            store = columns.array[xOffset, zOffset];

        generator.DecorateSection(sections, store);
    }

    private static (Array2D<ColumnSampleStore?> array, ChunkPosition anchor) GetColumns(Generator generator, ChunkPosition hint, Int32 extents)
    {
        ChunkPosition anchor = new(hint.X - extents, y: 0, hint.Z - extents);
        Int32 chunkSize = 2 * extents + 1;

        Array2D<ColumnSampleStore?> columns = new(chunkSize);

        for (var x = 0; x < chunkSize; x++)
        for (var z = 0; z < chunkSize; z++)
            columns[x, z] = generator.GetColumns(anchor.Offset(x, yOffset: 0, z));

        return (columns, anchor);
    }

    #region Disposable Support

    private Boolean disposed;

    private void Dispose(Boolean disposing)
    {
        if (disposed)
            return;

        if (!disposing)
            Throw.ForMissedDispose(this);

        disposed = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~DecorationContext()
    {
        Dispose(disposing: false);
    }

    #endregion IDisposable Support
}
