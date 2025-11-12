// <copyright file="DecorationContext.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Generation.Worlds.Water;

/// <summary>
///     Implementation of <see cref="IDecorationContext" />.
/// </summary>
public sealed class DecorationContext(Generator generator) : IDecorationContext
{
    /// <inheritdoc />
    public IWorldGenerator Generator => generator;

    /// <inheritdoc />
    public void DecorateSection(Neighborhood<Section> sections)
    {
        // No decorations to place.
    }

    #region DISPOSABLE

    private Boolean disposed;

    private void Dispose(Boolean disposing)
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
    ~DecorationContext()
    {
        Dispose(disposing: false);
    }

    #endregion DISPOSABLE
}
