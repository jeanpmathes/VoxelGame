// <copyright file="Dialog.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Graphics;

/// <summary>
///     Utility class for using dialogs.
/// </summary>
public static class Dialog
{
    /// <summary>
    ///     Show a message box with the given message.
    /// </summary>
    /// <param name="message">The message to display.</param>
    public static void ShowError(String message)
    {
        NativeMethods.ShowErrorBox(message, "Error");
    }
}
