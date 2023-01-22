// <copyright file="GameConsole.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Logging;
using VoxelGame.Client.Console.Commands;
using VoxelGame.Client.Entities;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;
using VoxelGame.UI.Providers;

namespace VoxelGame.Client.Console;

/// <summary>
///     The backend of the game console.
/// </summary>
public class GameConsole : IConsoleProvider
{
    /// <summary>
    ///     The name of the script to execute when the world is ready.
    /// </summary>
    public const string WorldReadyScript = "world_ready";

    private static readonly ILogger logger = LoggingHelper.CreateLogger<GameConsole>();

    private readonly CommandInvoker commandInvoker;

    /// <summary>
    ///     Create a new game console.
    /// </summary>
    /// <param name="commandInvoker">The invoker that will invoke all commands for this game console.</param>
    public GameConsole(CommandInvoker commandInvoker)
    {
        this.commandInvoker = commandInvoker;
    }

    private static ConsoleWrapper Console => Application.Client.Instance.CurrentGame!.Console;

    /// <inheritdoc />
    public void ProcessInput(string input)
    {
        Debug.Assert(Application.Client.Instance.CurrentGame != null);

        logger.LogDebug(Events.Console, "Console command: {Command}", input);

        commandInvoker.InvokeCommand(
            input,
            new Context(Console, commandInvoker, Application.Client.Instance.CurrentGame.Player));
    }

    /// <inheritdoc />
    public void OnWorldReady()
    {
        Debug.Assert(Application.Client.Instance.CurrentGame != null);
        ClientPlayer player = Application.Client.Instance.CurrentGame.Player;

        logger.LogDebug("Trying to execute world ready script");

        bool executed = RunScript.Do(new Context(Console, commandInvoker, player), WorldReadyScript, ignoreErrors: true);

        if (executed) logger.LogInformation(Events.Console, "Executed world ready script");
        else logger.LogDebug("No world ready script found");
    }

    /// <summary>
    ///     Build a default invoker for the game.
    /// </summary>
    /// <returns>A invoker, filled with all default commands and parsers.</returns>
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
                s => uint.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out _),
                s => uint.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture)));

        invoker.AddParser(
            Parser.BuildParser(
                s => double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out _),
                s => double.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture)));

        invoker.AddParser(Parser.BuildParser(
            s => Enum.IsDefined(typeof(Orientation), s),
            Enum.Parse<Orientation>));

        invoker.AddParser(Parser.BuildParser(s => bool.TryParse(s, out _), bool.Parse));

        invoker.SearchCommands();
        invoker.AddCommand(new Help(invoker));

        return invoker;
    }
}

