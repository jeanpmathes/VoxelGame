// <copyright file="Status.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Updates;

/// <summary>
///     Represents the status of an operation or class.
/// </summary>
public enum Status
{
    /// <summary>
    ///     The operation was created but not started yet.
    /// </summary>
    Created,

    /// <summary>
    ///     The operation is still working.
    /// </summary>
    Running,

    /// <summary>
    ///     The operation is done and was successful.
    /// </summary>
    Ok,

    /// <summary>
    ///     The operation is done and failed or cancelled.
    /// </summary>
    ErrorOrCancel
}
