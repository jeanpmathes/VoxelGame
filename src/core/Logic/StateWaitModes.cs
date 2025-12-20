// <copyright file="StateWaitModes.cs" company="VoxelGame">
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
using VoxelGame.Core.Updates;

namespace VoxelGame.Core.Logic;

/// <summary>
/// </summary>
[Flags]
public enum StateWaitModes
{
    /// <summary>
    ///     The state is not waiting for anything.
    ///     It will receive updates as normal.
    /// </summary>
    None = 0,

    /// <summary>
    ///     The state is waiting for the completion of an action, wrapped in a  <see cref="Future" />.
    /// </summary>
    WaitForCompletion = 1 << 0,

    /// <summary>
    ///     The state is waiting for any neighbors to become usable.
    /// </summary>
    WaitForNeighborUsability = 1 << 1,

    /// <summary>
    ///     The state is waiting for a transition request to be made.
    /// </summary>
    WaitForTransitionRequest = 1 << 2,

    /// <summary>
    ///     The state is waiting for the request level of the chunk to change.
    /// </summary>
    WaitForRequestLevelChange = 1 << 3,

    /// <summary>
    ///     The state is waiting for resources of the chunk to be released
    ///     that is currently being used by another state or operation.
    ///     This mode is used internally to wait for the resources this state needs to acquire.
    /// </summary>
    WaitForResource = 1 << 4
}
