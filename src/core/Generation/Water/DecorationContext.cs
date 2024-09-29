// <copyright file="DecorationContext.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Generation.Water;

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
