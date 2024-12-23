﻿// <copyright file="GameConsole.cs" company="VoxelGame">
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
public partial class GameConsole : IConsoleProvider
{
    /// <summary>
    ///     The name of the script to execute when the world is ready.
    /// </summary>
    public const String WorldReadyScript = "world_ready";

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

        LogProcessingConsoleInput(logger, input);

        commandInvoker.InvokeCommand(
            input,
            new Context(game.Console, commandInvoker, game.Player));
    }

    /// <inheritdoc />
    public void OnWorldReady()
    {
        if (game.Console == null)
            throw new InvalidOperationException();

        LogTryingToExecuteWorldReadyScript(logger);

        Boolean executed = RunScript.Do(new Context(game.Console, commandInvoker, game.Player), WorldReadyScript, ignoreErrors: true);

        if (executed) LogExecutedWorldReadyScript(logger);
        else LogNoWorldReadyScriptFound(logger);
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

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<GameConsole>();

    [LoggerMessage(EventId = Events.Console, Level = LogLevel.Debug, Message = "Processing console input: {Command}")]
    private static partial void LogProcessingConsoleInput(ILogger logger, String command);

    [LoggerMessage(EventId = Events.Console, Level = LogLevel.Debug, Message = "Trying to execute world ready script")]
    private static partial void LogTryingToExecuteWorldReadyScript(ILogger logger);

    [LoggerMessage(EventId = Events.Console, Level = LogLevel.Information, Message = "Executed world ready script")]
    private static partial void LogExecutedWorldReadyScript(ILogger logger);

    [LoggerMessage(EventId = Events.Console, Level = LogLevel.Debug, Message = "No world ready script found")]
    private static partial void LogNoWorldReadyScriptFound(ILogger logger);

    #endregion LOGGING
}
