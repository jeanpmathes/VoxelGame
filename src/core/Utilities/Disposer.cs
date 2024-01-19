// <copyright file="Disposer.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using Microsoft.Extensions.Logging;
using VoxelGame.Logging;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     Utility class to create a <see cref="IDisposable" /> object from an action.
/// </summary>
public sealed class Disposer : IDisposable
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<Disposer>();

    private readonly Action dispose;

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

    private void Dispose(bool disposing)
    {
        if (disposing) dispose();
        else logger.LogWarning(Events.LeakedNativeObject, "A disposer was not disposed");
    }

    /// <summary>
    ///     The finalizer.
    /// </summary>
    ~Disposer()
    {
        Dispose(disposing: false);
    }
}
