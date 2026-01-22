// <copyright file="Activity.cs" company="VoxelGame">
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
using System.Collections.Generic;

namespace VoxelGame.Core.Updates;

/// <summary>
///     Represents an activity that will be completed at some point.
///     Activities are implicit workloads - only defined by their completion, which is detected on the main thread.
///     The work itself can be performed on any combination of threads, even no threads - e.g. waiting for a specific time.
/// </summary>
public class Activity
{
    private readonly List<Action> continuations = [];

    private Boolean completed;

    private Activity() {}

    /// <summary>
    ///     Create a new activity.
    /// </summary>
    /// <param name="complete">The action to invoke when the activity is completed.</param>
    /// <returns>The created activity.</returns>
    public static Activity Create(out Action complete)
    {
        Activity activity = new();

        complete = activity.Complete;

        return activity;
    }

    private void Complete()
    {
        completed = true;

        foreach (Action continuation in continuations) continuation();

        continuations.Clear();
    }

    /// <summary>
    ///     Add a continuation to the activity.
    ///     If the activity is already completed, the continuation will be invoked immediately.
    ///     Otherwise, it will be invoked when the activity is completed.
    ///     Continuations are invoked in the order they are added.
    /// </summary>
    /// <param name="continuation">The continuation to add.</param>
    /// <returns>The activity itself.</returns>
    public Activity Then(Action continuation)
    {
        if (completed) continuation();
        else continuations.Add(continuation);

        return this;
    }
}
