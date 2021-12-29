// <copyright file="IConsoleProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

namespace VoxelGame.UI.Providers
{
    /// <summary>
    ///     An interface for a console backend that can process inputs into a console frontend.
    /// </summary>
    public interface IConsoleProvider
    {
        /// <summary>
        ///     Process a console input.
        /// </summary>
        /// <param name="input">The user input to process.</param>
        void ProcessInput(string input);
    }
}