// <copyright file="ILoadingContext.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.IO;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     Base interface for the context in which loading operations can be performed.
///     The loading context is used to track progress and errors during loading operations.
/// </summary>
public interface ILoadingContext
{
    /// <summary>
    ///     Begin a loading step.
    /// </summary>
    /// <param name="name">The name of the step.</param>
    public IDisposable BeginStep(String name);

    /// <summary>
    ///     Report a successful loading operation.
    /// </summary>
    public void ReportSuccess(String type, String resource);

    /// <summary>
    ///     Report a successful loading operation.
    /// </summary>
    public void ReportSuccess(String type, FileSystemInfo resource)
    {
        ReportSuccess(type, resource.GetResourceRelativePath());
    }

    /// <summary>
    ///     Report a failed loading operation, which will make it impossible to start the game.
    /// </summary>
    public void ReportFailure(String type, String resource, Exception exception, Boolean abort = false);

    /// <summary>
    ///     Report a failed loading operation, which will make it impossible to start the game.
    /// </summary>
    public void ReportFailure(String type, FileSystemInfo resource, Exception exception, Boolean abort = false)
    {
        ReportFailure(type, resource.GetResourceRelativePath(), exception, abort);
    }

    /// <summary>
    ///     Report a failed loading operation, which will make it impossible to start the game.
    /// </summary>
    public void ReportFailure(String type, String resource, String message, Boolean abort = false);

    /// <summary>
    ///     Report a failed loading operation, which will make it impossible to start the game.
    /// </summary>
    public void ReportFailure(String type, FileSystemInfo resource, String message, Boolean abort = false)
    {
        ReportFailure(type, resource.GetResourceRelativePath(), message, abort);
    }

    /// <summary>
    ///     Warn about a failed loading operation. The game is still able to start.
    /// </summary>
    public void ReportWarning(String type, String resource, Exception exception);

    /// <summary>
    ///     Warn about a failed loading operation. The game is still able to start.
    /// </summary>
    public void ReportWarning(String type, FileSystemInfo resource, Exception exception)
    {
        ReportWarning(type, resource.GetResourceRelativePath(), exception);
    }

    /// <summary>
    ///     Warn about a failed loading operation. The game is still able to start.
    /// </summary>
    public void ReportWarning(String type, String resource, String message);

    /// <summary>
    ///     Warn about a failed loading operation. The game is still able to start.
    /// </summary>
    public void ReportWarning(String type, FileSystemInfo resource, String message)
    {
        ReportWarning(type, resource.GetResourceRelativePath(), message);
    }
}
