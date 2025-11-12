// <copyright file="BaseGenerationContext.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Generation.Worlds;

/// <summary>
/// Base class for generation contexts.
/// </summary>
/// <param name="generator">The world generator.</param>
public abstract class BaseGenerationContext(IWorldGenerator generator) : IGenerationContext
{
    /// <inheritdoc />
    public IWorldGenerator Generator => generator;

    /// <inheritdoc />
    public abstract IEnumerable<Content> GenerateColumn(Int32 x, Int32 z, (Int32 start, Int32 end) heightRange);
    
    /// <inheritdoc />
    public virtual void GenerateStructures(Section section)
    {
        // No structures to generate.
    }
    
    #region DISPOSABLE

    private Boolean disposed;

    /// <summary>
    /// Overridable dispose method.
    /// </summary>
    /// <param name="disposing">Whether managed resources should be disposed.</param>
    protected virtual void Dispose(Boolean disposing)
    {
        if (disposed)
            return;

        if (!disposing)
            ExceptionTools.ThrowForMissedDispose(this);

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
    ~BaseGenerationContext()
    {
        Dispose(disposing: false);
    }

    #endregion DISPOSABLE
}
