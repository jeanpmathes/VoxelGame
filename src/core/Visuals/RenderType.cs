// <copyright file="RenderType.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Visuals;

/// <summary>
///     The render type of a fluid.
/// </summary>
public enum RenderType
{
    /// <summary>
    ///     The fluid is not rendered.
    /// </summary>
    NotRendered,

    /// <summary>
    ///     The fluid is opaque.
    /// </summary>
    Opaque,

    /// <summary>
    ///     The fluid is transparent.
    /// </summary>
    Transparent
}
