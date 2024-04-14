// <copyright file="Disposer.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     Utility class to create a <see cref="IDisposable" /> object from an action.
/// </summary>
public sealed class Disposer : IDisposable
{
    private readonly Action dispose;

    private Boolean disposed;

    /// <summary>
    ///     Create a new <see cref="Disposer" />.
    /// </summary>
    /// <param name="dispose">The dispose action, will only be called if the dispose method is called.</param>
    public Disposer(Action dispose)
    {
        this.dispose = dispose;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing) dispose();
        else Throw.ForMissedDispose(nameof(Disposer));

        disposed = true;
    }

    /// <summary>
    ///     The finalizer.
    /// </summary>
    ~Disposer()
    {
        Dispose(disposing: false);
    }
}
