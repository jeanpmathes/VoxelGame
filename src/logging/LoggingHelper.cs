// <copyright file="Logging.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;

namespace VoxelGame.Logging
{
    /// <summary>
    ///     Utility class to create loggers.
    /// </summary>
    public static class LoggingHelper
    {
        private static ILoggerFactory LoggerFactory { get; set; } = null!;

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
        ///     Setup to the logging system.
        /// </summary>
        /// <param name="category">The category of the logging.</param>
        /// <param name="logDebug">Whether to log debug messages.</param>
        /// <param name="appDataDirectory">The application directory, in which a log folder is created.</param>
        /// <returns></returns>
        public static ILogger SetupLogging(string category, bool logDebug, string appDataDirectory)
        {
            Debug.Assert(LoggerFactory == null);

            LogLevel level = logDebug ? LogLevel.Debug : LogLevel.Information;

            LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(
                builder =>
                {
                    builder
                        .AddFilter("Microsoft", LogLevel.Warning)
                        .AddFilter("System", LogLevel.Warning)
                        .AddFilter("VoxelGame", level)
                        .AddSimpleConsole(options => options.IncludeScopes = true)
                        .AddFile(
                            Path.Combine(appDataDirectory, "Logs", $"voxel-log-{{Date}}{DateTime.Now:_HH-mm-ss}.log"),
                            level);
                });

            return LoggerFactory.CreateLogger(category);
        }
    }
}