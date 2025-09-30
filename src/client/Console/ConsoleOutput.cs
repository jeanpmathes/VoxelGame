// <copyright file="ConsoleOutput.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Console;

/// <summary>
///     Receives output from the console commands and scripts and routes it back to the console implementation.
/// </summary>
public class ConsoleOutput
{
    private readonly SessionConsole console;
    private readonly Channel<(String message, Boolean error, FollowUp[] followUp)> responses = Channel.CreateUnbounded<(String message, Boolean error, FollowUp[] followUp)>();

    /// <summary>
    ///     Create a new console output.
    /// </summary>
    /// <param name="console">The console to route the output to.</param>
    public ConsoleOutput(SessionConsole console)
    {
        this.console = console;
    }

    /// <summary>
    ///     Write a response to the console.
    ///     This method must be called from the main thread.
    /// </summary>
    /// <param name="response">The response to write.</param>
    /// <param name="followUp">A group of follow-up actions.</param>
    public void WriteResponse(String response, FollowUp[]? followUp = null)
    {
        Core.App.Application.ThrowIfNotOnMainThread(this);

        console.AddMessage(response, followUp ?? [], isError: false);
    }

    /// <summary>
    ///     Queue a response to be written to the console.
    ///     This method can be called from any thread.
    /// </summary>
    /// <param name="response">The response to write.</param>
    /// <param name="followUp">A group of follow-up actions.</param>
    /// <param name="token">A token to cancel the operation.</param>
    public async ValueTask WriteResponseAsync(String response, FollowUp[]? followUp = null, CancellationToken token = default)
    {
        await responses.Writer.WriteAsync((response, error: false, followUp ?? []), token);
    }

    /// <summary>
    ///     Write an error to the console.
    ///     This method must be called from the main thread.
    /// </summary>
    /// <param name="error">The error to write.</param>
    /// <param name="followUp">A group of follow-up actions.</param>
    public void WriteError(String error, FollowUp[]? followUp = null)
    {
        Core.App.Application.ThrowIfNotOnMainThread(this);

        console.AddMessage(error, followUp ?? [], isError: true);
    }

    /// <summary>
    ///     Queue an error to be written to the console.
    ///     This method can be called from any thread.
    /// </summary>
    /// <param name="error">The error to write.</param>
    /// <param name="followUp">A group of follow-up actions.</param>
    /// <param name="token">A token to cancel the operation.</param>
    public async ValueTask WriteErrorAsync(String error, FollowUp[]? followUp = null, CancellationToken token = default)
    {
        await responses.Writer.WriteAsync((error, error: true, followUp ?? []), token);
    }

    /// <summary>
    ///     Flush all queued messages to the console.
    /// </summary>
    public void Flush()
    {
        Core.App.Application.ThrowIfNotOnMainThread(this);

        while (responses.Reader.TryRead(out (String message, Boolean error, FollowUp[] followUp) response))
            console.AddMessage(response.message, response.followUp, response.error);
    }

    /// <summary>
    ///     Clear the console content.
    ///     This method must be called from the main thread.
    /// </summary>
    public void Clear()
    {
        Core.App.Application.ThrowIfNotOnMainThread(this);

        Flush();

        console.Clear();
    }
}
