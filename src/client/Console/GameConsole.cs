// <copyright file="GameConsole.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using Microsoft.Extensions.Logging;
using VoxelGame.Client.Application;
using VoxelGame.Client.Console.Commands;
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
        if (game.Console == null) return;

        LogProcessingConsoleInput(logger, input);

        commandInvoker.InvokeCommand(
            input,
            new Context(game.Console, commandInvoker, game.Player));
    }

    /// <inheritdoc />
    public void OnWorldReady()
    {
        if (game.Console == null) return;

        LogTryingToExecuteWorldReadyScript(logger);

        Boolean executed = RunScript.Do(new Context(game.Console, commandInvoker, game.Player), WorldReadyScript, ignoreErrors: true);

        if (executed) LogExecutedWorldReadyScript(logger);
        else LogNoWorldReadyScriptFound(logger);
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<GameConsole>();

    [LoggerMessage(EventId = LogID.GameConsole + 0, Level = LogLevel.Debug, Message = "Processing console input: {Command}")]
    private static partial void LogProcessingConsoleInput(ILogger logger, String command);

    [LoggerMessage(EventId = LogID.GameConsole + 1, Level = LogLevel.Debug, Message = "Trying to execute world ready script")]
    private static partial void LogTryingToExecuteWorldReadyScript(ILogger logger);

    [LoggerMessage(EventId = LogID.GameConsole + 2, Level = LogLevel.Information, Message = "Executed world ready script")]
    private static partial void LogExecutedWorldReadyScript(ILogger logger);

    [LoggerMessage(EventId = LogID.GameConsole + 3, Level = LogLevel.Debug, Message = "No world ready script found")]
    private static partial void LogNoWorldReadyScriptFound(ILogger logger);

    #endregion LOGGING
}
