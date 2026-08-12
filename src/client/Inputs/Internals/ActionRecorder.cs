// <copyright file="ActionRecorder.cs" company="VoxelGame">
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
using VoxelGame.Client.Inputs.Actions;

namespace VoxelGame.Client.Inputs.Internals;

/// <summary>
///     Collects presses and releases to compute the final state as well as the number of presses and releases since the
///     last update.
/// </summary>
internal sealed class ActionRecorder
{
    private Boolean isDown;
    private Int32 pressCount;
    private Int32 releaseCount;

    /// <summary>
    ///     Record a press.
    /// </summary>
    internal void Press()
    {
        if (isDown) return;

        isDown = true;
        pressCount++;
    }

    /// <summary>
    ///     Record a release.
    /// </summary>
    /// <returns>
    ///     <see langword="true" /> if this changed the state from held to released; otherwise, <see langword="false" />.
    /// </returns>
    internal Boolean Release()
    {
        if (!isDown) return false;

        isDown = false;
        releaseCount++;

        return true;
    }

    /// <summary>
    ///     Return the current held state and all transitions since the previous call, then clear the transition counts.
    ///     The held state remains unchanged so it can be reported again on the next action update.
    /// </summary>
    /// <returns>The current held state and the press and release counts accumulated since the previous call.</returns>
    internal ActionStateChange Consume()
    {
        ActionStateChange change = new(isDown, pressCount, releaseCount);

        pressCount = 0;
        releaseCount = 0;

        return change;
    }

    /// <summary>
    ///     Reset the held state and discard all transitions waiting for a later action update.
    /// </summary>
    internal void Discard()
    {
        isDown = false;
        pressCount = 0;
        releaseCount = 0;
    }
}
