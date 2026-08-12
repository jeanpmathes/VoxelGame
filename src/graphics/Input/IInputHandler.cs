// <copyright file="IInputHandler.cs" company="VoxelGame">
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
using VoxelGame.Graphics.Input.Events;
using VoxelGame.GUI.Input;

namespace VoxelGame.Graphics.Input;

/// <summary>
///     Receives input messages in routing order. Returning <see langword="true" /> prevents later handlers from
///     receiving a routed message. Modifier-loss notifications are instead sent to every registered handler.
/// </summary>
public interface IInputHandler
{
    /// <summary>Receive a keyboard event. Return <see langword="true" /> to stop routing it.</summary>
    /// <param name="args">The event arguments.</param>
    /// <returns>Whether the event was handled.</returns>
    public Boolean HandleKeyboardKey(KeyboardKeyEventArgs args)
    {
        return false;
    }

    /// <summary>Receive text input. Return <see langword="true" /> to stop routing it.</summary>
    /// <param name="args">The event arguments.</param>
    /// <returns>Whether the event was handled.</returns>
    public Boolean HandleText(TextInputEventArgs args)
    {
        return false;
    }

    /// <summary>Receive a mouse-button event. Return <see langword="true" /> to stop routing it.</summary>
    /// <param name="args">The event arguments.</param>
    /// <returns>Whether the event was handled.</returns>
    public Boolean HandleMouseButton(MouseButtonEventArgs args)
    {
        return false;
    }

    /// <summary>Receive mouse movement. Return <see langword="true" /> to stop routing it.</summary>
    /// <param name="args">The event arguments.</param>
    /// <returns>Whether the event was handled.</returns>
    public Boolean HandleMouseMove(MouseMoveEventArgs args)
    {
        return false;
    }

    /// <summary>Receive mouse-wheel input. Return <see langword="true" /> to stop routing it.</summary>
    /// <param name="args">The event arguments.</param>
    /// <returns>Whether the event was handled.</returns>
    public Boolean HandleMouseWheel(MouseWheelEventArgs args)
    {
        return false;
    }

    /// <summary>
    ///     Receive notification that one or more logical modifiers are no longer held.
    ///     This is sent to every registered handler after the affected key releases have been routed.
    /// </summary>
    /// <param name="modifiers">The modifiers that are no longer held.</param>
    public void HandleModifiersLost(ModifierKeys modifiers) {}
}
