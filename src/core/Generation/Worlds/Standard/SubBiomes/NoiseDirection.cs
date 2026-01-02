// <copyright file="NoiseDirection.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Generation.Worlds.Standard.SubBiomes;

/// <summary>
///     The direction in which the noise-based offset is applied to the sub-biome height.
/// </summary>
public enum NoiseDirection
{
    /// <summary>
    ///     The noise-based offset is applied in both directions, up and down.
    /// </summary>
    Both,

    /// <summary>
    ///     The noise-based offset is applied only upwards, essentially using the absolute value of the noise value.
    /// </summary>
    Up,

    /// <summary>
    ///     The noise-based offset is applied only downwards, essentially using the negative absolute value of the noise value.
    /// </summary>
    Down
}
