// <copyright file="Activity.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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
