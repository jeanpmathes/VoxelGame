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
    internal class Program
    {
        public static string Version { get; private set; } = null!;

        private static ILoggerFactory LoggerFactory { get; set; } = null!;

        private static void Main(string[] args)
        {
            string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "voxel");

            bool logDebug = args.Length > 0 && args[0] == "-logDebug";

#if DEBUG
            logDebug = true;
#endif

            ILogger logger = SetupLogging(logDebug, appDataFolder);

            logger.LogInformation(logDebug ? "Debug will be logged." : "Debug will not be logged. Use '-logDebug' to log debug messages.");

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

            using (Game game = new Game(gameWindowSettings, nativeWindowSettings, appDataFolder))
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

        private static ILogger SetupLogging(bool logDebug, string appDataFolder)
        {
            LogLevel level = logDebug ? LogLevel.Debug : LogLevel.Information;

            LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("VoxelGame", level)
                    .AddConsole(options => options.IncludeScopes = true)
                    .AddFile(Path.Combine(appDataFolder, "Logs", $"voxel-log-{{Date}}{DateTime.Now:_HH-mm-ss}.log"), level);
            });

            return LoggerFactory.CreateLogger<Program>();
        }

        public static ILogger CreateLogger<T>()
        {
            return LoggerFactory.CreateLogger<T>();
        }
    }
}