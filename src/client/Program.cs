// <copyright file="World.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using Microsoft.Extensions.Logging;
using OpenToolkit.Windowing.Desktop;
using System;
using System.IO;
using VoxelGame.Core;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;

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
            GameInformation.Initialize(Version);
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

            using (Application.Client client = new Application.Client(gameWindowSettings, nativeWindowSettings, appDataDirectory, screenshotDirectory))
            {
                client.Run();
            }

            logger.LogInformation("Exiting game.");
        }
    }
}