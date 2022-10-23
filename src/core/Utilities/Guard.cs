// <copyright file="Guard.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using VoxelGame.Logging;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     Guards any held resource and calls appropriate methods on it when the guard is disposed.
/// </summary>
public sealed class Guard : IDisposable
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<Guard>();

    private readonly Action release;
    private readonly object resource;
    private readonly StackTrace? stackTrace;

    #region IDisposable Support

    /// <summary>
    ///     Create a new guard.
    /// </summary>
    /// <param name="resource">The resource to guard.</param>
    /// <param name="release">The method to call when the guard is disposed.</param>
    public Guard(object resource, Action release)
    {
        this.resource = resource;
        this.release = release;

        if (Debugger.IsAttached) stackTrace = new StackTrace(fNeedFileInfo: true);
    }

    /// <summary>
    ///     Dispose of this guard.
    /// </summary>
    private void Dispose(bool disposing)
    {
        if (!disposing) return;

        release();
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~Guard()
    {
        Dispose(disposing: false);
        WriteLog();
    }

    [Conditional("DEBUG")]
    private void WriteLog()
    {
        logger.LogWarning("Guard for resource {Resource} was not disposed. Guard was acquired {Stacktrace}", resource, stackTrace);
    }

    /// <summary>
    ///     Dispose of this chunk.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable Support
}
