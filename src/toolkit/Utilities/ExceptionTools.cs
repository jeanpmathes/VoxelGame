// <copyright file="ExceptionTools.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
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
