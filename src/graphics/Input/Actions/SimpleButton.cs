// <copyright file="SimpleButton.cs" company="VoxelGame">
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
///     A simple button, that can be pushed down.
/// </summary>
public class SimpleButton : Button
{
    /// <summary>
    ///     Create a new simple button.
    /// </summary>
    /// <param name="key">The button target.</param>
    /// <param name="input">The input manager.</param>
    public SimpleButton(VirtualKeys key, Input input) : base(key, input) {}

    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <inheritdoc />
    protected override void OnInputUpdated(Object? sender, EventArgs e)
    {
        KeyState state = Input.KeyState;
        IsDown = state.IsKeyDown(Key);
    }
}
