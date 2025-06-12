// <copyright file="ConsoleWrapper.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using VoxelGame.Core;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Console;

/// <summary>
///     A wrapper around the console interface provided by the UI.
/// </summary>
public class ConsoleWrapper
{
    private readonly Channel<(String message, Boolean error, FollowUp[] followUp)> responses = Channel.CreateUnbounded<(String message, Boolean error, FollowUp[] followUp)>();

    private readonly ConsoleInterface consoleInterface;

    /// <summary>
    ///     Create a new console wrapper.
    /// </summary>
    /// <param name="consoleInterface">The interface to wrap.</param>
    public ConsoleWrapper(ConsoleInterface consoleInterface)
    {
        this.consoleInterface = consoleInterface;
    }

    /// <summary>
    ///     Write a response to the console.
    ///     This method must be called from the main thread.
    /// </summary>
    /// <param name="response">The response to write.</param>
    /// <param name="followUp">A group of follow-up actions.</param>
    public void WriteResponse(String response, FollowUp[]? followUp = null)
    {
        ApplicationInformation.ThrowIfNotOnMainThread(this);

        consoleInterface.WriteResponse(response, followUp ?? []);
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
        ApplicationInformation.ThrowIfNotOnMainThread(this);

        consoleInterface.WriteError(error, followUp ?? []);
    }

    /// <summary>
    ///     Queue an error to be written to the console.
    ///     This method can be called from any thread.
    /// </summary>
    /// <param name="error">The error to write.</param>
    /// <param name="followUp">A group of follow-up actions.</param>
    /// <param name="token">A token to cancel the operation.</param>
    public async Task WriteErrorAsync(String error, FollowUp[]? followUp = null, CancellationToken token = default)
    {
        await responses.Writer.WriteAsync((error, error: true, followUp ?? []), token);
    }

    /// <summary>
    ///     Flush all queued messages to the console.
    /// </summary>
    public void Flush()
    {
        ApplicationInformation.ThrowIfNotOnMainThread(this);

        while (responses.Reader.TryRead(out (String message, Boolean error, FollowUp[] followUp) response))
            if (response.error)
                consoleInterface.WriteError(response.message, response.followUp);
            else
                consoleInterface.WriteResponse(response.message, response.followUp);
    }

    /// <summary>
    ///     Clear the console content.
    ///     This method must be called from the main thread.
    /// </summary>
    public void Clear()
    {
        ApplicationInformation.ThrowIfNotOnMainThread(this);

        consoleInterface.Clear();
    }
}
