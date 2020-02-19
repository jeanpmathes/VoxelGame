// <copyright file="Program.cs" company="VoxelGame">
//     All rights reserved.
// </copyright>
// <author>pershingthesecond</author>
using System;

namespace VoxelGame
{
    internal static class Program
    {
        private static void Main()
        {
            Console.WriteLine("Starting game...");

            using (Game game = new Game(800, 450, "VoxelGame"))
            {
                game.Run(60.0);
            }

            Console.WriteLine("Exiting...");
        }
    }
}