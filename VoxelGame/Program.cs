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

namespace VoxelGame
{
    internal class Program
    {
        public static string Version { get; private set; } = null!;

        public static ILoggerFactory LoggerFactory { get; private set; } = null!;

        private static void Main()
        {
            LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("VoxelGame", LogLevel.Debug)
                    .AddConsole(options => options.IncludeScopes = true)
                    .AddDebug()
                    .AddEventLog();
            });

            ILogger logger = LoggerFactory.CreateLogger<Program>();

            Version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "[VERSION UNAVAILABLE]";
            Console.Title = Language.VoxelGame + " " + Version;

            Console.WriteLine(Language.StartingGame);
            Console.WriteLine(Language.Version + " " + Version);

            GameWindowSettings gameWindowSettings = new GameWindowSettings
            {
                IsMultiThreaded = false,
                RenderFrequency = 60,
                UpdateFrequency = 60,
            };

            NativeWindowSettings nativeWindowSettings = NativeWindowSettings.Default;
            nativeWindowSettings.Title = Language.VoxelGame + " " + Version;
            nativeWindowSettings.Size = new Vector2i(800, 450);

            logger.LogInformation("Starting game on version: {Version}", Version);

            using (Game game = new Game(gameWindowSettings, nativeWindowSettings))
            {
                game.Run();
            }

            Console.WriteLine();
            Console.WriteLine(Language.PressAnyKeyToExit);
            Console.ReadKey(true);

            logger.LogInformation("Exiting game.");

            LoggerFactory.Dispose();
        }
    }
}