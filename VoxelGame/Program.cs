// <copyright file="World.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Desktop;
using System;
using Microsoft.Extensions.Logging;
using VoxelGame.Resources.Language;
using System.IO;
using System.Threading;
using VoxelGame.Utilities;

namespace VoxelGame
{
    internal static class Program
    {
        public static string Version { get; private set; } = null!;

        private static ILoggerFactory LoggerFactory { get; set; } = null!;

        [STAThread]
#if DEBUG
        private static void Main()
#else
        private static void Main(string[] args)
#endif
        {
            string appDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "voxel");
            string screenshotDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "VoxelGame");

            Directory.CreateDirectory(appDataDirectory);
            Directory.CreateDirectory(screenshotDirectory);

            bool logDebug;

#if DEBUG
            logDebug = true;
#else
            logDebug = args.Length > 0 && args[0] == "-logDebug";
#endif

            ILogger logger = SetupLogging(logDebug, appDataDirectory);

#if !DEBUG
            logger.LogInformation(logDebug ? "Debug will be logged." : "Debug will not be logged. Use '-logDebug' to log debug messages.");
#endif

            Version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "[VERSION UNAVAILABLE]";
            Console.Title = Language.VoxelGame + " " + Version;

            Console.WriteLine(Language.StartingGame);
            Console.WriteLine(Language.Version + " " + Version);

            GameWindowSettings gameWindowSettings = new GameWindowSettings
            {
                IsMultiThreaded = false,
                RenderFrequency = Config.GetDouble("maxFps", fallback: 60.0, min: 0.0),
                UpdateFrequency = 60.0
            };

            NativeWindowSettings nativeWindowSettings = NativeWindowSettings.Default;
            nativeWindowSettings.Title = Language.VoxelGame + " " + Version;
            nativeWindowSettings.Size = new Vector2i(800, 450);

            logger.LogInformation("Starting game on version: {Version}", Version);

            using (Game game = new Game(gameWindowSettings, nativeWindowSettings, appDataDirectory, screenshotDirectory))
            {
                game.Run();
            }

            Thread.Sleep(100);

            Console.WriteLine();
            Console.WriteLine(Language.PressAnyKeyToExit);
            Console.ReadKey(true);

            logger.LogInformation("Exiting game.");

            LoggerFactory.Dispose();
        }

        private static ILogger SetupLogging(bool logDebug, string appDataDirectory)
        {
            LogLevel level = logDebug ? LogLevel.Debug : LogLevel.Information;

            LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("VoxelGame", level)
                    .AddConsole(options => options.IncludeScopes = true)
                    .AddFile(Path.Combine(appDataDirectory, "Logs", $"voxel-log-{{Date}}{DateTime.Now:_HH-mm-ss}.log"), level);
            });

            return LoggerFactory.CreateLogger(nameof(Program));
        }

        public static ILogger CreateLogger<T>()
        {
            return LoggerFactory.CreateLogger<T>();
        }
    }
}