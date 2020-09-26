// <copyright file="World.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Windowing.Desktop;
using System;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Resources.Language;
using System.IO;
using System.Threading;
using VoxelGame.Core.Utilities;
using VoxelGame.Core;

namespace VoxelGame.Client
{
    internal static class Program
    {
        public static string Version { get; private set; } = null!;

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

            ILogger logger = LoggingHelper.SetupLogging(nameof(Program), logDebug, appDataDirectory);

#if !DEBUG
            logger.LogInformation(logDebug ? "Debug will be logged." : "Debug will not be logged. Use '-logDebug' to log debug messages.");
#endif

            Version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "[VERSION UNAVAILABLE]";
            Game.SetVersion(Version);
            Console.Title = Language.VoxelGame + " " + Version;

            Console.WriteLine(Language.StartingGame);
            Console.WriteLine(Language.Version + " " + Version);

            GameWindowSettings gameWindowSettings = new GameWindowSettings
            {
                IsMultiThreaded = false,
                RenderFrequency = Properties.client.Default.MaxFPS,
                UpdateFrequency = 60.0
            };

            NativeWindowSettings nativeWindowSettings = NativeWindowSettings.Default;
            nativeWindowSettings.WindowBorder = OpenToolkit.Windowing.Common.WindowBorder.Hidden;
            nativeWindowSettings.Profile = OpenToolkit.Windowing.Common.ContextProfile.Compatability;
            nativeWindowSettings.Title = Language.VoxelGame + " " + Version;
            nativeWindowSettings.Size = Properties.client.Default.ScreenSize.ToVector2i();
            nativeWindowSettings.StartFocused = false;

            logger.LogInformation("Starting game on version: {Version}", Version);

            using (Client client = new Client(gameWindowSettings, nativeWindowSettings, appDataDirectory, screenshotDirectory))
            {
                client.Run();
            }

            Thread.Sleep(100);

            Console.WriteLine();
            Console.WriteLine(Language.PressAnyKeyToExit);
            Console.ReadKey(true);

            logger.LogInformation("Exiting game.");
        }
    }
}