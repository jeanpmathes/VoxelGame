﻿// <copyright file="World.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;
using Properties;
using VoxelGame.Client.Application;
using VoxelGame.Client.Application.Settings;
using VoxelGame.Core;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;
using VoxelGame.Support;
using VoxelGame.Support.Core;

[assembly: CLSCompliant(isCompliant: false)]
[assembly: ComVisible(visibility: false)]
[assembly: SupportedOSPlatform("windows11.0")]

namespace VoxelGame.Client;

internal static class Program
{
    /// <summary>
    ///     Get the version of the program.
    /// </summary>
    private static String Version { get; set; } = null!;

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

    /// <summary>
    ///     Get whether the program is running with code that was compiled in debug mode.
    /// </summary>
    internal static Boolean IsDebug { get; private set; }

    [STAThread]
    private static Int32 Main(String[] commandLineArguments)
    {
        SetDebugMode();

        // Creating the directories could technically cause an exception.
        // However, this would break a core assumption related to the special folders.

        AppDataDirectory = FileSystem.CreateSubdirectory(Environment.SpecialFolder.ApplicationData, "voxel");
        ScreenshotDirectory = FileSystem.CreateSubdirectory(Environment.SpecialFolder.MyPictures, "VoxelGame");
        StructureDirectory = FileSystem.CreateSubdirectory(Environment.SpecialFolder.MyDocuments, "VoxelGame", "Structures");

        WorldsDirectory = AppDataDirectory.CreateSubdirectory("Worlds");

        return Arguments.Handle(commandLineArguments,
            logging =>
            {
                ILogger logger = LoggingHelper.SetupLogging(nameof(Program), logging.LogDebug, AppDataDirectory);

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
            (args, logger) => Run(logger,
                () =>
                {
                    GraphicsSettings graphicsSettings = new(Settings.Default);

                    Profile.CreateGlobalInstance(args.Profile);

                    WindowSettings windowSettings = new WindowSettings
                    {
                        Title = Language.VoxelGame + " " + Version,
                        Size = graphicsSettings.WindowSize,
                        RenderScale = graphicsSettings.RenderResolutionScale,
                        SupportPIX = args.SupportPIX,
                        UseGBV = args.UseGBV
                    }.Corrected;

                    logger.LogDebug("Opening window");

                    Int32 result;

                    using (Application.Client client = new(windowSettings, graphicsSettings, args))
                    {
                        result = client.Run();
                    }

                    Profile.CreateExitReport();

                    return result;
                }));
    }


    [Conditional("DEBUG")]
    private static void SetDebugMode()
    {
        IsDebug = true;
    }

    #pragma warning disable S2221 // Goal is to catch any exception that might be unhandled.
    private static Int32 Run(ILogger logger, Func<Int32> runnable)
    {
        if (IsDebug) return runnable();

        try
        {
            return runnable();
        }
        catch (Exception exception)
        {
            logger.LogCritical(Events.ApplicationInformation, exception, "Unhandled exception, likely a bug");

            Dialog.ShowError($"Unhandled exception: {exception.Message}\n\n{exception.StackTrace}");

            return 1;
        }
    }
    #pragma warning restore S2221
}
