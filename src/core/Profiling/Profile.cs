// <copyright file="Profile.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Collections.Properties;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Units;
using VoxelGame.Logging;

namespace VoxelGame.Core.Profiling;

/// <summary>
///     Storage for performance profiling measurements.
/// </summary>
public class Profile(ProfilerConfiguration configuration)
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<Profile>();

    private readonly ConcurrentDictionary<string, TimingMeasurement> measurements = new();
    private readonly Dictionary<string, StateMachine> stateMachines = new();

    /// <summary>
    ///     Get the configuration of the profiler.
    /// </summary>
    public ProfilerConfiguration Configuration => configuration;

    /// <summary>
    ///     Global instance to add reports to if no local instance is available.
    /// </summary>
    public static Profile? Instance { get; private set; }

    /// <summary>
    ///     Create a global instance if none exists.
    /// </summary>
    public static void CreateGlobalInstance(ProfilerConfiguration configuration)
    {
        if (Instance != null) return;

        logger.LogInformation("Global profiler configured: {Configuration}", configuration);

        if (configuration == ProfilerConfiguration.Disabled) return;

        Instance = new Profile(configuration);
    }

    /// <summary>
    ///     If a global instance with full configuration exists, create a report and write it to a file.
    /// </summary>
    public static void CreateExitReport()
    {
        if (Instance == null) return;
        if (Instance.Configuration != ProfilerConfiguration.Full) return;

        logger.LogInformation("Creating profiler exit report");

        Property report = Instance.GenerateReport(full: true);
        string text = PropertyPrinter.Print(report);

        OS.Show("Report", text);
    }

    /// <summary>
    ///     Generate a property-based report of the current performance.
    /// </summary>
    /// <param name="full">
    ///     Whether to include all available information in the report.
    ///     If true, generating the report will take longer.
    ///     Only valid if the profiler is configured to be full.
    /// </param>
    /// <returns>The report.</returns>
    public Property GenerateReport(bool full = false)
    {
        List<Property> timings = [];

        foreach (TimingMeasurement measurement in measurements.Values)
            if (measurement.IsRoot)
                timings.Add(measurement.GenerateReport());

        List<Property> states = [];

        foreach (StateMachine stateMachine in stateMachines.Values)
            states.Add(stateMachine.GenerateReport(full));

        return new Group(nameof(Profile),
        [
            new Group("Timings", timings),
            new Group("State Machines", states)
        ]);
    }

    /// <summary>
    /// Prepare a timing measurement for later completion.
    /// This can be called multiple times with the same key, but only the first call will have an effect.
    /// </summary>
    /// <param name="key">The unique key of the measurement.</param>
    /// <param name="parent">The unique key of the parent measurement, if any.</param>
    /// <param name="style">The style of the measurement.</param>
    public void PrepareTimingMeasurement(string key, string? parent, TimingStyle style)
    {
        bool added = measurements.TryAdd(key, new TimingMeasurement(key, style, parent == null));

        if (added && parent != null)
            measurements[parent].AddChild(measurements[key]);
    }

    /// <summary>
    ///     Finish a timing measurement and add the duration to the measurement.
    ///     The measurement must have been prepared before.
    /// </summary>
    /// <param name="key">The unique key of the measurement.</param>
    /// <param name="duration">The duration of the measurement.</param>
    public void FinishTimingMeasurement(string key, double duration)
    {
        measurements[key].AddTiming(duration);
    }

    /// <summary>
    ///     Records a state transition.
    ///     Method must be called from main thread.
    /// </summary>
    /// <param name="name">The name of the state machine that is profiled.</param>
    /// <param name="fromName">The previous state, or null if the state machine is just starting.</param>
    /// <param name="toName">The new state, or null if the state machine is just stopping.</param>
    public void RecordStateTransition(string name, string? fromName, string? toName)
    {
        StateMachine stateMachine = stateMachines.GetOrAdd(name, new StateMachine(name));

        if (fromName != null) stateMachine.activeStates[fromName] = stateMachine.activeStates.GetValueOrDefault(fromName) - 1;

        if (toName != null) stateMachine.activeStates[toName] = stateMachine.activeStates.GetValueOrDefault(toName) + 1;
    }

    /// <summary>
    ///     Update the state durations, which measure how many state machine steps are spent in each state by all instances in
    ///     total.
    /// </summary>
    /// <param name="name">The name of the state machine for which to update the state durations.</param>
    public void UpdateStateDurations(string name)
    {
        StateMachine stateMachine = stateMachines.GetOrAdd(name, new StateMachine(name));

        foreach ((string state, int count) in stateMachine.activeStates)
        {
            stateMachine.stateDurations[state] = stateMachine.stateDurations.GetValueOrDefault(state) + count;
            stateMachine.totalDurations += count;
        }
    }

    /// <summary>
    ///     Record the lifetime, meaning all state transitions and the time spent in each state, of a state machine instance.
    /// </summary>
    /// <param name="name">The name of the state machine for which to record the lifetime.</param>
    /// <param name="lifetime">
    ///     The lifetime of the state machine instance.
    ///     It is represented as a sequence of states.
    /// </param>
    public void RecordStateLifetime(string name, IEnumerable<string> lifetime)
    {
        Debug.Assert(Configuration == ProfilerConfiguration.Full);

        StateMachine stateMachine = stateMachines.GetOrAdd(name, new StateMachine(name));
        stateMachine.lifetimes.Add(lifetime);
    }

    private sealed class TimingMeasurement(string name, TimingStyle style, bool isRoot)
    {
        private readonly List<TimingMeasurement> children = [];

        private readonly object timingLock = new();
        private readonly object childrenLock = new();
        private CircularTimeBuffer? reoccurring;
        private double once;

        public bool IsRoot => isRoot;

        public void AddTiming(double duration)
        {
            lock (timingLock)
            {
                if (style == TimingStyle.Reoccurring && reoccurring == null)
                    reoccurring = new CircularTimeBuffer(capacity: 200);

                if (reoccurring != null)
                    reoccurring.Write(duration);
                else once = duration;
            }
        }

        public void AddChild(TimingMeasurement child)
        {
            lock (childrenLock)
            {
                children.Add(child);
            }
        }

        public Property GenerateReport()
        {
            List<Property> content = [];

            lock (timingLock)
            {
                if (reoccurring != null)
                {
                    content.Add(new Measure("Average", new Duration {Milliseconds = reoccurring.Average}));
                    content.Add(new Measure("Minimum", new Duration {Milliseconds = reoccurring.Min}));
                    content.Add(new Measure("Maximum", new Duration {Milliseconds = reoccurring.Max}));
                }
                else
                {
                    content.Add(new Measure("Time", new Duration {Milliseconds = once}));
                }
            }

            lock (childrenLock)
            {
                foreach (TimingMeasurement child in children)
                    content.Add(child.GenerateReport());
            }

            return new Group(name, content);
        }
    }

    private sealed class StateMachine(string name)
    {
        public readonly Dictionary<string, int> activeStates = new();
        public readonly Dictionary<string, long> stateDurations = new();

        public readonly List<IEnumerable<string>> lifetimes = [];

        public double totalDurations;

        private Group GenerateActiveStatesReport()
        {
            List<Property> states = [];

            foreach ((string state, int count) in activeStates) states.Add(new Integer(state, count));

            return new Group("Active", states);
        }

        private Group GenerateStateDurationsReport()
        {
            List<Property> states = [];

            foreach ((string state, long duration) in stateDurations) states.Add(new Message(state, $"{duration / totalDurations:P2}"));

            return new Group("Durations", states);
        }

        private Group GenerateLifetimeReport()
        {
            List<Property> content = [];

            foreach (IEnumerable<string> lifetime in lifetimes)
            {
                StringBuilder builder = new();

                foreach (string state in lifetime)
                {
                    if (builder.Length > 0) builder.Append(" -> ");

                    builder.Append(state);
                }

                content.Add(new Message("Lifetime", builder.ToString()));
            }

            return new Group("Lifetimes", content);
        }

        public Property GenerateReport(bool full)
        {
            List<Property> content =
            [
                GenerateActiveStatesReport(),
                GenerateStateDurationsReport()
            ];

            if (full) content.Add(GenerateLifetimeReport());

            return new Group(name, content);
        }
    }
}
