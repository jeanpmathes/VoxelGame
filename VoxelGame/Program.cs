// <copyright file="Program.cs" company="VoxelGame">
//     All rights reserved.
// </copyright>
// <author>pershingthesecond</author>
using Resources;
using System;

namespace VoxelGame
{
    internal static class Program
    {
        private static void Main()
        {
            Console.Title = Language.VoxelGame + " Console";
            Console.WriteLine(Language.StartingGame);

            using (Game game = new Game(800, 450, Language.VoxelGame))
            {
                game.Run(60.0);
            }

            Console.WriteLine(Language.ExitingGame);

            Console.ReadKey(true);
        }
    }
}