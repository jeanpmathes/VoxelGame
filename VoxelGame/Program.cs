// <copyright file="Program.cs" company="VoxelGame">
//     All rights reserved.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Desktop;
using VoxelGame.Resources.Language;
using System;

namespace VoxelGame
{
    internal static class Program
    {
        public static string Version { get; private set; } = null!;

        private static void Main()
        {
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

            using (Game game = new Game(gameWindowSettings, nativeWindowSettings))
            {
                game.Run();
            }

            Console.WriteLine(Language.ExitingGame);

            Console.ReadKey(true);
        }
    }
}