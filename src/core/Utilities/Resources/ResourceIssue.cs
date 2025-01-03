// <copyright file="ResourceError.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics.CodeAnalysis;

namespace VoxelGame.Core.Utilities.Resources;

/// <summary>
///     The level of a resource issue.
/// </summary>
public enum Level
{
    /// <summary>
    ///     A warning - the game can still be played.
    ///     This means the missing resource is optional.
    /// </summary>
    Warning,

    /// <summary>
    ///     An error - the game cannot be played.
    ///     This means the missing resource is mandatory.
    /// </summary>
    Error
}

/// <summary>
///     An issue that occurred while loading a resource.
/// </summary>
public class ResourceIssue
{
    private ResourceIssue(Level level, String message, Exception? exception = null)
    {
        Level = level;
        Message = message;
        Exception = exception;
    }

    /// <summary>
    ///     An optional exception that caused the issue.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    ///     The issue message.
    /// </summary>
    public String Message { get; }

    /// <summary>
    ///     The level of the issue.
    /// </summary>
    public Level Level { get; }

    /// <summary>
    ///     Create a new resource issue from an exception.
    /// </summary>
    /// <param name="level">The level of the issue.</param>
    /// <param name="exception">The exception that caused the issue.</param>
    /// <returns>The resource issue, or <c>null</c> if the exception is <c>null</c>.</returns>
    [return: NotNullIfNotNull(nameof(exception))]
    public static ResourceIssue? FromException(Level level, Exception? exception)
    {
        return exception == null ? null : From(level, exception);
    }

    /// <summary>
    ///     Create a new resource issue from a message.
    /// </summary>
    /// <param name="level">The level of the issue.</param>
    /// <param name="message">The issue message.</param>
    /// <returns>The resource issue, or <c>null</c> if the message is <c>null</c>.</returns>
    [return: NotNullIfNotNull(nameof(message))]
    public static ResourceIssue? FromMessage(Level level, String? message)
    {
        return message == null ? null : From(level, message);
    }

    /// <summary>
    ///     Create a new resource issue from a message and an exception.
    /// </summary>
    /// <param name="level">The level of the issue.</param>
    /// <param name="message">The issue message.</param>
    /// <param name="exception">The exception that caused the issue.</param>
    /// <returns>The resource issue, or <c>null</c> if both the message and the exception are <c>null</c>.</returns>
    public static ResourceIssue From(Level level, String message, Exception? exception = null)
    {
        return new ResourceIssue(level, message, exception);
    }

    /// <summary>
    ///     Create a new resource issue from an exception.
    /// </summary>
    /// <param name="level">The level of the issue.</param>
    /// <param name="exception">The exception that caused the issue.</param>
    /// <returns>The resource issue.</returns>
    public static ResourceIssue From(Level level, Exception exception)
    {
        return new ResourceIssue(level, exception.Message, exception);
    }
}
