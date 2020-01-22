using System;

using VoxelGame.Rendering;

namespace VoxelGame
{
    class Program
    {
        static void Main(string[] args)
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
