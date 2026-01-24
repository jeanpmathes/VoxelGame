// <copyright file="IUpdateableProcess.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Updates;

/// <summary>
///     A process that can be updated and will complete at some point.
///     Should be used in combination with <see cref="UpdateDispatch" />.
/// </summary>
public interface IUpdateableProcess
{
    /// <summary>
    ///     Whether the process is currently running.
    ///     If not, it will no longer be updated by the <see cref="UpdateDispatch" />.
    /// </summary>
    Boolean IsRunning { get; }

    /// <summary>
    ///     Is called by <see cref="UpdateDispatch" /> to update the process.
    /// </summary>
    void Update();

    /// <summary>
    ///     Attempt to cancel the process.
    ///     Canceled process can either ignore the cancellation, or stop to enter a failed state.
    /// </summary>
    void Cancel();
}
