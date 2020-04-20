// <copyright file="Program.cs" company="VoxelGame">
//     All rights reserved.
// </copyright>
// <author>pershingthesecond</author>
using System;
using Resources;
using System.IO;

namespace VoxelGame
{
    internal static class Program
    {
        private static void Main()
        {
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\Worlds");

            Console.WriteLine(Language.StartingGame);

            using (Game game = new Game(800, 450, Language.VoxelGame))
            {
                game.Run(60.0);
            }

            Console.WriteLine(Language.ExitingGame);
        }
    }
}