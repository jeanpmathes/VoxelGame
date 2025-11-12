// <copyright file="ExceptionTools.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using VoxelGame.Logging;

namespace VoxelGame.Toolkit.Utilities;

/// <summary>
///     Utility for throwing exceptions.
/// </summary>
public partial class ExceptionTools
{
    #pragma warning disable
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private static ExceptionTools instance = new();
    #pragma warning restore

    private ExceptionTools() {}

    /// <summary>
    ///     Handle an incorrectly disposed object, meaning an object that was disposed by the GC.
    /// </summary>
    /// <typeparam name="T">The type of the object that was incorrectly disposed.</typeparam>
    /// <param name="subject">The subject that was not disposed.</param>
    /// <param name="source">Where the object was created.</param>
    // Intentionally not conditional.
    public static void ThrowForMissedDispose<T>(T? subject = default, String? source = null) where T : notnull
    {
        LogMissedDispose(logger, Reflections.GetLongName<T>(), subject?.ToString(), source ?? "unknown");

        Debugger.Break();
    }

    /// <summary>
    ///     Throw an exception if an object is disposed.
    /// </summary>
    /// <param name="disposed">Whether the object is disposed.</param>
    /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
    [Conditional("DEBUG")]
    public static void ThrowIfDisposed(Boolean disposed)
    {
        if (!disposed) return;

        String? obj = new StackTrace().GetFrame(index: 1)?.GetMethod()?.ReflectedType?.Name;

        throw new ObjectDisposedException(obj);
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<ExceptionTools>();

    [LoggerMessage(EventId = LogID.Throw + 0, Level = LogLevel.Warning, Message = "Object of type '{Type}' ({Object}) was incorrectly disposed, it was created at: {Source}")]
    private static partial void LogMissedDispose(ILogger logger, String type, String? @object, String source);

    #endregion LOGGING
}
