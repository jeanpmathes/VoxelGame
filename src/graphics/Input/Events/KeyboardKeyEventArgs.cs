// <copyright file="KeyboardKeyEventArgs.cs" company="VoxelGame">
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
using VoxelGame.Graphics.Definition;
using VoxelGame.GUI.Input;

namespace VoxelGame.Graphics.Input.Events;

/// <summary>
///     Keyboard key event arguments.
/// </summary>
public class KeyboardKeyEventArgs : EventArgs
{
    /// <summary>
    ///     The key.
    /// </summary>
    public VirtualKeys Key { get; init; }

    /// <summary>
    ///     Whether the key is pressed or released.
    /// </summary>
    public Boolean IsPressed { get; init; }

    /// <summary>
    ///     Whether the key is being held down.
    /// </summary>
    public Boolean IsRepeat { get; init; }

    /// <summary>
    ///     Whether the event was created by parts of the input system instead of being a physical event.
    /// </summary>
    public Boolean IsSynthetic { get; init; }

    /// <summary>
    ///     The active modifiers.
    /// </summary>
    public ModifierKeys Modifiers { get; init; }
}
