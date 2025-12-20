// <copyright file="StateTracker.cs" company="VoxelGame">
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
using System.Collections.Generic;

namespace VoxelGame.Core.Profiling;

/// <summary>
///     Records state transitions and time spent in each state.
/// </summary>
/// <param name="name">The name of the state machine that is profiled.</param>
/// <param name="profile">
///     The profiler the state machine is associated with. If no profiler is provided, the global
///     profiler is used.
/// </param>
public class StateTracker(String name, Profile? profile = null)
{
    private readonly List<String> lifetime = [];

    /// <summary>
    ///     Records a state transition.
    ///     Assumes states are represented by objects, where the object's type is the state's name.
    /// </summary>
    /// <param name="from">The previous state, or null if the state machine is just starting.</param>
    /// <param name="to">The new state, or null if the state machine is just stopping.</param>
    public void Transition(Object? from, Object? to)
    {
        Profile? profiler = profile ?? Profile.Instance;

        if (profiler == null) return;

        String? fromName = from?.GetType().Name;
        String? toName = to?.GetType().Name;

        profiler.RecordStateTransition(name, fromName, toName);

        if (profiler.Configuration != ProfilerConfiguration.Full)
            return;

        if (toName != null)
        {
            lifetime.Add(toName);
        }
        else
        {
            profiler.RecordStateLifetime(name, lifetime);
            lifetime.Clear();
        }
    }
}
