// <copyright file="IInputControl.cs" company="VoxelGame">
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
using VoxelGame.Client.Inputs;

namespace VoxelGame.Client.Scenes;

/// <summary>
///     Controls retrieving and handling input.
/// </summary>
public interface IInputControl
{
    /// <summary>
    ///     Whether it is OK to handle game input currently.
    /// </summary>
    Boolean CanHandleGameInput { get; }

    /// <summary>
    ///     Whether it is OK to handle meta input currently.
    /// </summary>
    Boolean CanHandleMetaInput { get; }

    /// <summary>
    ///     Get the keybinds to use for input handling.
    /// </summary>
    KeybindManager Keybinds { get; }
}
