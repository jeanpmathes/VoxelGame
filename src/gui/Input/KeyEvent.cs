// <copyright file="KeyEvent.cs" company="VoxelGame">
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
using VoxelGame.GUI.Visuals;

namespace VoxelGame.GUI.Input;

/// <summary>
///     A (special) key input event.
/// </summary>
public sealed class KeyEvent : InputEvent
{
    /// <summary>
    ///     Creates a new <seealso cref="KeyEvent" />.
    /// </summary>
    public KeyEvent(Visual source, Key key, Boolean isDown, Boolean isRepeat, ModifierKeys modifiers) : base(source)
    {
        Key = key;
        IsDown = isDown;
        IsRepeat = isRepeat;
        Modifiers = modifiers;
    }

    /// <summary>
    ///     The key that is the source of this event.
    /// </summary>
    public Key Key { get; }

    /// <summary>
    ///     Whether the key is being pressed (true) or released (false).
    /// </summary>
    public Boolean IsDown { get; }

    /// <summary>
    ///     Whether this event is a repeat event (i.e. the key is being held down and this event is being fired repeatedly).
    ///     This is only true for key down events, and is always false for key up events.
    /// </summary>
    public Boolean IsRepeat { get; }

    /// <summary>
    ///     Any modifier keys that are currently pressed (e.g. Shift, Control, Alt) when this event is fired.
    /// </summary>
    public ModifierKeys Modifiers { get; }
}
