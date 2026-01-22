// <copyright file="SessionConsole.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using Microsoft.Extensions.Logging;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Client.Console.Commands;
using VoxelGame.Client.Sessions;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;
using VoxelGame.UI.Providers;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Console;

/// <summary>
///     The backend of the in-game console.
/// </summary>
public partial class SessionConsole : SessionComponent, IConsoleProvider
{
    /// <summary>
    ///     The name of the script to execute when the world is ready.
    /// </summary>
    public const String WorldReadyScript = "world_ready";

    private readonly CommandInvoker commandInvoker;
    private readonly ConsoleOutput output;

    private readonly Session session;

    private Boolean executedWorldReadyScript;

    [Constructible]
    private SessionConsole(Session session, CommandInvoker commandInvoker) : base(session)
    {
        this.session = session;
        this.commandInvoker = commandInvoker;

        output = new ConsoleOutput(this);
    }

    /// <inheritdoc />
    public void ProcessInput(String input)
    {
        LogProcessingConsoleInput(logger, input);

        commandInvoker.InvokeCommand(
            input,
            new Context(output, commandInvoker, session.Player));
    }

    /// <inheritdoc />
    public void OnWorldReady()
    {
        if (executedWorldReadyScript) return;

        executedWorldReadyScript = true;

        LogTryingToExecuteWorldReadyScript(logger);

        Boolean executed = RunScript.Do(new Context(output, commandInvoker, session.Player), WorldReadyScript, ignoreErrors: true);

        if (executed) LogExecutedWorldReadyScript(logger);
        else LogNoWorldReadyScriptFound(logger);
    }

    /// <inheritdoc />
    public event EventHandler<IConsoleProvider.MessageAddedEventArgs>? MessageAdded;

    /// <inheritdoc />
    public event EventHandler? Cleared;

    /// <inheritdoc />
    public override void OnLogicUpdate(Delta delta, Timer? timer)
    {
        output.Flush();
    }

    /// <summary>
    ///     Add a message to the console.
    /// </summary>
    /// <param name="message">The text of the message to add.</param>
    /// <param name="followUp">The follow-up actions associated with the message.</param>
    /// <param name="isError">Whether the message is an error message, or a response.</param>
    public void AddMessage(String message, FollowUp[] followUp, Boolean isError)
    {
        MessageAdded?.Invoke(this, new IConsoleProvider.MessageAddedEventArgs(message, followUp, isError));
    }

    /// <summary>
    ///     Clear the console output.
    /// </summary>
    public void Clear()
    {
        Cleared?.Invoke(this, EventArgs.Empty);
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<SessionConsole>();

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
