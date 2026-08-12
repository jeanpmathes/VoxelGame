// <copyright file="Layer.cs" company="VoxelGame">
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
using VoxelGame.Graphics.Input;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Inputs;

/// <summary>
///     The <see cref="Layer" /> of a keybind defines when it is processed and updated, as well as its priority to receive
///     input.
/// </summary>
public enum Layer
{
    /// <summary>
    ///     Actions that remain available outside gameplay.
    ///     This is the highest priority, even above the user interface.
    ///     Actions of this layer should be used in the input update cycle.
    /// </summary>
    Application,

    /// <summary>
    ///     Actions used by gameplay.
    ///     This is the lowest priority.
    ///     Actions of this layer should be used in the logic update cycle.
    /// </summary>
    Game
}

/// <summary>
///     Utilities for working with <see cref="Layer" />.
/// </summary>
public static class Layers
{
    /// <param name="layer">The client input layer.</param>
    extension(Layer layer)
    {
        /// <summary>
        ///     Get the <see cref="VoxelGame.Graphics.Input.InputHandlerLayer" /> corresponding to this <see cref="Layer" />.
        /// </summary>
        /// <returns>The corresponding input-handler layer.</returns>
        internal InputHandlerLayer InputHandlerLayer => layer switch
        {
            Layer.Application => InputHandlerLayer.Application,
            Layer.Game => InputHandlerLayer.Game,
            _ => throw Exceptions.UnsupportedEnumValue(layer)
        };

        /// <summary>
        ///     Get whether <paramref name="combination" /> may be assigned to a keybind in this layer.
        /// </summary>
        /// <param name="combination">The combination to check.</param>
        /// <returns><see langword="true" /> if the combination may be assigned; otherwise, <see langword="false" />.</returns>
        internal Boolean Allows(KeyOrButtonCombination combination)
        {
            return layer switch
            {
                // If we were to allow the left mouse button here, one could not undo it:
                // Pressing the left mouse button would be handled on the application layer, 
                // so the UI layer would never receive it - it is a UI action to change a keybind.
                Layer.Application => combination.KeyOrButton != VirtualKeys.LeftButton,

                Layer.Game => true,

                _ => throw Exceptions.UnsupportedEnumValue(layer)
            };
        }
    }
}
