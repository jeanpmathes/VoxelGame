// <copyright file="Meshable.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Visuals.Meshables;

/// <summary>
///     Defines how a block is meshed.
/// </summary>
public enum Meshable
{
    /// <summary>
    ///     See <see cref="SimpleBlock" />.
    /// </summary>
    Simple,

    /// <summary>
    ///     See <see cref="ComplexBlock" />.
    /// </summary>
    Foliage,

    /// <summary>
    ///     See <see cref="PartialHeightBlock" />.
    /// </summary>
    Complex,

    /// <summary>
    ///     See <see cref="PartialHeightBlock" />.
    /// </summary>
    PartialHeight,

    /// <summary>
    ///     See <see cref="UnmeshedBlock" />.
    /// </summary>
    Unmeshed
}
