// <copyright file="World.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using Properties;
using VoxelGame.Client.Application;
using VoxelGame.Core;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;
#if !DEBUG
using System.Threading;
#endif

[assembly: CLSCompliant(isCompliant: false)]
[assembly: ComVisible(visibility: false)]

namespace VoxelGame.Client;

internal static class Program
{
    /// <summary>
    ///     Get the version of the program.
    /// </summary>
    internal static string Version { get; private set; } = null!;

    /// <summary>
    ///     Get the app data directory.
    /// </summary>
    internal static string AppDataDirectory { get; private set; } = null!;

    /// <summary>
    ///     Get the screenshot directory.
    /// </summary>
    internal static string ScreenshotDirectory { get; private set; } = null!;

    /// <summary>
    ///     Get the directory structures are exported to.
    /// </summary>
    internal static string StructureDirectory { get; private set; } = null!;

    /// <summary>
    ///     Get the world directory.
    /// </summary>
    internal static string WorldsDirectory { get; private set; } = null!;

    [STAThread]
#if DEBUG
    private static void Main()
#else
    private static void Main(string[] args)
#endif
    {
        AppDataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "voxel");

        ScreenshotDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
            "VoxelGame");

        StructureDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "VoxelGame",
            "Structures");

        WorldsDirectory = Path.Combine(AppDataDirectory, "Worlds");

        Directory.CreateDirectory(AppDataDirectory);
        Directory.CreateDirectory(ScreenshotDirectory);
        Directory.CreateDirectory(StructureDirectory);
        Directory.CreateDirectory(WorldsDirectory);

#if DEBUG
        const bool logDebug = true;
#else
        bool logDebug = args.Length > 0 && args[0] == "-logDebug";
#endif

        ILogger logger = LoggingHelper.SetupLogging(nameof(Program), logDebug, AppDataDirectory);

#if !DEBUG
        AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
        {
            logger.LogCritical(Events.ApplicationInformation, eventArgs.ExceptionObject as Exception, "Unhandled exception, likely a bug. Terminating: {Exit}", eventArgs.IsTerminating);

            // The runtime will emit a message, to prevent mixing we wait.
            Thread.Sleep(millisecondsTimeout: 100);
        };
#endif

#if !DEBUG
        if (logDebug) logger.LogInformation(Events.Meta, "Logging debug messages");
        else
            logger.LogInformation(
                Events.Meta,
                "Debug messages will not be logged. Use '-logDebug' to log debug messages");
#endif

        Version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "[VERSION UNAVAILABLE]";
        ApplicationInformation.Initialize(Version);
        System.Console.Title = Language.VoxelGame + @" " + Version;

        logger.LogInformation(Events.ApplicationInformation, "Starting game on version: {Version}", Version);

        GraphicsSettings graphicsSettings = new(Settings.Default);

        GameWindowSettings gameWindowSettings = new()
        {
            RenderFrequency = graphicsSettings.MaxFPS,
            UpdateFrequency = 60.0
        };

        var nativeWindowSettings = NativeWindowSettings.Default;
        nativeWindowSettings.WindowBorder = WindowBorder.Hidden;
        nativeWindowSettings.Profile = ContextProfile.Core;
        nativeWindowSettings.Title = Language.VoxelGame + " " + Version;
        nativeWindowSettings.Size = Settings.Default.ScreenSize.ToVector2i();
        nativeWindowSettings.StartFocused = true;

        logger.LogDebug("Opening window");

        using (Application.Client client = new(
                   gameWindowSettings,
                   nativeWindowSettings,
                   graphicsSettings))
        {
            client.Run();
        }

        logger.LogInformation(Events.ApplicationState, "Exiting");
    }
}
