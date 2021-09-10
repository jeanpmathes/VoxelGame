// <copyright file="World.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.IO;
using Microsoft.Extensions.Logging;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Desktop;
using Properties;
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
            string appDataDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "voxel");

            string screenshotDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                "VoxelGame");

            Directory.CreateDirectory(appDataDirectory);
            Directory.CreateDirectory(screenshotDirectory);

#if DEBUG
            const bool logDebug = true;
#else
            bool logDebug = args.Length > 0 && args[0] == "-logDebug";
#endif

            ILogger logger = LoggingHelper.SetupLogging(nameof(Program), logDebug, appDataDirectory);

#if !DEBUG
            if (logDebug)
            {
                logger.LogInformation("Logging debug messages");
            }
            else
            {
                logger.LogInformation("Debug messages will not be logged. Use '-logDebug' to log debug messages");
            }
#endif

            Version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "[VERSION UNAVAILABLE]";
            GameInformation.Initialize(Version);
            Console.Title = Language.VoxelGame + @" " + Version;

            Console.WriteLine(Language.StartingGame);
            Console.WriteLine(Language.Version + @" " + Version);

            GameWindowSettings gameWindowSettings = new()
            {
                IsMultiThreaded = false,
                RenderFrequency = client.Default.MaxFPS,
                UpdateFrequency = 60.0
            };

            NativeWindowSettings nativeWindowSettings = NativeWindowSettings.Default;
            nativeWindowSettings.WindowBorder = WindowBorder.Hidden;
            nativeWindowSettings.Profile = ContextProfile.Compatability;
            nativeWindowSettings.Title = Language.VoxelGame + " " + Version;
            nativeWindowSettings.Size = client.Default.ScreenSize.ToVector2i();
            nativeWindowSettings.StartFocused = false;

            logger.LogInformation("Starting game on version: {Version}", Version);

            using (Application.Client client = new(
                gameWindowSettings,
                nativeWindowSettings,
                appDataDirectory,
                screenshotDirectory))
            {
                client.Run();
            }

            logger.LogInformation("Exiting");
        }
    }
}
