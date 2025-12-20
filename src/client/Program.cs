// <copyright file="Program.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;
using Properties;
using VoxelGame.Annotations.Attributes;
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
    [LateInitialization] private static partial Version Version { get; set; }

    /// <summary>
    ///     Get whether the program is running with code that was compiled in debug mode.
    /// </summary>
    private static Boolean IsDebug { get; set; }

    /// <summary>
    ///     Get the app data directory.
    /// </summary>
    [LateInitialization] private static partial DirectoryInfo AppDataDirectory { get; set; }

    /// <summary>
    ///     Get the screenshot directory.
    /// </summary>
    [LateInitialization] internal static partial DirectoryInfo ScreenshotDirectory { get; private set; }

    /// <summary>
    ///     Get the directory structures are exported to.
    /// </summary>
    [LateInitialization] internal static partial DirectoryInfo StructureDirectory { get; private set; }

    /// <summary>
    ///     Get the world directory.
    /// </summary>
    [LateInitialization] internal static partial DirectoryInfo WorldsDirectory { get; private set; }

    [STAThread]
    private static Int32 Main(String[] commandLineArguments)
    {
        System.Console.WriteLine(@"VoxelGame Client Copyright (C) 2020-2025 Jean Patrick Mathes");
        System.Console.WriteLine(@"This program comes with ABSOLUTELY NO WARRANTY. This is free software, and you are welcome to redistribute it under certain conditions.");
        System.Console.WriteLine(@"See the LICENSE file in the project root for details.");
        System.Console.WriteLine();

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
