// <copyright file="StateTracker.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

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
public class StateTracker(string name, Profile? profile = null)
{
    private readonly List<string> lifetime = [];

    /// <summary>
    ///     Records a state transition.
    ///     Assumes states are represented by objects, where the object's type is the state's name.
    /// </summary>
    /// <param name="from">The previous state, or null if the state machine is just starting.</param>
    /// <param name="to">The new state, or null if the state machine is just stopping.</param>
    public void Transition(object? from, object? to)
    {
        Profile? profiler = profile ?? Profile.Instance;

        if (profiler == null) return;

        string? fromName = from?.GetType().Name;
        string? toName = to?.GetType().Name;

        profiler.RecordStateTransition(name, fromName, toName);

        if (profiler.Configuration != ProfilerConfiguration.Full)
            return;

        if (toName != null)
            lifetime.Add(toName);
        else
            profiler.RecordStateLifetime(name, lifetime);
    }
}
