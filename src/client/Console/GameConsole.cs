// <copyright file="GameConsole.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using Microsoft.Extensions.Logging;
using VoxelGame.Logging;
using VoxelGame.UI.Providers;

namespace VoxelGame.Client.Console
{
    public class GameConsole : IConsoleProvider
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<GameConsole>();

        private static ConsoleWrapper Console => Application.Client.Instance.Console;

        public void ProcessInput(string input)
        {
            logger.LogDebug("Console command: {Command}", input);

            Console.WriteResponse("Test Response!");
            Console.WriteError("Test Error!");
        }
    }
}
