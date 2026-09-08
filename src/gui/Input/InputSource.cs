// <copyright file="InputSource.cs" company="VoxelGame">
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
using System.Collections.Generic;
using VoxelGame.GUI.Utilities;

namespace VoxelGame.GUI.Input;

/// <summary>
///     Abstract base class for input sources, for example, a platform-specific window.
/// </summary>
public class InputSource
{
    private readonly List<IInputReceiver> receivers = [];

    /// <summary>
    ///     Add an input receiver to this input source, which will receive all input events.
    ///     If the receiver is already added, nothing happens.
    /// </summary>
    /// <param name="receiver">The receiver to add.</param>
    public void AddReceiver(IInputReceiver receiver)
    {
        if (!receivers.Contains(receiver))
            receivers.Add(receiver);
    }

    /// <summary>
    ///     Send a key event, corresponding to a keyboard button.
    /// </summary>
    /// <param name="key">Which key the event corresponds to.</param>
    /// <param name="isDown">Whether the key is being pressed down (<see langword="true"/>) or up (<see langword="false"/>).</param>
    /// <param name="isRepeat">
    ///     Whether the event is a repeat event, i.e., the key is being held down, and this event is firing
    ///     repeatedly.
    /// </param>
    /// <param name="modifiers">The modifier keys that are currently active.</param>
    /// <param name="isSynthetic">Whether the event was created by parts of the input system instead of being a physical event.</param>
    /// <returns>Whether a receiver handled the event.</returns>
    protected Boolean SendKeyEvent(Key key, Boolean isDown, Boolean isRepeat, ModifierKeys modifiers, Boolean isSynthetic = false)
    {
        foreach (IInputReceiver receiver in receivers)
            if (receiver.ReceiveKeyEvent(key, isDown, isRepeat, modifiers, isSynthetic))
                return true;

        return false;
    }

    /// <summary>
    ///     Send a text input event, corresponding to a character being typed.
    /// </summary>
    /// <param name="text">The text that was input.</param>
    /// <returns>Whether a receiver handled the event.</returns>
    protected Boolean SendTextEvent(String text)
    {
        foreach (IInputReceiver receiver in receivers)
            if (receiver.ReceiveTextEvent(text))
                return true;

        return false;
    }

    /// <summary>
    ///     Send a pointer button event, corresponding to a mouse button event.
    /// </summary>
    /// <param name="position">The position of the pointer event, in the coordinate space of the canvas.</param>
    /// <param name="button">The button that the event corresponds to.</param>
    /// <param name="isDown">Whether the button is being pressed down (<see langword="true"/>) or up (<see langword="false"/>).</param>
    /// <param name="modifiers">The modifier keys which are currently active.</param>
    /// <param name="isSynthetic">Whether the event was created by parts of the input system instead of being a physical event.</param>
    /// <returns>Whether a receiver handled the event.</returns>
    protected Boolean SendPointerButtonEvent(Point position, PointerButton button, Boolean isDown, ModifierKeys modifiers, Boolean isSynthetic = false)
    {
        foreach (IInputReceiver receiver in receivers)
            if (receiver.ReceivePointerButtonEvent(position, button, isDown, modifiers, isSynthetic))
                return true;

        return false;
    }

    /// <summary>
    ///     Send a pointer move event, corresponding to the mouse moving across the screen.
    /// </summary>
    /// <param name="position">The position of the pointer event, in the coordinate space of the canvas.</param>
    /// <param name="deltaX">The change in the X coordinate since the last pointer move event.</param>
    /// <param name="deltaY">The change in the Y coordinate since the last pointer move event.</param>
    /// <returns>Whether a receiver handled the event.</returns>
    protected Boolean SendPointerMoveEvent(Point position, Single deltaX, Single deltaY)
    {
        foreach (IInputReceiver receiver in receivers)
            if (receiver.ReceivePointerMoveEvent(position, deltaX, deltaY))
                return true;

        return false;
    }

    /// <summary>
    ///     Send a scroll event, corresponding to the mouse wheel being scrolled.
    /// </summary>
    /// <param name="position">The position of the pointer event, in the coordinate space of the canvas.</param>
    /// <param name="deltaX">The amount of horizontal scroll.</param>
    /// <param name="deltaY">The amount of vertical scroll.</param>
    /// <returns>Whether a receiver handled the event.</returns>
    protected Boolean SendScrollEvent(Point position, Single deltaX, Single deltaY)
    {
        foreach (IInputReceiver receiver in receivers)
            if (receiver.ReceiveScrollEvent(position, deltaX, deltaY))
                return true;

        return false;
    }
}
