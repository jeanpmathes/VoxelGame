// <copyright file="GenerationContext.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Logic.Voxels;

namespace VoxelGame.Core.Generation.Worlds.Standard;

/// <summary>
///     Implementation of <see cref="IGenerationContext" />.
/// </summary>
public sealed class GenerationContext(Generator generator, ChunkPosition hint) : BaseGenerationContext(generator)
{
    private ColumnSampleStore? columns = generator.GetColumns(hint);
    
    /// <inheritdoc />
    public override IEnumerable<Content> GenerateColumn(Int32 x, Int32 z, (Int32 start, Int32 end) heightRange)
    {
        ChunkPosition chunk = ChunkPosition.From((x, 0, z));
        columns ??= ColumnSampleStore.Sample(chunk.X, chunk.Z, generator);

        Debug.Assert(columns.Contains(chunk));

        return generator.GenerateColumn(x, z, heightRange, columns);
    }

    /// <inheritdoc />
    public override void GenerateStructures(Section section)
    {
        generator.GenerateStructures(section, columns);
    }

    #region DISPOSABLE

    private Boolean disposed;

    /// <inheritdoc />
    protected override void Dispose(Boolean disposing)
    {
        if (!disposed)
        {
            if (disposing && columns != null)
                generator.AddColumns(columns);   

            disposed = true;
        }

        base.Dispose(disposing);
    }

    #endregion DISPOSABLE
}
