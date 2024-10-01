// <copyright file="GenerationContext.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Generation.Default;

/// <summary>
///     Implementation of <see cref="IGenerationContext" />.
/// </summary>
public sealed class GenerationContext(Generator generator, ChunkPosition hint) : IGenerationContext
{
    private ColumnSampleStore? columns = generator.GetColumns(hint);

    /// <inheritdoc />
    public IWorldGenerator Generator => generator;

    /// <inheritdoc />
    public IEnumerable<Content> GenerateColumn(Int32 x, Int32 z, (Int32 start, Int32 end) heightRange)
    {
        ChunkPosition chunk = ChunkPosition.From((x, 0, z));
        columns ??= ColumnSampleStore.Sample(chunk.X, chunk.Z, generator);

        Debug.Assert(columns.Contains(chunk));

        return generator.GenerateColumn(x, z, heightRange, columns);
    }

    /// <inheritdoc />
    public void GenerateStructures(Section section)
    {
        generator.GenerateStructures(section, columns);
    }

    #region Disposable Support

    private Boolean disposed;

    private void Dispose(Boolean disposing)
    {
        if (disposed)
            return;

        if (!disposing)
            Throw.ForMissedDispose(this);

        if (columns != null)
            generator.AddColumns(columns);

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
    ~GenerationContext()
    {
        Dispose(disposing: false);
    }

    #endregion IDisposable Support
}
