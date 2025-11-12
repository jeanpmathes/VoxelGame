// <copyright file="Guard.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     Guards any held resource and calls appropriate methods on it when the guard is disposed.
/// </summary>
public sealed class Guard : IDisposable
{
    private readonly Action release;
    private readonly Object resource;
    private readonly String? source;

    /// <summary>
    ///     Create a new guard.
    /// </summary>
    /// <param name="resource">The resource to guard.</param>
    /// <param name="source">Where the guard was created.</param>
    /// <param name="release">The method to call when the guard is disposed.</param>
    public Guard(Object resource, String source, Action release)
    {
        this.resource = resource;
        this.source = source;
        this.release = release;
    }

    /// <summary>
    ///     Check if the guard is guarding an object.
    /// </summary>
    /// <param name="object">The object to check.</param>
    /// <returns>True if the guard is guarding the resource.</returns>
    public Boolean IsGuarding(Object @object)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        return resource == @object;
    }

    #region DISPOSABLE

    private Boolean disposed;

    /// <summary>
    ///     Dispose of this guard.
    /// </summary>
    private void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing) release();
        else ExceptionTools.ThrowForMissedDispose(resource, source);

        disposed = true;
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~Guard()
    {
        Dispose(disposing: false);
    }

    /// <summary>
    ///     Dispose of this chunk.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion DISPOSABLE
}
