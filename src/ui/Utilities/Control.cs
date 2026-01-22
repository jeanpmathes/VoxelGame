// <copyright file="Control.cs" company="VoxelGame">
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

using Gwen.Net.Control;

namespace VoxelGame.UI.Utilities;

/// <summary>
///     Utility methods for controls.
/// </summary>
public static class Control
{
    /// <summary>
    ///     Indicate a control as used. This is purely for linting purposes, as controls are always used by their parent.
    /// </summary>
    /// <param name="control">The control to use.</param>
    public static void Used(ControlBase control)
    {
        // Intentionally left empty.
    }
}
