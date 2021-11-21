// <copyright file="GameConsole.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Globalization;
using Microsoft.Extensions.Logging;
using VoxelGame.Client.Console.Commands;
using VoxelGame.Logging;
using VoxelGame.UI.Providers;

namespace VoxelGame.Client.Console
{
    public class GameConsole : IConsoleProvider
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<GameConsole>();

        private readonly CommandInvoker commandInvoker;

        public GameConsole(CommandInvoker commandInvoker)
        {
            this.commandInvoker = commandInvoker;
        }

        private static ConsoleWrapper Console => Application.Client.Instance.Console;

        public void ProcessInput(string input)
        {
            logger.LogDebug(Events.Console, "Console command: {Command}", input);
            commandInvoker.InvokeCommand(input, new CommandContext(Console, Application.Client.Player));
        }

        public static CommandInvoker BuildInvoker()
        {
            CommandInvoker invoker = new();

            invoker.AddParser(Parser.BuildParser(_ => true, s => s));

            invoker.AddParser(
                Parser.BuildParser(
                    s => int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out _),
                    s => int.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture)));

            invoker.AddParser(
                Parser.BuildParser(
                    s => float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out _),
                    s => float.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture)));

            invoker.AddParser(Parser.BuildParser(s => bool.TryParse(s, out _), bool.Parse));

            invoker.SearchCommands();
            invoker.AddCommand(new Help(invoker));

            return invoker;
        }
    }
}
