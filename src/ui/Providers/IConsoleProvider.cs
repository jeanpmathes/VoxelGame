// <copyright file="IConsoleProvider.cs" company="VoxelGame">
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
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.UI.Providers;

/// <summary>
///     An interface for a console backend that can process inputs from a console frontend.
/// </summary>
public interface IConsoleProvider
{
    /// <summary>
    ///     Process a console input.
    /// </summary>
    /// <param name="input">The user input to process.</param>
    void ProcessInput(String input);

    /// <summary>
    ///     Call this method on world-ready to run init commands.
    /// </summary>
    void OnWorldReady();

    /// <summary>
    ///     Invoked when a new message is added to the console.
    /// </summary>
    event EventHandler<MessageAddedEventArgs>? MessageAdded;

    /// <summary>
    ///     Invoked when the console is cleared.
    /// </summary>
    event EventHandler? Cleared;

    /// <summary>
    ///     Event arguments for the <see cref="MessageAdded" /> event.
    /// </summary>
    /// <param name="message">The text of the message.</param>
    /// <param name="followUp">The follow-up actions associated with the message.</param>
    /// <param name="isError">Whether the message is an error message.</param>
    class MessageAddedEventArgs(String message, FollowUp[] followUp, Boolean isError) : EventArgs
    {
        /// <summary>
        ///     The text of the message.
        /// </summary>
        public String Message { get; } = message;

        /// <summary>
        ///     The follow-up actions associated with the message.
        /// </summary>
        public FollowUp[] FollowUp { get; } = followUp;

        /// <summary>
        ///     Whether the message is an error message.
        /// </summary>
        public Boolean IsError { get; } = isError;
    }
}
