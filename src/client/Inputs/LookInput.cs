// <copyright file="LookInput.cs" company="VoxelGame">
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
using OpenTK.Mathematics;
using VoxelGame.Graphics.Input.Devices;

namespace VoxelGame.Client.Inputs;

/// <summary>
///     Wraps input sources to provide action data for look movement.
/// </summary>
public class LookInput
{
    private readonly Mouse mouse;

    private Single sensitivity;

    /// <summary>
    ///     Create a new look input wrapper.
    /// </summary>
    /// <param name="mouse">The mouse providing the movement.</param>
    /// <param name="sensitivity">The sensitivity to apply to the mouse movement.</param>
    public LookInput(Mouse mouse, Single sensitivity)
    {
        this.mouse = mouse;
        this.sensitivity = sensitivity;
    }

    /// <summary>
    ///     Get the input value.
    /// </summary>
    public Vector2d Value => mouse.Delta * sensitivity;

    /// <summary>
    ///     Set the sensitivity of the look input.
    /// </summary>
    /// <param name="newSensitivity">The new sensitivity.</param>
    public void SetSensitivity(Single newSensitivity)
    {
        sensitivity = newSensitivity;
    }
}
