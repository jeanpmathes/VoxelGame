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

    private readonly string caller;
    private readonly int line;
    private readonly string path;
    private readonly Action release;
    private readonly object resource;

    #region IDisposable Support

    /// <summary>
    ///     Create a new guard.
    /// </summary>
    /// <param name="resource">The resource to guard.</param>
    /// <param name="release">The method to call when the guard is disposed.</param>
    /// <param name="caller">The name of the calling method that acquired the resource.</param>
    /// <param name="path">The path of the calling file.</param>
    /// <param name="line">The line of the calling file.</param>
    public Guard(object resource, Action release, string caller, string path, int line)
    {
        this.resource = resource;
        this.release = release;

        this.caller = caller;
        this.path = path;
        this.line = line;
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
        logger.LogWarning("Guard for resource {Resource} was not disposed. Guard was acquired by {Caller} in {Path} ({Line})", resource, caller, path, line);
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
