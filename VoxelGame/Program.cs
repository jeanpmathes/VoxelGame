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
        private static void Main()
        {
            Console.Title = Language.VoxelGame + " Console";
            Console.WriteLine(Language.StartingGame);

            GameWindowSettings gameWindowSettings = new GameWindowSettings
            {
                IsMultiThreaded = false,
                RenderFrequency = 60,
                UpdateFrequency = 60,
            };

            NativeWindowSettings nativeWindowSettings = NativeWindowSettings.Default;
            nativeWindowSettings.Title = Language.VoxelGame;
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