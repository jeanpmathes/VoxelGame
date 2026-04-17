// <copyright file="IInputReceiver.cs" company="VoxelGame">
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
using System.Drawing;

namespace VoxelGame.GUI.Input;

/// <summary>
///     An interface for classes that can receive inputs as methods.
/// </summary>
public interface IInputReceiver
{
    /// <summary>
    ///     Receive a key event, corresponding to a keyboard button.
    /// </summary>
    /// <param name="key">Which key the event corresponds to.</param>
    /// <param name="isDown">Whether the key is being pressed down (<c>true</c>) or up (<c>false</c>).</param>
    /// <param name="isRepeat">
    ///     Whether the event is a repeat event, i.e., the key is being held down, and this event is firing
    ///     repeatedly.
    /// </param>
    /// <param name="modifiers">The modifier keys which are currently active.</param>
    public void ReceiveKeyEvent(Key key, Boolean isDown, Boolean isRepeat, ModifierKeys modifiers);

    /// <summary>
    ///     Receive a text input event, corresponding to a character being typed.
    /// </summary>
    /// <param name="text">The text that was input.</param>
    public void ReceiveTextEvent(String text);

    /// <summary>
    ///     Receive a pointer button event, corresponding to a mouse button event.
    /// </summary>
    /// <param name="position">The position of the pointer event, in the coordinate space of the canvas.</param>
    /// <param name="button">The button that the event corresponds to.</param>
    /// <param name="isDown">Whether the button is being pressed down (<c>true</c>) or up (<c>false</c>).</param>
    /// <param name="modifiers">The modifier keys which are currently active.</param>
    public void ReceivePointerButtonEvent(PointF position, PointerButton button, Boolean isDown, ModifierKeys modifiers);

    /// <summary>
    ///     Receive a pointer move event, corresponding to the mouse moving across the screen.
    /// </summary>
    /// <param name="position">The position of the pointer event, in the coordinate space of the canvas.</param>
    /// <param name="deltaX">The change in the X coordinate since the last pointer move event.</param>
    /// <param name="deltaY">The change in the Y coordinate since the last pointer move event.</param>
    public void ReceivePointerMoveEvent(PointF position, Single deltaX, Single deltaY);

    /// <summary>
    ///     Receive a scroll event, corresponding to the mouse wheel being scrolled.
    /// </summary>
    /// <param name="position">The position of the pointer event, in the coordinate space of the canvas.</param>
    /// <param name="deltaX">The amount of horizontal scroll.</param>
    /// <param name="deltaY">The amount of vertical scroll.</param>
    public void ReceiveScrollEvent(PointF position, Single deltaX, Single deltaY);
}
