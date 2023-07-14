// <copyright file="World.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using Microsoft.Extensions.Logging;
using Properties;
using VoxelGame.Client.Application;
using VoxelGame.Core;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;
using VoxelGame.Support;

[assembly: CLSCompliant(isCompliant: false)]
[assembly: ComVisible(visibility: false)]
[assembly: SupportedOSPlatform("windows11.0")]

namespace VoxelGame.Client;

internal static class Program
{
    /// <summary>
    ///     Get the version of the program.
    /// </summary>
    private static string Version { get; set; } = null!;

    /// <summary>
    ///     Get the app data directory.
    /// </summary>
    private static DirectoryInfo AppDataDirectory { get; set; } = null!;

    /// <summary>
    ///     Get the screenshot directory.
    /// </summary>
    internal static DirectoryInfo ScreenshotDirectory { get; private set; } = null!;

    /// <summary>
    ///     Get the directory structures are exported to.
    /// </summary>
    internal static DirectoryInfo StructureDirectory { get; private set; } = null!;

    /// <summary>
    ///     Get the world directory.
    /// </summary>
    internal static DirectoryInfo WorldsDirectory { get; private set; } = null!;

    [STAThread]
    private static int Main(string[] commandLineArguments)
    {
        AppDataDirectory = FileSystem.CreateSubdirectory(Environment.SpecialFolder.ApplicationData, "voxel");
        ScreenshotDirectory = FileSystem.CreateSubdirectory(Environment.SpecialFolder.MyPictures, "VoxelGame");
        StructureDirectory = FileSystem.CreateSubdirectory(Environment.SpecialFolder.MyDocuments, "VoxelGame", "Structures");
        WorldsDirectory = FileSystem.CreateSubdirectory(AppDataDirectory, "Worlds");

        return Arguments.Handle(commandLineArguments,
            logging =>
            {
                ILogger logger = LoggingHelper.SetupLogging(nameof(Program), logging.LogDebug, AppDataDirectory);

                SetupExceptionHandler(logger);

                if (logging.LogDebug) logger.LogDebug(Events.Meta, "Logging debug messages");
                else
                    logger.LogInformation(
                        Events.Meta,
                        "Debug messages will not be logged. Use the respective argument to log debug messages");

                Version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "[VERSION UNAVAILABLE]";
                ApplicationInformation.Initialize(Version);
                System.Console.Title = Language.VoxelGame + @" " + Version;

                logger.LogInformation(Events.ApplicationInformation, "Starting game on version: {Version}", Version);

                return logger;
            },
            (args, logger) =>
            {
                GraphicsSettings graphicsSettings = new(Settings.Default);

                WindowSettings windowSettings = new WindowSettings
                {
                    Title = Language.VoxelGame + " " + Version,
                    Size = graphicsSettings.WindowSize
                }.Corrected; // todo: icon

                logger.LogDebug("Opening window");

                using Application.Client client = new(windowSettings, graphicsSettings, args);

                return client.Run();
            });
    }

    [Conditional("RELEASE")]
    private static void SetupExceptionHandler(ILogger logger)
    {
        AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
        {
            logger.LogCritical(Events.ApplicationInformation, eventArgs.ExceptionObject as Exception, "Unhandled exception, likely a bug. Terminating: {Exit}", eventArgs.IsTerminating);

            // The runtime will emit a message, to prevent mixing we wait.
            Thread.Sleep(millisecondsTimeout: 100);

            // todo: error window
        };
    }
}
