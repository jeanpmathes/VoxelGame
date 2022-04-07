// <copyright file="ConsoleWrapper.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Console;

/// <summary>
///     A wrapper around the console provided by the UI.
/// </summary>
public class ConsoleWrapper
{
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
    /// </summary>
    /// <param name="response">The response to write.</param>
    public void WriteResponse(string response)
    {
        consoleInterface.WriteResponse(response);
    }

    /// <summary>
    ///     Write an error to the console.
    /// </summary>
    /// <param name="error">The error to write.</param>
    public void WriteError(string error)
    {
        consoleInterface.WriteError(error);
    }

    /// <summary>
    ///     Clear the console content.
    /// </summary>
    public void Clear()
    {
        consoleInterface.Clear();
    }
}
