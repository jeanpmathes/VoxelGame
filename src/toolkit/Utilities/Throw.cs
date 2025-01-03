﻿// <copyright file="Throw.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using VoxelGame.Logging;

namespace VoxelGame.Toolkit.Utilities;

/// <summary>
///     Utility for throwing exceptions.
/// </summary>
public partial class Throw
{
    #pragma warning disable
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private static Throw instance = new();
    #pragma warning restore

    private Throw() {}

    /// <summary>
    ///     Handle an incorrectly disposed object, meaning an object that was disposed by the GC.
    /// </summary>
    /// <typeparam name="T">The type of the object that was incorrectly disposed.</typeparam>
    /// <param name="object">The object that was not disposed.</param>
    /// <param name="source">Where the object was created.</param>
    // Intentionally not conditional.
    public static void ForMissedDispose<T>(T? @object = default, String? source = null)
    {
        LogMissedDispose(logger, typeof(T).Name, @object, source ?? "unknown");

        Debugger.Break();
    }

    /// <summary>
    ///     Throw an exception if an object is disposed.
    /// </summary>
    /// <param name="disposed">Whether the object is disposed.</param>
    /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
    [Conditional("DEBUG")]
    public static void IfDisposed(Boolean disposed)
    {
        if (!disposed) return;

        String? obj = new StackTrace().GetFrame(index: 1)?.GetMethod()?.ReflectedType?.Name;

        throw new ObjectDisposedException(obj);
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<Throw>();

    [LoggerMessage(EventId = LogID.Throw + 0, Level = LogLevel.Warning, Message = "Object of type '{Type}' ({Object}) was incorrectly disposed, it was created at: {Source}")]
    private static partial void LogMissedDispose(ILogger logger, String type, Object? @object, String source);

    #endregion LOGGING
}
