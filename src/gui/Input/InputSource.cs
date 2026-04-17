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
using System.Drawing;

namespace VoxelGame.GUI.Input;

/// <summary>
///     Abstract base class for input sources, for example, a platform-specific window.
/// </summary>
public class InputSource
{
    private readonly HashSet<IInputReceiver> receivers = [];

    /// <summary>
    ///     Add an input receiver to this input source, which will receive all input events.
    ///     If the receiver is already added, nothing happens.
    /// </summary>
    /// <param name="receiver">The receiver to add.</param>
    public void AddReceiver(IInputReceiver receiver)
    {
        receivers.Add(receiver);
    }

    /// <summary>
    ///     Send a key event, corresponding to a keyboard button.
    /// </summary>
    /// <param name="key">Which key the event corresponds to.</param>
    /// <param name="isDown">Whether the key is being pressed down (<c>true</c>) or up (<c>false</c>).</param>
    /// <param name="isRepeat">
    ///     Whether the event is a repeat event, i.e., the key is being held down, and this event is firing
    ///     repeatedly.
    /// </param>
    /// <param name="modifiers">The modifier keys which are currently active.</param>
    protected void SendKeyEvent(Key key, Boolean isDown, Boolean isRepeat, ModifierKeys modifiers)
    {
        foreach (IInputReceiver receiver in receivers)
            receiver.ReceiveKeyEvent(key, isDown, isRepeat, modifiers);
    }

    /// <summary>
    ///     Send a text input event, corresponding to a character being typed.
    /// </summary>
    /// <param name="text">The text that was input.</param>
    protected void SendTextEvent(String text)
    {
        foreach (IInputReceiver receiver in receivers)
            receiver.ReceiveTextEvent(text);
    }

    /// <summary>
    ///     Send a pointer button event, corresponding to a mouse button event.
    /// </summary>
    /// <param name="position">The position of the pointer event, in the coordinate space of the canvas.</param>
    /// <param name="button">The button that the event corresponds to.</param>
    /// <param name="isDown">Whether the button is being pressed down (<c>true</c>) or up (<c>false</c>).</param>
    /// <param name="modifiers">The modifier keys which are currently active.</param>
    protected void SendPointerButtonEvent(PointF position, PointerButton button, Boolean isDown, ModifierKeys modifiers)
    {
        foreach (IInputReceiver receiver in receivers)
            receiver.ReceivePointerButtonEvent(position, button, isDown, modifiers);
    }

    /// <summary>
    ///     Send a pointer move event, corresponding to the mouse moving across the screen.
    /// </summary>
    /// <param name="position">The position of the pointer event, in the coordinate space of the canvas.</param>
    /// <param name="deltaX">The change in the X coordinate since the last pointer move event.</param>
    /// <param name="deltaY">The change in the Y coordinate since the last pointer move event.</param>
    protected void SendPointerMoveEvent(PointF position, Single deltaX, Single deltaY)
    {
        foreach (IInputReceiver receiver in receivers)
            receiver.ReceivePointerMoveEvent(position, deltaX, deltaY);
    }

    /// <summary>
    ///     Send a scroll event, corresponding to the mouse wheel being scrolled.
    /// </summary>
    /// <param name="position">The position of the pointer event, in the coordinate space of the canvas.</param>
    /// <param name="deltaX">The amount of horizontal scroll.</param>
    /// <param name="deltaY">The amount of vertical scroll.</param>
    protected void SendScrollEvent(PointF position, Single deltaX, Single deltaY)
    {
        foreach (IInputReceiver receiver in receivers)
            receiver.ReceiveScrollEvent(position, deltaX, deltaY);
    }
}
