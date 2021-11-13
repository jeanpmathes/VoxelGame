// <copyright file="GameConsole.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.UI.Providers;

namespace VoxelGame.Client.Console
{
    public class GameConsole : IConsoleProvider
    {
        private int counter;

        public (string response, bool isError) ProcessInput(string input)
        {
            return ("Test", counter++ % 2 == 0);
        }
    }
}