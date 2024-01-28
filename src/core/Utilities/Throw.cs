// <copyright file="Throw.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using VoxelGame.Logging;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     Utility class for throwing exceptions.
/// </summary>
public class Throw
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<Throw>();

    #pragma warning disable
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private static Throw instance = new();
    #pragma warning restore

    private Throw() {}

    /// <summary>
    ///     Throw an exception if an object is disposed.
    /// </summary>
    /// <param name="disposed">Whether the object is disposed.</param>
    /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
    [Conditional("DEBUG")]
    public static void IfDisposed(bool disposed)
    {
        if (!disposed) return;

        string? obj = new StackTrace().GetFrame(index: 1)?.GetMethod()?.ReflectedType?.Name;

        throw new ObjectDisposedException(obj);
    }

    /// <summary>
    ///     Throw an exception if an object is null.
    /// </summary>
    /// <param name="obj">The object to check.</param>
    /// <param name="message">The message to throw.</param>
    /// <exception cref="ArgumentNullException">Thrown if the object is null.</exception>
    [Conditional("DEBUG")]
    public static void IfNull([NotNull] object? obj, string message = "")
    {
        if (obj is null) throw new ArgumentNullException(message);
    }

    /// <summary>
    ///     Ensure that the current thread is the main thread.
    /// </summary>
    /// <returns>True if the current thread is the main thread.</returns>
    [Conditional("DEBUG")]
    public static void IfNotOnMainThread(object @object, [CallerMemberName] string operation = "")
    {
        if (ApplicationInformation.Instance.IsOnMainThread) return;

        Debug.Fail($"Attempted to perform operation '{operation}' with object '{@object}' from non-main thread");
    }


    /// <summary>
    ///     Handle a incorrectly disposed object, meaning an object that was disposed by the GC.
    /// </summary>
    /// <param name="type">The type of the object that was not disposed.</param>
    /// <param name="object">The object that was not disposed.</param>
    /// <param name="trace">The stack trace of object creation.</param>
    // Intentionally not conditional.
    public static void ForMissedDispose(string type, object? @object = null, StackTrace? trace = null)
    {
        logger.LogWarning(Events.Dispose, "Object of type '{Type}' ({Object}) was incorrectly disposed, it was created at:\n{Trace}", type, @object, trace);

        Debugger.Break();
    }
}
