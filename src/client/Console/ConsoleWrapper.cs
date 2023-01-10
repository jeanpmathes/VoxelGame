// <copyright file="ConsoleWrapper.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Concurrent;
using VoxelGame.Core;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Console;

/// <summary>
///     A wrapper around the console provided by the UI.
/// </summary>
public class ConsoleWrapper
{
    private readonly ConsoleInterface consoleInterface;
    private readonly ConcurrentQueue<(string message, FollowUp[] followUp)> responses = new();

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
    public void WriteResponse(string response, params FollowUp[] followUp)
    {
        ApplicationInformation.Instance.EnsureMainThread(this);
        consoleInterface.WriteResponse(response, followUp);
    }

    /// <summary>
    ///     Queue a response to be written to the console.
    ///     This method can be called from any thread.
    /// </summary>
    /// <param name="response">The response to write.</param>
    /// <param name="followUp">A group of follow-up actions.</param>
    public void EnqueueResponse(string response, params FollowUp[] followUp)
    {
        responses.Enqueue((response, followUp));
    }

    /// <summary>
    ///     Flush all queued messages to the console.
    /// </summary>
    public void Flush()
    {
        ApplicationInformation.Instance.EnsureMainThread(this);

        while (responses.TryDequeue(out (string message, FollowUp[] followUp) response))
            WriteResponse(response.message, response.followUp);
    }

    /// <summary>
    ///     Write an error to the console.
    ///     This method must be called from the main thread.
    /// </summary>
    /// <param name="error">The error to write.</param>
    /// <param name="followUp">A group of follow-up actions.</param>
    public void WriteError(string error, params FollowUp[] followUp)
    {
        ApplicationInformation.Instance.EnsureMainThread(this);
        consoleInterface.WriteError(error, followUp);
    }

    /// <summary>
    ///     Clear the console content.
    ///     This method must be called from the main thread.
    /// </summary>
    public void Clear()
    {
        ApplicationInformation.Instance.EnsureMainThread(this);
        consoleInterface.Clear();
    }
}


