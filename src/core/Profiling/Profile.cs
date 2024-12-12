// <copyright file="Profile.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
public partial class Profile(ProfilerConfiguration configuration)
{
    private readonly ConcurrentDictionary<String, TimingMeasurement> measurements = new();
    private readonly Dictionary<String, StateMachine> stateMachines = new();

    /// <summary>
    ///     Get the configuration of the profiler.
    /// </summary>
    public ProfilerConfiguration Configuration => configuration;

    /// <summary>
    ///     Global instance to add reports to if no local instance is available.
    /// </summary>
    public static Profile? Instance { get; private set; }

    /// <summary>
    ///     Get a profiler that is guaranteed to be active.
    ///     Can use the global instance or create a new one.
    /// </summary>
    /// <param name="full">Whether to create a full profiler if creating a new one.</param>
    /// <returns>An active profiler.</returns>
    public static Profile GetSingleUseActiveProfiler(Boolean full = false)
    {
        if (Instance != null && Instance.Configuration != ProfilerConfiguration.Disabled)
            return Instance;

        return new Profile(full ? ProfilerConfiguration.Full : ProfilerConfiguration.Basic);
    }

    /// <summary>
    ///     Create a global instance if none exists.
    /// </summary>
    public static void CreateGlobalInstance(ProfilerConfiguration configuration)
    {
        if (Instance != null) return;

        LogGlobalProfilerConfigured(logger, configuration);

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

        LogCreatingProfilerExitReport(logger);

        Property report = Instance.GenerateReport(full: true);
        String text = PropertyPrinter.Print(report);

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
    public Property GenerateReport(Boolean full = false)
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
    public void PrepareTimingMeasurement(String key, String? parent, TimingStyle style)
    {
        Boolean added = measurements.TryAdd(key, new TimingMeasurement(key, style, parent == null));

        if (added && parent != null)
            measurements[parent].AddChild(measurements[key]);
    }

    /// <summary>
    ///     Finish a timing measurement and add the duration to the measurement.
    ///     The measurement must have been prepared before.
    /// </summary>
    /// <param name="key">The unique key of the measurement.</param>
    /// <param name="duration">The duration of the measurement.</param>
    public void FinishTimingMeasurement(String key, Double duration)
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
    public void RecordStateTransition(String name, String? fromName, String? toName)
    {
        stateMachines.GetOrAdd(name, new StateMachine(name)).RecordTransition(fromName, toName);
    }

    /// <summary>
    ///     Update the state durations, which measure how many state machine steps are spent in each state by all instances in
    ///     total.
    /// </summary>
    /// <param name="name">The name of the state machine for which to update the state durations.</param>
    public void UpdateStateDurations(String name)
    {
        stateMachines.GetOrAdd(name, new StateMachine(name)).UpdateStateDurations();
    }

    /// <summary>
    ///     Record the lifetime, meaning all state transitions and the time spent in each state, of a state machine instance.
    /// </summary>
    /// <param name="name">The name of the state machine for which to record the lifetime.</param>
    /// <param name="lifetime">
    ///     The lifetime of the state machine instance.
    ///     It is represented as a sequence of states.
    /// </param>
    public void RecordStateLifetime(String name, IEnumerable<String> lifetime)
    {
        Debug.Assert(Configuration == ProfilerConfiguration.Full);

        stateMachines.GetOrAdd(name, new StateMachine(name)).RecordLifetimes(lifetime);
    }

    private sealed class TimingMeasurement(String name, TimingStyle style, Boolean isRoot)
    {
        private readonly List<TimingMeasurement> children = [];

        private readonly Lock timingLock = new();
        private readonly Lock childrenLock = new();

        private CircularTimeBuffer? reoccurring;
        private Double once;

        public Boolean IsRoot => isRoot;

        public void AddTiming(Double duration)
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

    private sealed class StateMachine(String name)
    {
        private readonly Dictionary<String, Int32> activeStates = new();
        private readonly Dictionary<String, Int64> stateDurations = new();

        private readonly List<IEnumerable<String>> lifetimes = [];

        private UInt64 totalDurations;

        public void RecordTransition(String? fromName, String? toName)
        {
            if (fromName != null)
                activeStates[fromName] = activeStates.GetValueOrDefault(fromName) - 1;

            if (toName != null)
                activeStates[toName] = activeStates.GetValueOrDefault(toName) + 1;
        }

        public void UpdateStateDurations()
        {
            foreach ((String state, Int32 count) in activeStates)
            {
                stateDurations[state] = stateDurations.GetValueOrDefault(state) + count;
                totalDurations += (UInt64) count;
            }
        }

        public void RecordLifetimes(IEnumerable<String> lifetime)
        {
            lifetimes.Add(lifetime.ToList());
        }

        private Group GenerateActiveStatesReport()
        {
            List<Property> states = [];

            foreach ((String state, Int32 count) in activeStates)
                states.Add(new Integer(state, count));

            return new Group("Active", states);
        }

        private Group GenerateStateDurationsReport()
        {
            List<Property> states = [];

            foreach ((String state, Int64 duration) in stateDurations)
                states.Add(new Message(state, $"{duration / (Double) totalDurations:P2}"));

            return new Group("Durations", states);
        }

        private Group GenerateLifetimeReport()
        {
            List<Property> content = [];

            foreach (IEnumerable<String> lifetime in lifetimes)
            {
                StringBuilder builder = new();

                foreach (String state in lifetime)
                {
                    if (builder.Length > 0) builder.Append(" -> ");

                    builder.Append(state);
                }

                content.Add(new Message("Lifetime", builder.ToString()));
            }

            return new Group("Lifetimes", content);
        }

        public Property GenerateReport(Boolean full)
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

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<Profile>();

    [LoggerMessage(EventId = Events.Profiling, Level = LogLevel.Information, Message = "Global profiler configured: {Configuration}")]
    private static partial void LogGlobalProfilerConfigured(ILogger logger, ProfilerConfiguration configuration);

    [LoggerMessage(EventId = Events.Profiling, Level = LogLevel.Information, Message = "Creating profiler exit report")]
    private static partial void LogCreatingProfilerExitReport(ILogger logger);

    #endregion LOGGING
}
