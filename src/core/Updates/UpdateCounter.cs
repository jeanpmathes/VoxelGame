// <copyright file="UpdateCounter.cs" company="VoxelGame">
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
using System.Diagnostics;

namespace VoxelGame.Core.Updates;

/// <summary>
///     A counter for update cycles.
/// </summary>
public class UpdateCounter
{
    /// <summary>
    ///     The number of the current update cycle. It is incremented every time a new cycle begins.
    /// </summary>
    public UInt64 Current { get; private set; }

    /// <summary>
    ///     Increment the update counter.
    /// </summary>
    public void Increment()
    {
        Debug.Assert(Current < UInt64.MaxValue);

        Current++;
    }

    /// <summary>
    ///     Reset the update counter.
    /// </summary>
    public void Reset()
    {
        Current = 0;
    }
}
