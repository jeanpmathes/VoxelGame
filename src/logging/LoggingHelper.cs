// <copyright file="Logging.cs" company="VoxelGame">
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
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VoxelGame.Annotations.Attributes;

namespace VoxelGame.Logging;

/// <summary>
///     Utility class to create loggers.
/// </summary>
public static partial class LoggingHelper
{
    private static Boolean isMockLoggingSetUp;

    /// <summary>
    ///     Get the logger factory.
    /// </summary>
    [LateInitialization] public static partial ILoggerFactory LoggerFactory { get; set; }

    /// <summary>
    ///     Create a logger.
    /// </summary>
    /// <typeparam name="T">The class that will log to the logger.</typeparam>
    /// <returns>The logger.</returns>
    public static ILogger CreateLogger<T>()
    {
        return LoggerFactory.CreateLogger<T>();
    }

    /// <summary>
    ///     Create a logger.
    /// </summary>
    /// <param name="category">The category of the logging.</param>
    /// <returns>The logger.</returns>
    public static ILogger CreateLogger(String category)
    {
        return LoggerFactory.CreateLogger(category);
    }

    /// <summary>
    ///     Set up to the logging system.
    /// </summary>
    /// <param name="category">The category of the logging.</param>
    /// <param name="logDebug">Whether to log debug messages.</param>
    /// <param name="appDataDirectory">The application directory, in which a log folder is created.</param>
    /// <returns></returns>
    public static ILogger SetUpLogging(String category, Boolean logDebug, FileSystemInfo appDataDirectory)
    {
        LogLevel level = logDebug ? LogLevel.Debug : LogLevel.Information;

        LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddFilter("VoxelGame", level)
                .AddSimpleConsole(options => options.IncludeScopes = true)
                .AddFile(
                    Path.Combine(appDataDirectory.FullName, "Logs", $"voxel-log-{{Date}}{DateTime.Now:_HH-mm-ss}.log"),
                    level);
        });

        return LoggerFactory.CreateLogger(category);
    }

    /// <summary>
    ///     Set up a mock logger. All loggers created with this helper will be null loggers.
    /// </summary>
    /// <returns>A mock logger.</returns>
    public static void SetUpMockLogging()
    {
        if (isMockLoggingSetUp) return;

        isMockLoggingSetUp = true;

        LoggerFactory = new NullLoggerFactory();
    }
}
