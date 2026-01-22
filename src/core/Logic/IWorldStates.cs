// <copyright file="IWorldStates.cs" company="VoxelGame">
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
using VoxelGame.Core.Updates;

namespace VoxelGame.Core.Logic;

/// <summary>
///     Gives access to the world states and allows to observe them.
/// </summary>
public interface IWorldStates
{
    /// <summary>
    ///     Whether the world is active.
    /// </summary>
    Boolean IsActive { get; }

    /// <summary>
    ///     Whether the world is terminating.
    /// </summary>
    Boolean IsTerminating { get; }

    /// <summary>
    ///     Begin terminating the world.
    /// </summary>
    /// <returns>The started activity, or <c>null</c> if the world cannot be terminated.</returns>
    Activity? BeginTerminating();

    /// <summary>
    ///     Begin saving the world.
    /// </summary>
    /// <returns>The started activity, or <c>null</c> if the world cannot be saved.</returns>
    Activity? BeginSaving();

    /// <summary>
    ///     Fired when the world enters an active state.
    ///     Note that this is also fired after saving is complete.
    /// </summary>
    event EventHandler<EventArgs>? Activating;

    /// <summary>
    ///     Fired when the world leaves an active state.
    /// </summary>
    event EventHandler<EventArgs>? Deactivating;

    /// <summary>
    ///     Fired when the world enters a terminating state.
    /// </summary>
    event EventHandler<EventArgs>? Terminating;
}
