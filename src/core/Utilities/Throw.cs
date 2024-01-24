// <copyright file="Throw.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     Utility class for throwing exceptions.
/// </summary>
public static class Throw
{
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
}
