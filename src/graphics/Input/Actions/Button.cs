// <copyright file="Button.cs" company="VoxelGame">
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

namespace VoxelGame.Graphics.Input.Actions;

/// <summary>
///     A button input action.
/// </summary>
public abstract class Button : InputAction
{
    /// <summary>
    ///     Create a new button.
    /// </summary>
    /// <param name="key">The trigger key.</param>
    /// <param name="input">The input manager.</param>
    protected Button(VirtualKeys key, Input input) : base(input)
    {
        Key = key;
    }

    /// <summary>
    ///     Get the used key or button.
    /// </summary>
    public VirtualKeys Key { get; private set; }

    /// <summary>
    ///     Get whether the button is pressed.
    /// </summary>
    public Boolean IsDown { get; private protected set; }

    /// <summary>
    ///     Get whether the button is up.
    /// </summary>
    public Boolean IsUp => !IsDown;

    /// <summary>
    ///     Set the binding to a different key or button.
    /// </summary>
    /// <param name="keyOrButton">The new key or button.</param>
    public void SetBinding(VirtualKeys keyOrButton)
    {
        Key = keyOrButton;
    }
}
