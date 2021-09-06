// <copyright file="Logging.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;

namespace VoxelGame.Logging
{
    public static class LoggingHelper
    {
        private static ILoggerFactory LoggerFactory { get; set; } = null!;

        public static ILogger CreateLogger<T>()
        {
            return LoggerFactory.CreateLogger<T>();
        }

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