// <copyright file="Dialog.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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
