// <copyright file="Program.cs" company="VoxelGame">
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
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Utilities;
using VoxelGame.Graphics;
using VoxelGame.Graphics.Core;
using VoxelGame.Logging;

[assembly: CLSCompliant(isCompliant: false)]
[assembly: ComVisible(visibility: false)]
[assembly: SupportedOSPlatform("windows11.0")]

namespace VoxelGame.Client;

internal static partial class Program
{
    /// <summary>
    ///     Get the version of the program.
    /// </summary>
    private static Version Version { get; set; } = null!;

    /// <summary>
    ///     Get whether the program is running with code that was compiled in debug mode.
    /// </summary>
    private static Boolean IsDebug { get; set; }

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
            IsDebug,
            logging =>
            {
                ILogger logger = LoggingHelper.SetUpLogging(nameof(Program), logging.LogDebug, AppDataDirectory);

                if (logging.LogDebug) LogDebugMessages(logger);
                else LogDebugMessagesNotLogged(logger);

                Version = typeof(Program).Assembly.GetName().Version ?? new Version("0.0.0.1");
                System.Console.Title = Language.VoxelGame + @" " + Version;

                LogStartingGame(logger, Version);

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

                    LogOpeningWindow(logger);

                    Int32 result;

                    using (Application.Client client = new(windowSettings, graphicsSettings, args, Version))
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

    private static Int32 Run(ILogger logger, Func<Int32> runnable)
    {
        using ILoggerFactory factory = LoggingHelper.LoggerFactory;
        
        if (IsDebug) return runnable();

        try
        {
            return runnable();
        }
        catch (Exception exception)
        {
            LogUnhandledException(logger, exception);

            Dialog.ShowError($"Unhandled exception: {exception.Message}\n\n{exception.StackTrace}");

            return 1;
        }
    }

    #region LOGGING

    [LoggerMessage(EventId = LogID.Program + 0, Level = LogLevel.Debug, Message = "Logging debug messages")]
    private static partial void LogDebugMessages(ILogger logger);

    [LoggerMessage(EventId = LogID.Program + 1, Level = LogLevel.Information, Message = "Debug messages will not be logged - use the respective argument to log debug messages")]
    private static partial void LogDebugMessagesNotLogged(ILogger logger);

    [LoggerMessage(EventId = LogID.Program + 2, Level = LogLevel.Information, Message = "Starting game on version: {Version}")]
    private static partial void LogStartingGame(ILogger logger, Version version);

    [LoggerMessage(EventId = LogID.Program + 3, Level = LogLevel.Debug, Message = "Opening window")]
    private static partial void LogOpeningWindow(ILogger logger);

    [LoggerMessage(EventId = LogID.Program + 4, Level = LogLevel.Critical, Message = "Unhandled exception, likely a bug")]
    private static partial void LogUnhandledException(ILogger logger, Exception exception);

    #endregion LOGGING
}
