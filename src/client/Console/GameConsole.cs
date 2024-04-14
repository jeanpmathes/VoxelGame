// <copyright file="GameConsole.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Globalization;
using Microsoft.Extensions.Logging;
using VoxelGame.Client.Application;
using VoxelGame.Client.Console.Commands;
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
    public const String WorldReadyScript = "world_ready";

    private static readonly ILogger logger = LoggingHelper.CreateLogger<GameConsole>();

    private readonly Game game;
    private readonly CommandInvoker commandInvoker;

    /// <summary>
    ///     Create a new game console.
    /// </summary>
    /// <param name="game">The game that this console is for.</param>
    /// <param name="commandInvoker">The invoker that will invoke all commands for this game console.</param>
    public GameConsole(Game game, CommandInvoker commandInvoker)
    {
        this.game = game;
        this.commandInvoker = commandInvoker;
    }

    /// <inheritdoc />
    public void ProcessInput(String input)
    {
        if (game.Console == null)
            throw new InvalidOperationException();

        logger.LogDebug(Events.Console, "Processing console input: {Command}", input);

        commandInvoker.InvokeCommand(
            input,
            new Context(game.Console, commandInvoker, game.Player));
    }

    /// <inheritdoc />
    public void OnWorldReady()
    {
        if (game.Console == null)
            throw new InvalidOperationException();

        logger.LogDebug("Trying to execute world ready script");

        Boolean executed = RunScript.Do(new Context(game.Console, commandInvoker, game.Player), WorldReadyScript, ignoreErrors: true);

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
                s => Int32.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out _),
                s => Int32.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture)));

        invoker.AddParser(
            Parser.BuildParser(
                s => UInt32.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out _),
                s => UInt32.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture)));

        invoker.AddParser(
            Parser.BuildParser(
                s => Double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out _),
                s => Double.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture)));

        invoker.AddParser(Parser.BuildParser(
            s => Enum.IsDefined(typeof(Orientation), s),
            Enum.Parse<Orientation>));

        invoker.AddParser(Parser.BuildParser(s => Boolean.TryParse(s, out _), Boolean.Parse));

        invoker.SearchCommands();
        invoker.AddCommand(new Help(invoker));

        return invoker;
    }
}
