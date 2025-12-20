// <copyright file="InputAction.cs" company="VoxelGame">
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

namespace VoxelGame.Graphics.Input.Actions;

/// <summary>
///     The base input action.
/// </summary>
public abstract class InputAction
{
    /// <summary>
    ///     Create a new input action.
    /// </summary>
    /// <param name="input">The input manager providing the input.</param>
    protected InputAction(Input input)
    {
        Input = input;

        input.InputUpdated += OnInputUpdated;
    }

    /// <summary>
    ///     Get the input manager providing the input.
    /// </summary>
    protected Input Input { get; }

    /// <summary>
    ///     Called every frame.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected abstract void OnInputUpdated(Object? sender, EventArgs e);
}
