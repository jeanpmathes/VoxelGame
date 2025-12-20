// <copyright file="PushButton.cs" company="VoxelGame">
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
using VoxelGame.Graphics.Definition;

namespace VoxelGame.Graphics.Input.Actions;

/// <summary>
///     A button that can only be pressed again when it is released.
/// </summary>
public class PushButton : Button
{
    private Boolean hasReleased;
    private Boolean pushed;

    /// <summary>
    ///     Create a new push button.
    /// </summary>
    /// <param name="key">The key or button to target.</param>
    /// <param name="input">The input manager.</param>
    public PushButton(VirtualKeys key, Input input) : base(key, input) {}

    /// <summary>
    ///     Get whether the button is pushed this frame.
    /// </summary>
    public Boolean Pushed
    {
        get => pushed;
        private set
        {
            pushed = value;
            IsDown = value;
        }
    }

    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <inheritdoc />
    protected override void OnInputUpdated(Object? sender, EventArgs e)
    {
        KeyState state = Input.KeyState;

        Pushed = false;

        if (hasReleased && state.IsKeyDown(Key))
        {
            hasReleased = false;
            Pushed = true;
        }
        else if (state.IsKeyUp(Key))
        {
            hasReleased = true;
        }
    }
}
