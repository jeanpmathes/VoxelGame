// <copyright file="ConsoleWrapper.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Console
{
    /// <summary>
    ///     A wrapper around the console provided by the UI.
    /// </summary>
    public class ConsoleWrapper
    {
        private ConsoleInterface? consoleInterface;

        /// <summary>
        ///     Set the console interface.
        /// </summary>
        /// <param name="activeInterface">The console interface provided by the UI.</param>
        public void SetInterface(ConsoleInterface activeInterface)
        {
            consoleInterface = activeInterface;
        }

        /// <summary>
        ///     Clear the console interface.
        /// </summary>
        public void ClearInterface()
        {
            consoleInterface = null;
        }

        /// <summary>
        ///     Write a response to the console.
        /// </summary>
        /// <param name="response">The response to write.</param>
        public void WriteResponse(string response)
        {
            consoleInterface?.WriteResponse(response);
        }

        /// <summary>
        ///     Write an error to the console.
        /// </summary>
        /// <param name="error">The error to write.</param>
        public void WriteError(string error)
        {
            consoleInterface?.WriteError(error);
        }

        /// <summary>
        ///     Clear the console content.
        /// </summary>
        public void Clear()
        {
            consoleInterface?.Clear();
        }
    }
}